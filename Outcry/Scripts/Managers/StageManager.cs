using Cinemachine;
using Cysharp.Threading.Tasks;
using StageEnums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class StageManager : Singleton<StageManager>
{
    public EStageState CurrentState { get; private set; }

    // 암전 효과를 위한 게임 오브젝트
    public List<Renderer> ColorBgs;
    public List<Renderer> BlackBgs;
    public GameObject color;
    public GameObject black;

    // 내부 변수
    private StageData currentStageData;
    public StageData CurrentStageData => currentStageData;
    private GameObject currentBoundsObject;
    private CinemachineVirtualCamera stageCamera;
    public CinemachineVirtualCamera StageCamera => stageCamera; //범위 공격 위해서 public으로 노출 필요
    private float stageTimer;
    private int aliveMonstersCount; // 살아있는 몬스터 수를 추적하는 변수

    // 로드된 프리팹 원본
    private GameObject mapPrefab;
    private GameObject playerPrefab;
    private List<GameObject> enemyPrefabs; // ID로 프리팹을 빠르게 찾기 위한 딕셔너리

    // 이벤트
    public static event Action<StageData> OnStageCleared;
    public Action<float> OnTimeChanged;

    public StageController currentStageController; // 현재 스테이지 컨트롤러 참조

    protected override void Awake()
    {
        base.Awake();
        EffectManager.Instance.ToString();
        UIManager.Instance.ToString();
        CurrentState = EStageState.None; // 대기 상태에서 시작
    }

    private void Update()
    {
        if (CurrentState != EStageState.InProgress)
        {
            return;
        }

        UpdateTimer();
    }

    private void OnEnable()
    {
        EventBus.Subscribe(EventBusKey.ChangePlayerDead, OnPlayerDiedHandler);
        EventBus.Subscribe(EventBusKey.ChangeEnemyDead, OnEnemyDiedHandler);
        
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(EventBusKey.ChangePlayerDead, OnPlayerDiedHandler);
        EventBus.Unsubscribe(EventBusKey.ChangeEnemyDead, OnEnemyDiedHandler);
    }

    protected override void OnDestroy()
    {
        if (Instance != this)
        {
            return;
        }

        base.OnDestroy();
    }

    public void InitializeStage(StageData stageData, GameObject map, GameObject player, List<GameObject> enemy)
    {
        currentStageData = stageData;
        mapPrefab = map;
        playerPrefab = player;
        enemyPrefabs = enemy; // 전달받은 모든 몬스터 정보 저장
        
        StartCoroutine(StageFlowRoutine());
    }

    /// <summary>
    /// 스테이지 데이터에 맞는 StageController를 생성하여 반환하는 팩토리 메서드
    /// </summary>
    private StageController CreateStageController(StageData data, GameObject mapInstance)
    {
        // 컨트롤러는 맵의 자식으로 생성하여 함께 관리하면 편리함
        GameObject controllerObject = new GameObject($"{data.Stage_id}_Controller");
        controllerObject.transform.SetParent(mapInstance.transform);

        switch (data.Stage_id)
        {
            case (int)EStageType.Tutorial:
                return controllerObject.AddComponent<TutorialStageController>();

            case (int)EStageType.Village:
                return controllerObject.AddComponent<LobbyVillageController>();

            case (int)EStageType.RuinsOfTheFallenKing:
                return controllerObject.AddComponent<FallenKingStageController>();

            case (int)EStageType.AbandonedMine:
                return controllerObject.AddComponent<AbandonedMineStageController>();

            case (int)EStageType.HallOfBlood:
                return controllerObject.AddComponent<HallOfBloodStageController>();

            default:
                return controllerObject.AddComponent<StageController>();
        }
    }

    public void SetTotalMonsterCount(int count)
    {
        aliveMonstersCount = count;
        Debug.Log($"[StageManager] 이번 스테이지의 총 몬스터 수는 {count}마리로 설정되었습니다.");
    }

    public float GetElapsedTime()
    {
        return currentStageData.Time_limit - stageTimer;
    }

    #region 스테이지 흐름 코루틴
    // 스테이지의 전체적인 흐름을 관리하는 메인 코루틴
    private IEnumerator StageFlowRoutine()
    {
        // 스폰 및 등장 연출
        yield return StartCoroutine(IntroRoutine());

        // 스테이지 시작
        CurrentState = EStageState.InProgress;
        stageTimer = currentStageData.Time_limit;
        //stageTimer = 10f; // TEST
        Debug.Log("스테이지 시작!");
        
        OnTimeChanged = UIManager.Instance.GetUI<HUDUI>().ChangeTimerBar;
    }

    // 스폰 및 등장 연출 코루틴
    private IEnumerator IntroRoutine()
    {
        CurrentState = EStageState.Ready;

        GameObject mapInstance = null;
        if (mapPrefab != null)
        {
            mapInstance = Instantiate(mapPrefab, Vector3.zero, Quaternion.identity);
            color = mapInstance.GetComponentInChildren<BgColor>().gameObject;
            black = mapInstance.GetComponentInChildren<BgBlack>().gameObject;

            ColorBgs = new List<Renderer>(color.GetComponentsInChildren<Renderer>(true));
            BlackBgs = new List<Renderer>(black.GetComponentsInChildren<Renderer>(true));
        }

        if (mapInstance == null)
        {
            Debug.LogError("맵 인스턴스 생성에 실패했습니다!");
            yield break;
        }

        // 생성된 맵 내부에서 스폰 지점들을 찾음
        // 맵에서 모든 스폰 포인트 정보를 미리 찾아 Dictionary로 저장 (빠른 조회용)
        //Transform playerSpawnTransform = null;
        var playerSpawnPoints = new Dictionary<int, Transform>();
        var enemySpawnPoints = new Dictionary<int, Transform>();
        var obstacleSpawnPoints = new List<Transform>();

        // 스폰 위치 정보 StageManager가 준비
        SpawnPoint[] spawnPoints = mapInstance.GetComponentsInChildren<SpawnPoint>();
        foreach (var spawnPoint in spawnPoints)
        {
            switch (spawnPoint.Type)
            {
                case ESpawnType.Player:
                    if (!playerSpawnPoints.ContainsKey(spawnPoint.SpawnIndex))
                    {
                        playerSpawnPoints.Add(spawnPoint.SpawnIndex, spawnPoint.transform);
                    }
                    else
                    {
                        Debug.LogWarning($"SpawnIndex {spawnPoint.SpawnIndex}가 중복됩니다.", spawnPoint.gameObject);
                    }
                    break;

                case ESpawnType.Enemy:
                    if (!enemySpawnPoints.ContainsKey(spawnPoint.SpawnIndex))
                    {
                        enemySpawnPoints.Add(spawnPoint.SpawnIndex, spawnPoint.transform);
                    }
                    else
                    {
                        Debug.LogWarning($"SpawnIndex {spawnPoint.SpawnIndex}가 중복됩니다.", spawnPoint.gameObject);
                    }
                    break;

                case ESpawnType.Obstacle:
                    obstacleSpawnPoints.Add(spawnPoint.transform);
                    break;
            }
        }

        // 컨트롤러 생성 및 데이터 전달
        currentStageController = CreateStageController(currentStageData, mapInstance);

        stageCamera = CameraManager.Instance.GetCurrentVirtualCamera();
        if (stageCamera == null)
        {
            Debug.LogError("CinemachineVirtualCamera가 씬에 존재하지 않습니다!");
            yield break;
        }

        if (currentStageController != null)
        {
            // 컨트롤러에게 스폰에 필요한 모든 정보 넘겨줌
            currentStageController.Initialize(this, currentStageData, playerPrefab, enemyPrefabs, playerSpawnPoints, enemySpawnPoints, stageCamera, obstacleSpawnPoints);
            currentStageController.StageSequence().Forget();
        }
        else
        {
            Debug.LogError($"ID: {currentStageData.Stage_id}에 해당하는 StageController를 찾을 수 없습니다!");
            yield break;
        }

        // 카메라 경계 설정
        var initialTilemap = mapInstance.GetComponentInChildren<Tilemap>();
        if (initialTilemap != null)
        {
            UpdateCameraBounds(initialTilemap);
        }
        //CreateCameraBounds(mapInstance, stageCamera);

        // TODO: 스테이지 시작 UI?

        // TODO: 등장 연출 동안 조작 비활성화?
        //_playerInstance.SetControllable(false);
        //_currentBoss.SetActiveAI(false);

        Debug.Log("보스 등장 연출...");
        yield return new WaitForSeconds(1.0f);
    }

    // 승리 처리 코루틴
    private IEnumerator VictoryRoutine()
    {
        CurrentState = EStageState.Finished;
        

        currentStageController?.OnStageVictory();

        if (currentStageData != null)
        {
            // StageData에 보스 ID
            // TODO: 처치한 보스 ID를 GameManager의 UserData에 추가
            OnStageCleared?.Invoke(currentStageData);
            UGSManager.Instance.LogStageResult(currentStageData.Stage_id, true, currentStageData.Time_limit - stageTimer, 0);
        }

        
        
        Debug.Log("스테이지 클리어!");
        Time.timeScale = 0.5f;
        yield return new WaitForSecondsRealtime(2.0f);
        Time.timeScale = 1.0f;

        UIManager.Instance.Show<VictoryUI>();

        if (currentStageData != null && currentStageData.Stage_id != (int)EStageType.Tutorial)
        {
            // TODO: 클리어 보상 UI(보상 중복 보유 가능 여부에 따라 달라질 예정)
            int bossId = currentStageData.Monster_ids[0];
            Sprite soulSprite = GameManager.Instance.GetSprite(bossId);

            var popup = UIManager.Instance.Show<ConfirmUI>();
            popup.Setup(new ConfirmPopupData
            {
                Title = "",
                Message = $"{currentStageData.Boss_names[0]} Vanquished.",
                Type = EConfirmPopupType.SOUL_ACQUIRE_OK,
                ItemSprite = soulSprite
            });
        }
    }

    // 패배 처리 코루틴
    private IEnumerator DefeatRoutine()
    {
        CurrentState = EStageState.Finished;

        currentStageController?.OnStageDefeat();
        
        if (currentStageData != null && currentStageController != null)
        {
            float monsterFullHp = -1f;
            float lastMonsterHp = -1f;
            if (DataManager.Instance.MonsterDataList.TryGetMonsterModelData(currentStageData.Monster_ids[0], out var monsterData))
            {
                monsterFullHp = monsterData.health;
            }
            if (currentStageController.aliveMonsters[0].TryGetComponent(out MonsterCondition condition))
            {
                lastMonsterHp = condition.CurrentHealth.CurValue();
            }

            if (monsterFullHp < 0 || lastMonsterHp < 0)
            {
                Debug.LogError("[LogStageResult] Cannot Find MonsterModelData or Alive MonsterCondition");
            }
            else
            {
                UGSManager.Instance.LogStageResult(currentStageData.Stage_id, false, currentStageData.Time_limit - stageTimer, (int)((lastMonsterHp/monsterFullHp) * 100));
            }
        }

        Debug.Log("스테이지 실패!");
        yield return new WaitForSeconds(1.0f);

        if (currentStageData.Stage_id == (int)EStageType.Tutorial)
        {
            GameManager.Instance.StartStage((int)EStageType.Tutorial);
            yield break;
        }
        
        UIManager.Instance.Show<DefeatUI>();
    }
    #endregion

    #region 카메라 경계 설정
    /// <summary>
    /// 지정된 맵 인스턴스를 기반으로 카메라 이동 범위를 설정하는 메인 메서드
    /// </summary>
    private void CreateCameraBounds(GameObject mapInstance, CinemachineVirtualCamera stageCam)
    {
        var tilemap = mapInstance.GetComponentInChildren<Tilemap>();
        tilemap.CompressBounds();

        // 기준이 될 Tilemap Renderer 찾음
        var tilemapRenderer = tilemap.GetComponent<TilemapRenderer>();
        if (tilemapRenderer == null)
        {
            Debug.LogWarning("맵에 TilemapRenderer가 없어 카메라 경계를 설정할 수 없습니다.");
            return;
        }

        // 모든 스케일이 적용된 최종 월드 경계(Bounds)를 가져옴
        Bounds worldBounds = tilemapRenderer.bounds;

        // 경계를 담을 깨끗한 게임오브젝트와 PolygonCollider2D를 새로 생성
        GameObject boundsObject = new GameObject("CameraBounds_Generated_Polygon");
        PolygonCollider2D boundingShape = boundsObject.AddComponent<PolygonCollider2D>();

        // 계산된 월드 경계(Bounds)의 네 꼭짓점 좌표 계산
        Vector2 bottomLeft = new Vector2(worldBounds.min.x, worldBounds.min.y);
        Vector2 topLeft = new Vector2(worldBounds.min.x, worldBounds.max.y);
        Vector2 topRight = new Vector2(worldBounds.max.x, worldBounds.max.y);
        Vector2 bottomRight = new Vector2(worldBounds.max.x, worldBounds.min.y);

        // 계산된 네 꼭짓점으로 폴리곤의 경로(points) 설정
        // boundsObject가 월드 원점에 있으므로 월드 좌표가 그대로 로컬 좌표가 됨
        Vector2[] points = new Vector2[] { bottomLeft, topLeft, topRight, bottomRight };
        boundingShape.points = points;
        boundingShape.isTrigger = true;

        Debug.Log($"카메라 경계(Polygon) 생성 완료. Bounds: {worldBounds}");

        // 현재 가상 카메라에 Confiner 설정
        if (stageCam != null)
        {
            SetupCameraConfiner(stageCam, boundingShape);
        }
        else
        {
            Debug.LogError("StageCamera를 찾을 수 없어 Confiner를 설정할 수 없습니다.");
            
            Destroy(boundsObject);
        }
    }

    /// <summary>
    /// 지정된 타일맵을 기준으로 카메라 경계를 생성하고 적용하는 메서드
    /// </summary>
    /// <param name="boundsTilemap">경계로 사용할 타일맵</param>
    public void UpdateCameraBounds(Tilemap boundsTilemap)
    {
        if (boundsTilemap == null)
        {
            Debug.LogWarning("경계를 설정할 타일맵이 null입니다.");
            return;
        }

        // 이전에 생성된 경계 오브젝트가 있다면 파괴
        if (currentBoundsObject != null)
        {
            Destroy(currentBoundsObject);
        }

        boundsTilemap.CompressBounds();
        var tilemapRenderer = boundsTilemap.GetComponent<TilemapRenderer>();
        if (tilemapRenderer == null)
        {
            return;
        }

        Bounds worldBounds = tilemapRenderer.bounds;

        // 경계를 담을 새 게임오브젝트 생성
        currentBoundsObject = new GameObject("CameraBounds_Generated_Polygon");
        PolygonCollider2D boundingShape = currentBoundsObject.AddComponent<PolygonCollider2D>();

        // 계산된 월드 경계(Bounds)의 네 꼭짓점 좌표 계산
        Vector2[] points = new Vector2[]
        {
            new Vector2(worldBounds.min.x, worldBounds.min.y),
            new Vector2(worldBounds.min.x, worldBounds.max.y),
            new Vector2(worldBounds.max.x, worldBounds.max.y),
            new Vector2(worldBounds.max.x, worldBounds.min.y)
        };
        boundingShape.points = points;
        boundingShape.isTrigger = true;

        // 현재 가상 카메라에 Confiner 설정
        if (stageCamera != null)
        {
            SetupCameraConfiner(stageCamera, boundingShape);
        }
    }

    /// <summary>
    /// Cinemachine 가상 카메라에 Confiner를 설정하고 경계(Collider) 연결
    /// </summary>
    private void SetupCameraConfiner(CinemachineVirtualCamera vcam, Collider2D boundingShape)
    {
        var confiner = GetOrAddComponent<CinemachineConfiner2D>(vcam.gameObject);
        confiner.m_BoundingShape2D = boundingShape;
        confiner.InvalidateCache(); // 변경사항 즉시 반영
        Debug.Log("카메라 Confiner 설정 완료!");
    }

    /// <summary>
    /// 유티틸리로 이동?
    /// 게임오브젝트에서 특정 컴포넌트를 가져오고 없으면 새로 추가하여 반환하는 헬퍼 메서드
    /// </summary>
    private T GetOrAddComponent<T>(GameObject go) where T : Component
    {
        if (!go.TryGetComponent<T>(out var component))
        {
            component = go.AddComponent<T>();
        }
        return component;
    }
    #endregion

    #region 이벤트 핸들러
    // 플레이어 사망 이벤트가 방송되면 실행될 핸들러
    private void OnPlayerDiedHandler(object data)
    {
        if ((bool)data && CurrentState == EStageState.InProgress)
        {
            Cursor.visible = true;

            StartCoroutine(DefeatRoutine());
        }
    }

    // 보스 사망 이벤트가 방송되면 실행될 핸들러
    public void OnEnemyDiedHandler(object data)
    {
        // 이벤트 데이터가 true(사망)이고 게임이 진행 중일 때만 로직 실행
        if ((bool)data && CurrentState == EStageState.InProgress)
        {
            // StageController에게 몬스터 리스트 확인 신호 보냄
            currentStageController?.CheckForDeadMonsters();

            aliveMonstersCount--; // 살아있는 몬스터 수 감소
            Debug.Log($"몬스터 사망! 남은 몬스터 수: {aliveMonstersCount}");

            // 남은 몬스터가 없으면 승리 코루틴 시작
            if (aliveMonstersCount <= 0)
            {
                Cursor.visible = true;
                StartCoroutine(VictoryRoutine());
            }
        }
    }
    #endregion

    #region 게임 플레이 로직
    private void UpdateTimer()
    {
        // 타이머가 음수이면 무한 모드 -> 타이머 업데이트 안 함
        if (stageTimer < 0)
        {
            return;
        }

        stageTimer -= Time.deltaTime;
        
        // TODO: 타이머 UI 표시?

        if (stageTimer <= 0)
        {
            // 타임오버 시 플레이어 사망 처리
            PlayerManager.Instance.player.runFSM = false; // FSM 멈추기
            OnPlayerDiedHandler(true);
        }
        else
        {
            OnTimeChanged?.Invoke(stageTimer);
        }
    }

    public void TogglePause()
    {
        if(GameManager.Instance.CurrentGameState == EGameState.Lobby)
        {
            // 로비에서 다른 팝업이 열려있을 때 옵션창이 아닌 다른 팝업이 열려있다면 모두 닫기
            if (UIManager.Instance.PopupStack.Count == 1 && UIManager.Instance.PopupStack.Peek().GetType() != typeof(OptionUI))
            {
                UIManager.Instance.CloseAllPopups();
                CursorManager.Instance.SetInGame(true);
                PlayerManager.Instance.player.PlayerInputEnable();
                return;
            }
        }

        // 게임이 진행 중일 때 -> 일시정지 상태로 변경
        if (CurrentState == EStageState.InProgress)
        {
            CurrentState = EStageState.Paused;
            Time.timeScale = 0f; // 게임 시간 정지

            var optionPopup = UIManager.Instance.Show<OptionUI>();
            if (GameManager.Instance.CurrentGameState == EGameState.Lobby || currentStageData.ID == (int)EStageType.Tutorial)
            {
                optionPopup.Setup(new OptionUIData
                {
                    Type = EOptionUIType.Lobby,
                    ExitText = "Quit Game",
                    OnClickExitAction = () =>
                    {
                        GameManager.Instance.QuitGame();
                    },
                    OnClickStageOptionExitAction = () =>
                    {
                        CurrentState = EStageState.InProgress;
                        Time.timeScale = 1f; // 게임 시간 다시 시작
                        UIManager.Instance.Hide<OptionUI>();
                        CursorManager.Instance.SetInGame(true);
                    }
                });
            }
            else // 스테이지 플레이 중일 때는 로비로 이동
            {
                optionPopup.Setup(new OptionUIData
                {
                    Type = EOptionUIType.Stage,
                    ExitText = "Back to Lobby",
                    OnClickExitAction = () =>
                    {
                        GameManager.Instance.GoToLobby();
                    },
                    OnClickStageOptionExitAction = () =>
                    {
                        CurrentState = EStageState.InProgress;
                        Time.timeScale = 1f; // 게임 시간 다시 시작
                        UIManager.Instance.Hide<OptionUI>();
                        CursorManager.Instance.SetInGame(true);
                    }
                });
            }

            CursorManager.Instance.SetInGame(false);
        }
        // 일시정지 상태일 때 -> 게임 진행 상태로 변경
        else if (CurrentState == EStageState.Paused)
        {
            CurrentState = EStageState.InProgress;
            Time.timeScale = 1f; // 게임 시간 다시 시작

            UIManager.Instance.CloseAllPopups();

            CursorManager.Instance.SetInGame(true);
        }
    }
    #endregion
}
