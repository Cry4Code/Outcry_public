using Cinemachine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HallOfBloodStageController : StageController
{
    public WayPoint[] WayPoints;

    public ZoneMarker Phase3BossZone;
    
    // 보스 페이즈 관리용
    private enum EBossPhase { Phase1, TransitionToPhase2, Phase2, TransitionToPhase3, Phase3 }
    private EBossPhase currentPhase;

    [Header("테스트 옵션")]
    [Tooltip("체크하면 보스 HP와 상관없이 페이즈 2 시작")]
    public bool StartPhase2Trigger = false;
    private bool testTriggered = false; // 테스트가 중복 실행되지 않도록 방지

    // 페이즈별 오브젝트
    private GameObject phase1Map;
    private GameObject phase2Map;
    private GameObject phase2And3Platforms;
    public GameObject BloodMoon { get; private set; }

    private CinemachineBrain cinemachineBrain;
    private CinemachineVirtualCamera playerFollowCamera;
    private CinemachineVirtualCamera autoScrollCamera;
    private CameraAutoScroller cameraScroller;
    private GameObject phase3StartTrigger; // 3페이즈 시작용 트리거 오브젝트
    private TriggerForwarder phase3TriggerForwarder; // 트리거 이벤트 전달용 컴포넌트
    private GameObject phase3DeathPlane;

    // 페이즈 별 카메라 경계 타일맵
    private Tilemap phase2BoundsTilemap;
    private Tilemap phase3BoundsTilemap;

    private Transform phase2PlayerSpawnPoint;
    private Transform phase2MonsterSpawnPoint;

    private GameObject walkingBossPrefab;
    private GameObject flyingBossPrefab;

    private MonsterBase bossMonster;

    private void Awake()
    {
        Transform mapRoot = transform.parent;
        if (mapRoot == null)
        {
            Debug.LogError("StageController는 반드시 맵 프리팹의 자식으로 배치되어야 합니다!");
            return;
        }

        // 오브젝트 캐싱
        phase1Map = mapRoot.GetComponentInChildren<Phase1MapMarker>(true)?.gameObject;
        phase2Map = mapRoot.GetComponentInChildren<Phase2MapMarker>(true)?.gameObject;
        phase2And3Platforms = mapRoot.GetComponentInChildren<Phase2PlatformsMarker>(true)?.gameObject;
        BloodMoon = mapRoot.GetComponentInChildren<BloodMoonMarker>(true)?.gameObject;
        autoScrollCamera = mapRoot.GetComponentInChildren<AutoScrollCameraMarker>(true)?.GetComponent<CinemachineVirtualCamera>();
        cameraScroller = mapRoot.GetComponentInChildren<CameraAutoScroller>(true);
        WayPoints = mapRoot.GetComponentsInChildren<WayPoint>(true);
        Phase3BossZone = mapRoot.GetComponentInChildren<ZoneMarker>(true);
        phase3StartTrigger = mapRoot.GetComponentInChildren<Phase3StartTriggerMarker>(true)?.gameObject;
        if (phase3StartTrigger != null)
        {
            phase3TriggerForwarder = phase3StartTrigger.GetComponent<TriggerForwarder>();
        }
        phase3DeathPlane = mapRoot.GetComponentInChildren<Phase3DeathPlaneMarker>(true)?.gameObject;

        // 각 페이즈의 카메라 경계 타일맵
        phase2BoundsTilemap = mapRoot.GetComponentInChildren<Phase2CamBoundsMarker>(true)?.GetComponent<Tilemap>();
        phase3BoundsTilemap = mapRoot.GetComponentInChildren<Phase3CamBoundsMarker>(true)?.GetComponent<Tilemap>();

        var allSpawnPoints = mapRoot.GetComponentsInChildren<SpawnPoint>(true);
        phase2PlayerSpawnPoint = allSpawnPoints.FirstOrDefault(p => p.Type == StageEnums.ESpawnType.Player && p.SpawnIndex == 1)?.transform;
        phase2MonsterSpawnPoint = allSpawnPoints.FirstOrDefault(p => p.Type == StageEnums.ESpawnType.Enemy && p.SpawnIndex == 1)?.transform;
    }

    // 테스트용 트리거 감지
    private void Update()
    {
        if (!testTriggered)
        {
            testTriggered = true; // 중복 실행 방지
            Debug.Log("테스트 옵션으로 페이즈 2를 시작합니다.");
            // StageSequence의 WaitUntil이 이 값을 감지하고 다음 단계로 진행합니다.
        }
    }

    public override void Initialize(StageManager manager, StageData data, GameObject player, List<GameObject> enemys, Dictionary<int, Transform> playerSpawns, Dictionary<int, Transform> enemySpawns, CinemachineVirtualCamera vcam, List<Transform> obstacleSpawns)
    {
        base.Initialize(manager, data, player, enemys, playerSpawns, enemySpawns, vcam, obstacleSpawns);

        playerFollowCamera = stageCamera;

        if (Camera.main != null)
        {
            cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        }

        if (enemys != null && enemys.Count >= 2)
        {
            walkingBossPrefab = enemys[0];
            flyingBossPrefab = enemys[1];
        }
        else
        {
            Debug.LogError("보스 프리팹이 충분하지 않습니다! (걷는 보스, 나는 보스 총 2개가 필요합니다.)");
        }

        if (phase1Map != null) phase1Map.SetActive(true);
        if (phase2Map != null) phase2Map.SetActive(false);
        if (phase2And3Platforms != null) phase2And3Platforms.SetActive(false);
        if (phase3StartTrigger != null) phase3StartTrigger.SetActive(false);
        if (autoScrollCamera != null) autoScrollCamera.gameObject.SetActive(false);
        if (cameraScroller != null) cameraScroller.gameObject.SetActive(false);
    }

    public override async UniTask StageSequence()
    {
        await Phase1_Sequence();

        // 보스 HP가 50% 이하가 되거나 테스트 옵션이 켜질 때까지 대기
        await UniTask.WaitUntil(() =>
            StartPhase2Trigger,
            cancellationToken: this.GetCancellationTokenOnDestroy());

        if (this.GetCancellationTokenOnDestroy().IsCancellationRequested)
        {
            return;
        }

        // 페이즈 2 전환
        await TransitionToPhase2();

        // 페이즈 2
        await Phase2_Sequence();

        // 플레이어가 마지막 발판 트리거에 닿을 때까지 대기
        await UniTask.WaitUntil(() => currentPhase == EBossPhase.TransitionToPhase3, cancellationToken: this.GetCancellationTokenOnDestroy());

        if (this.GetCancellationTokenOnDestroy().IsCancellationRequested)
        {
            return;
        }

        // 페이즈 3
        await Phase3_Sequence();
    }

    #region 페이즈별 로직
    private async UniTask Phase1_Sequence()
    {
        currentPhase = EBossPhase.Phase1;
        Debug.Log("페이즈 1 시작");

        SpawnPlayerAt(playerSpawnPoints[0], 3f);

        await AudioManager.Instance.PlayBGM((int)SoundEnums.EBGM.HallOfBloodPhase1);

        int walkingBossId = stageData.Monster_ids[0];
        Transform phase1SpawnPoint = enemySpawnPoints[0];
        GameObject bossInstance = SpawnMonsterAt(walkingBossPrefab, walkingBossId, phase1SpawnPoint);
        SettingBossHpBar();

        // 인게임 마우스 커서가 먼저 나오지 않게 조절
        await UniTask.Yield(PlayerLoopTiming.Update);
        await UniTask.Yield(PlayerLoopTiming.Update);

        InitializeInGameCursor();

        if (bossInstance != null)
        {
            bossMonster = bossInstance.GetComponent<MonsterBase>();
        }
    }

    private async UniTask TransitionToPhase2()
    {
        currentPhase = EBossPhase.TransitionToPhase2;
        Debug.Log("페이즈 2로 전환 시작");

        // 페이즈 전환 효과음
        EffectManager.Instance.PlayEffectsByIdAsync(1035100, EffectOrder.SpecialEffect).Forget();

        // 화면 진동 효과
        EffectManager.Instance.PlayEffectByIdAndTypeAsync(9900, EffectType.Camera).Forget();

        await FadeManager.Instance.FadeOut();

        // 인게임 커서 숨기기, 사용자 입력 막기
        CursorManager.Instance.SetInGame(false);
        PlayerManager.Instance.player.PlayerInputDisable();

        // 카메라 경계를 페이즈 2용으로 업데이트
        if (stageManager != null && phase2BoundsTilemap != null)
        {
            stageManager.UpdateCameraBounds(phase2BoundsTilemap);
        }

        // playerFollowCamera -> autoScrollCamera 전환에만 적용되도록 하는 조건
        CinemachineCore.GetBlendOverride = (from, to, defaultBlend, owner) =>
        {
            if (from == playerFollowCamera && to == autoScrollCamera)
            {
                return new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.Cut, 0);
            }
            return defaultBlend; // 그 외의 경우는 기본 블렌드 사용
        };

        // 카메라 우선순위 변경하여 전환 트리거
        ActivatePhase2Camera();

        // 다음 프레임까지 기다림. 이 프레임 동안 CinemachineBrain이 컷 전환 처리
        await UniTask.NextFrame(this.GetCancellationTokenOnDestroy());

        // 전환이 완료되었으므로 오버라이드 규칙을 즉시 제거하여 다른 전환에 영향을 주지 않도록 한다
        CinemachineCore.GetBlendOverride = null;

        // 이전 보스 제거
        int prevHP = bossMonster.Condition.CurrentHealth.CurValue();
        if (bossMonster != null)
        {
            StageManager.Instance.OnEnemyDiedHandler(true);
            aliveMonsters.Remove(bossMonster.gameObject);
            Debug.Log("페이즈 1 보스를 제거했습니다.");
        }

        if (phase1Map != null) phase1Map.SetActive(false);
        if (phase2Map != null) phase2Map.SetActive(true);
        if (phase3DeathPlane != null) phase3DeathPlane.SetActive(false);
        if (phase2And3Platforms != null) phase2And3Platforms.SetActive(true);
        if (phase3StartTrigger != null) phase3StartTrigger.SetActive(true);

        Vector3 playerNewPos = phase2PlayerSpawnPoint.position;
        playerInstance.transform.position = playerNewPos;
        PlayerManager.Instance.player.runFSM = false;

        int flyingBossId = stageData.Monster_ids[1];
        GameObject newBossInstance = SpawnMonsterAt(flyingBossPrefab, flyingBossId, phase2MonsterSpawnPoint);
        if (newBossInstance != null)
        {
            bossMonster = newBossInstance.GetComponent<MonsterBase>();
            UniTask.DelayFrame(1).ContinueWith(() =>
            {
                SettingBossHpBar();
                ChangeBossHp(bossMonster, prevHP);
            }).Forget();
        }

        // 사용자 입력 활성화
        PlayerManager.Instance.player.PlayerInputEnable();

        // 다음 프레임까지 기다려서 마우스 좌표가 갱신될 시간 준다
        await UniTask.NextFrame(cancellationToken: this.GetCancellationTokenOnDestroy());

        // 갱신된 좌표를 바탕으로 커서를 안전하게 표시
        CursorManager.Instance.SetInGame(true);

        await FadeManager.Instance.FadeIn();

        playerFollowCamera.gameObject.SetActive(true);
    }

    private void ActivatePhase2Camera()
    {
        autoScrollCamera.gameObject.SetActive(true);
        cameraScroller.gameObject.SetActive(true);
        autoScrollCamera.Priority = 20;
        playerFollowCamera.Priority = 10;
    }

    private async UniTask Phase2_Sequence()
    {
        currentPhase = EBossPhase.Phase2;
        Debug.Log("페이즈 2 시작");

        await AudioManager.Instance.PlayBGM((int)SoundEnums.EBGM.HallOfBloodPhase2);

        PlayerManager.Instance.player.runFSM = true;

        cameraScroller.StartScroll();

        phase3TriggerForwarder.OnTriggerEnter_2D += OnPhase3TriggerEntered;

        // UniTask.Yield()는 async 메서드에서 반환값이 없을 때 필요
        await UniTask.Yield();
    }

    private void OnPhase3TriggerEntered(Collider2D other)
    {
        if (other.CompareTag("CameraScrollTarget"))
        {
            Debug.Log("페이즈 3으로 전환합니다.");
            currentPhase = EBossPhase.TransitionToPhase3;
            phase3TriggerForwarder.OnTriggerEnter_2D -= OnPhase3TriggerEntered;
            phase3StartTrigger.SetActive(false);
        }
    }

    private async UniTask Phase3_Sequence()
    {
        currentPhase = EBossPhase.Phase3;
        
        //VampireLordFlyingAI로 페이즈 3 전환 알림
        var vampireLordFlyingAI = bossMonster.MonsterAI as VampireLordFlyingAI;
        if (vampireLordFlyingAI)
        {
            vampireLordFlyingAI.TransitionToPhase3();
        }
            
        Debug.Log("페이즈 3 시작");

        cameraScroller.StopScroll();
        if (phase3StartTrigger != null)
        {
            phase3StartTrigger.SetActive(false);
        }

        // 카메라 경계를 페이즈 3용으로 업데이트
        if (stageManager != null && phase3BoundsTilemap != null)
        {
            stageManager.UpdateCameraBounds(phase3BoundsTilemap);
        }

        // 플레이어 추적 카메라로 다시 전환하고 시점 고정
        playerFollowCamera.Priority = 20;
        autoScrollCamera.Priority = 10;

        autoScrollCamera.gameObject.SetActive(false);
        cameraScroller.gameObject.SetActive(false);

        if (phase3DeathPlane != null)
        {
            phase3DeathPlane.SetActive(true);
        }

        await UniTask.Yield();
    }
    #endregion
}
