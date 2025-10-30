using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatStormSkillSequenceNode : SkillSequenceNode
{
    private const int BAT_COUNT = 20;
    private const float MIN_BAT_SPEED = 4f;
    private const float MAX_BAT_SPEED = 20f;
    private const float HOMING_CUTOFF_DISTANCE = 3f;
    private const float SPAWN_RADIUS = 4f;
    private const float PRE_LAUNCH_DELAY = 0.5f;
    private const float POST_LAUNCH_DELAY = 5f;
    private const float ASCEND_HEIGHT = 8.0f; // 몬스터가 떠오를 높이
    private const float ASCEND_SPEED = 6.0f;  // 몬스터가 떠오르는 속도

    // 상태 관리 플래그
    private bool isAscending;
    private bool areBatsSpawned;
    private bool areBatsLaunched;
    private float launchTimer;
    private bool isWaitingForTransition;
    private float transitionTimer;
    private float originalGravityScale;
    private Vector2 ascendTargetPosition;
    private bool isChargeTriggered;
    private bool isWarningUITriggerd = false;

    private List<GameObject> spawnedBats = new List<GameObject>();
    private HallOfBloodStageController stageController;

    public BatStormSkillSequenceNode(int skillId) : base(skillId)
    {
        this.nodeName = "BatStormSkillSequenceNode";
    }

    public override async void InitializeSkillSequenceNode(MonsterBase monster, PlayerController target)
    {
        this.monster = monster;
        this.target = target;

        // 몬스터 기존 중력 스케일 저장
        if (monster.Rb2D != null)
        {
            originalGravityScale = monster.Rb2D.gravityScale;
        }

        // 스테이지 컨트롤러 캐싱 (페이즈 전환용)
        stageController = StageManager.Instance.currentStageController as HallOfBloodStageController;
        if (stageController == null)
        {
            Debug.LogError("BatStormSkillSequenceNode는 HallOfBloodStageController 환경에서만 동작합니다.");
        }

        // 행동 노드 정의
        ConditionNode canPerform = new ConditionNode(CanPerform);
        ActionNode warningAction = new ActionNode(WarningAction);
        ActionNode ascendAction = new ActionNode(AscendAction); // 공중으로 떠오르기
        ActionNode chargeAction = new ActionNode(ChargeAction); // 차지 액션
        ActionNode spawnBatsAction = new ActionNode(SpawnBatsAction); // 박쥐 소환
        ActionNode launchBatsAction = new ActionNode(SkillAction); // 박쥐 발사 (실질적인 스킬 액션)
        ActionNode waitAction = new ActionNode(WaitAndTransitionAction); // 대기 후 페이즈 전환

        // 노드 이름 설정 (디버깅용)
        canPerform.nodeName = "CanPerform";
        ascendAction.nodeName = "AscendAction";
        chargeAction.nodeName = "ChargeAction";
        spawnBatsAction.nodeName = "SpawnBatsAction";
        launchBatsAction.nodeName = "LaunchBatsAction";
        waitAction.nodeName = "WaitAndTransitionAction";

        children.Clear();
        AddChild(canPerform);
        AddChild(warningAction);
        AddChild(ascendAction);
        AddChild(chargeAction);
        AddChild(spawnBatsAction);
        AddChild(launchBatsAction);
        AddChild(waitAction);

        nodeName = skillData.skillName + skillData.skillId;

        // 박쥐 오브젝트 풀 미리 등록
        await ObjectPoolManager.Instance.RegisterPoolAsync(AddressablePaths.Projectile.Bat, BAT_COUNT, BAT_COUNT);
    }

    protected override bool CanPerform()
    {
        // TODO: 배트스톰 테스트용
        if (TestManager.Instance.triggerForBatStorm)
        {
            Debug.LogWarning("[TestManager] BatStorm 스킬 강제 발동!");
            TestManager.Instance.triggerForBatStorm = false;

            skillTriggered = false;
            //스킬 실행용 플래그들 초기화
            ResetSkillFlags();

            return true;
        }

        // 이미 스킬을 한 번 사용했다면 실행하지 않음
        if (skillTriggered)
        {
            return false;
        }

        // 보스 체력이 70% 이하일 때 발동
        bool isLowHealth = monster.Condition.CurrentHealth.CurValue() <= skillData.triggerHealth * monster.Condition.MaxHealth;

        if (isLowHealth)
        {
            Debug.Log($"[스킬 조건 충족] {skillData.skillName} (ID: {skillData.skillId}) | 현재 체력: {monster.Condition.CurrentHealth} / {monster.Condition.MaxHealth}");

            // 스킬 실행에 필요한 모든 상태 플래그 초기화
            ResetSkillFlags();
            return true;
        }

        return false;
    }

    private NodeState WarningAction()
    {
        if (!isWarningUITriggerd) //아직 경고 UI 시작 전.
        {
            // 경고창 띄우기
            EffectManager.Instance.PlayEffectByIdAndTypeAsync(103010, EffectType.ScreenUI).Forget();
            isWarningUITriggerd = true;
            return NodeState.Running;
        }

        if (EffectManager.Instance.IsEffectPlaying(103010, EffectType.ScreenUI))
        {
            // 경고창이 재생되는 동안 대기
            return NodeState.Running;
        }

        Debug.Log($"[몬스터] {skillData.skillName} (ID: {skillData.skillId}) WarningAction Done!");
        return NodeState.Success;
    }

    /// <summary>
    /// 공중으로 떠오르는 액션
    /// </summary>
    private NodeState AscendAction()
    {
        if (!isAscending)
        {
            // 스킬 시작 처리
            skillTriggered = true;
            isAscending = true;
            monster.Condition.SetInivincible(true);

            // 중력 제거 및 목표 위치 설정
            if (monster.Rb2D != null)
            {
                monster.Rb2D.gravityScale = 0f;
                monster.Rb2D.velocity = Vector2.zero; // 혹시 모를 기존 속도 제거
            }
            ascendTargetPosition = (Vector2)monster.transform.position + Vector2.up * ASCEND_HEIGHT;

            // 전용 스킬 효과음 출력 (ID는 기획에 맞게 설정 필요)
            //EffectManager.Instance.PlayEffectByIdAndTypeAsync(skillData.skillId, EffectType.Sound);

            // 인트로(상승) 애니메이션 실행
            monster.Animator.SetBool(AnimatorHash.MonsterParameter.IsFlying, true); // 공중 상태 시작
            monster.Animator.SetTrigger(AnimatorHash.MonsterParameter.BatStorm); // 상승 애니메이션 시작
        }

        // 목표 높이에 도달할 때까지 몬스터 위로 이동
        if (Vector2.Distance(monster.transform.position, ascendTargetPosition) > 0.01f)
        {
            Vector2 newPosition = Vector2.MoveTowards(monster.transform.position, ascendTargetPosition, ASCEND_SPEED * Time.deltaTime);
            monster.Rb2D.MovePosition(newPosition);
            return NodeState.Running;
        }

        // 목표 높이에 도달했는지 매 프레임 확인
        bool hasArrived = Vector2.Distance(monster.transform.position, ascendTargetPosition) < 0.01f;
        if (!hasArrived)
        {
            // 목표 지점을 향해 부드럽게 이동
            Vector2 newPosition = Vector2.MoveTowards(
                monster.transform.position,
                ascendTargetPosition,
                ASCEND_SPEED * Time.deltaTime
            );
            monster.Rb2D.MovePosition(newPosition);

            // 아직 행동이 진행 중이므로 Running 상태 반환
            return NodeState.Running;
        }

        monster.Animator.SetTrigger(AnimatorHash.MonsterParameter.BatStormFlying);

        Debug.Log($"[{skillData.skillName}] 공중 부양 완료");

        return NodeState.Success;
    }

    private NodeState ChargeAction()
    {
        // 차지 애니메이션 시작
        if (!isChargeTriggered)
        {
            isChargeTriggered = true;
            monster.Animator.SetTrigger(AnimatorHash.MonsterParameter.ChargeStart);
            return NodeState.Running;
        }

        // 차지 애니메이션이 끝날 때까지 대기
        if (AnimatorUtility.IsAnimationPlaying(monster.Animator, AnimatorHash.MonsterAnimation.ChargeStart))
        {
            return NodeState.Running;
        }

        Debug.Log($"[{skillData.skillName}] 차지 완료. 박쥐를 소환합니다.");
        return NodeState.Success;
    }

    /// <summary>
    /// 박쥐 소환 액션
    /// </summary>
    private NodeState SpawnBatsAction()
    {
        if (!areBatsSpawned)
        {
            areBatsSpawned = true;
            spawnedBats.Clear();

            Debug.Log($"[{skillData.skillName}] 박쥐 {BAT_COUNT}마리 소환 시작");

            for (int i = 0; i < BAT_COUNT; i++)
            {
                // 보스 주변에 원형으로 위치 계산
                float angle = i * (360f / BAT_COUNT);
                float radians = angle * Mathf.Deg2Rad;
                Vector2 spawnPosition = (Vector2)monster.transform.position + new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * SPAWN_RADIUS;

                // 오브젝트 풀에서 박쥐 가져오기
                GameObject bat = ObjectPoolManager.Instance.GetObject(AddressablePaths.Projectile.Bat, null, spawnPosition);
                if (bat != null)
                {
                    // 생성된 박쥐의 부모를 보스 몬스터로 설정
                    bat.transform.SetParent(monster.transform);
                    spawnedBats.Add(bat);
                }
            }
        }

        return NodeState.Success;
    }

    /// <summary>
    /// 박쥐 발사 액션 (실질적인 스킬 로직)
    /// </summary>
    protected override NodeState SkillAction()
    {
        if (!areBatsLaunched)
        {
            // 발사 시작 시간 기록 (0.5초 딜레이용)
            areBatsLaunched = true;
            launchTimer = Time.time;
            return NodeState.Running;
        }

        // 0.5초 대기
        if (Time.time - launchTimer < PRE_LAUNCH_DELAY)
        {
            return NodeState.Running;
        }

        // 대기 시간이 끝나면 박쥐 발사(이 로직은 한 번만 실행)
        if (spawnedBats.Count > 0)
        {
            Debug.Log($"[{skillData.skillName}] 박쥐 발사!");

            // 카메라 효과 적용
            //EffectManager.Instance.PlayEffectByIdAndTypeAsync(skillData.skillId, EffectType.Camera);

            foreach (var batObject in spawnedBats)
            {
                BatProjectileController batController = batObject.GetComponent<BatProjectileController>();
                if (batController != null)
                {
                    float randomSpeed = Random.Range(MIN_BAT_SPEED, MAX_BAT_SPEED);
                    batController.Launch(target, randomSpeed, HOMING_CUTOFF_DISTANCE, skillData.damage1);
                    // TODO: 발사 효과음 출력 (ID는 기획에 맞게 설정 필요)
                }
            }
            spawnedBats.Clear(); // 발사 후 목록 비우기
            EffectManager.Instance.PlayEffectsByIdAsync(skillData.skillId, EffectOrder.SpecialEffect).Forget();
        }

        // 보스 데미지 처리
        monster.Condition.SetInivincible(false);

        return NodeState.Success;
    }

    /// <summary>
    /// 5초 대기 후 페이즈 전환 신호 보내기
    /// </summary>
    private NodeState WaitAndTransitionAction()
    {
        if (!isWaitingForTransition)
        {
            isWaitingForTransition = true;
            transitionTimer = Time.time;
            Debug.Log($"[{skillData.skillName}] 스킬 종료. {POST_LAUNCH_DELAY}초 후 페이즈 2로 전환합니다.");
            return NodeState.Running;
        }

        if (Time.time - transitionTimer < POST_LAUNCH_DELAY)
        {
            return NodeState.Running;
        }

        monster.Condition.SetInivincible(true);

        // 5초가 지나면 페이즈 전환 신호 보내기
        if (stageController != null)
        {
            Debug.Log("페이즈 2 전환 신호 전송!");
            stageController.StartPhase2Trigger = true;
        }

        // TODO: 스킬 종료 후 보스 비활성화?
        if (monster != null)
        {
            monster.gameObject.SetActive(false);
        }

        return NodeState.Success;
    }

    private void ResetSkillFlags()
    {
        isAscending = false;
        areBatsSpawned = false;
        areBatsLaunched = false;
        isWaitingForTransition = false;
    }
}
