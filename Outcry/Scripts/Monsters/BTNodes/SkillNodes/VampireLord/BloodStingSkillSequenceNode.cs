using UnityEngine;

public class BloodStingSkillSequenceNode : SkillSequenceNode
{
    private float stateEnterTime; // 스킬(상태)에 진입한 시간
    [SerializeField] 
    private float cooldownTimer = 0f; // 쿨다운 계산을 위한 타이머
    //애니메이터 추가
    private Animator animator;
    // 상수
    private const float MOVE_SPEED = 60f;   // 이동 속도
    private const float ALLOW_GAP = 1.0f;
    private const float FORCED_EXIT_TIME = 2f;

    private float targetPosX;
    private bool isArrived = false;
    
    public BloodStingSkillSequenceNode(int skillId) : base(skillId)
    {
    }

    public override void InitializeSkillSequenceNode(MonsterBase monster, PlayerController target)
    {
        base.InitializeSkillSequenceNode(monster, target);
        this.nodeName = "BloodStingSkillSequenceNode";
        animator = monster.Animator;
                
        // 게임 시작 시 바로 스킬을 사용할 수 있도록 쿨다운을 초기화
        if (skillData != null)
        {
            cooldownTimer = skillData.cooldown;
        }
    }

    protected override bool CanPerform() // 이해 완료
    {
        // 포션-오버라이드 발동 시, 평소 조건 무시하고 즉시 허가
        if (monster.MonsterAI.blackBoard.PotionOverrideEdge)
            return true;

        // 쿨다운이 다 차지 않았을 때만 시간 더함
        if (cooldownTimer < skillData.cooldown)
        {
            cooldownTimer += Time.deltaTime;
        }

        // 플레이어와의 거리 확인 (거리가  이상일때만 사용) 황상욱
        bool isInRange = Vector2.Distance(monster.transform.position, target.transform.position) >= skillData.range;

        // 쿨다운 확인
        bool isCooldownComplete = (cooldownTimer >= skillData.cooldown);
        
        // 쿨다운이 차지 않았을 때 and 플레이어가 일정 거리 이상일 때
        return isInRange && isCooldownComplete;
    }

    protected override NodeState SkillAction()
    {
        NodeState state;

        // SkillAction() 이 불렸을 때 이미 실행중이 아니면
        if (!skillTriggered)
        {
            monster.MonsterAI.TryConsumePotionEdge(); // 포션 엣지-레치 소모

            targetPosX = target.transform.position.x; // 타겟 x 좌표 초기화
            // 몬스터를 기준으로 플레이어가 어느 방향에 있는지 계산
            float directionToTarget = Mathf.Sign(targetPosX - monster.transform.position.x);
            // 스킬 시작할 때 플레이어를 바라보게 만듦
            // Mathf.Abs를 사용하여 기존 스케일의 크기 유지
            monster.transform.localScale = new Vector3(
                Mathf.Abs(monster.transform.localScale.x) * directionToTarget,
                monster.transform.localScale.y,
                monster.transform.localScale.z
            );

            animator.SetTrigger(AnimatorHash.MonsterParameter.BloodSting); // 돌진 시작 애니메이션 트리거 on
            monster.AttackController.SetDamages(skillData.damage1); // MonsterAttackController에서 OnTriggerStay2D 안에서 데미지를 모두 관리

            // 상태 초기화 및 애니메이션 시작 시간 기록            
            skillTriggered = true;
            stateEnterTime = Time.time;
            cooldownTimer = 0f; // 스킬을 사용했으므로 쿨다운 타이머 리셋
            return NodeState.Running;
        }

        // 애니메이션 출력 보장
        if (!isAnimationStarted)
        {
            isAnimationStarted = AnimatorUtility.IsAnimationStarted(monster.Animator, AnimatorHash.MonsterAnimation.BloodStingStart);
            return NodeState.Running;
        }


        // 각 애니메이션 클립에 실행될 로직들
        // start 애니메이션이 실행되는 동안 실행중 반환
        if (AnimatorUtility.IsAnimationPlaying(monster.Animator, AnimatorHash.MonsterAnimation.BloodStingStart))
        {
            Debug.Log($"Running skill: {skillData.skillName} (ID: {skillData.skillId})");
            state = NodeState.Running;
        }
        //루프 애니메이션이 실행되는 동안 이동과 실행중 반환, 도착할 시 파라미터 설정
        else if (AnimatorUtility.IsAnimationPlaying(monster.Animator, AnimatorHash.MonsterAnimation.BloodStingLoop))
        {

            if (Mathf.Abs( targetPosX - monster.Rb2D.position.x) > ALLOW_GAP)
            {
                //이동 로직 구현
                float direction = Mathf.Sign(monster.transform.localScale.x);
                // Vector3.right를 사용하여 월드 좌표계의 오른쪽 방향을 기준으로 이동
                // direction 값에 따라 왼쪽 또는 오른쪽으로 움직임
                Vector2 moveDir = Vector2.right * direction;
                Vector2 newPosition = monster.Rb2D.position + moveDir * (MOVE_SPEED * Time.fixedDeltaTime);
                monster.Rb2D.MovePosition(newPosition);
            }
            else
            {
                isArrived = true; // isArrived 라는 bool 값을 이동이 끝났을 때 true 로 바꿔줘서 애니메이션이 끝났는지 다른곳에서 확인할 수 있게 해줌
                animator.SetTrigger(AnimatorHash.MonsterParameter.IsArrived);
            }

            //일정 시간 이상 추적 시 강제 종료
            if (Time.time - stateEnterTime > FORCED_EXIT_TIME)
            {
                animator.SetTrigger(AnimatorHash.MonsterParameter.IsArrived);
                FieldReset();
                return NodeState.Success;

            }
            state = NodeState.Running;
        }
        //isArrived 를 통해서 이전 모든 애니메이션이 실행 된 상태에서, 엔드 애니메이션 까지 실행중이 아니면 == 모든 애니메이션이 실행됨 => 필드를 초기화 하고 성공 반환 
        else if (!AnimatorUtility.IsAnimationPlaying(monster.Animator, AnimatorHash.MonsterAnimation.BloodStingEnd) && isArrived)
        {
            FieldReset();
            state = NodeState.Success;
        }
        else if (IsIdleAnimationPlaying())
        {
            // 스턴 등의 이유로 Idle 애니메이션으로 복귀했을 때 필드 초기화 후 성공 반환
            Debug.Log("[몬스터BT] HeavyDestroyerSkillSequenceNode: 스킬 도중 Idle 애니메이션으로 복귀 감지");
            FieldReset();
            state = NodeState.Success;
        }
        else
        {
            state = NodeState.Running;
        }

        return state;

    }
    
    private void FieldReset()
    {
        skillTriggered = false; // 다음 스킬 사용을 위해 플래그 리셋
        monster.AttackController.SetDamages(0); //데미지 초기화
        isArrived = false; // 다음 스킬 사용을 위해 플래그 리셋
    }
}
