
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class ThreePointSkillSequenceNode : SkillSequenceNode
{
    private Animator animator;
    
    // 애니메이션 클립 초당 프레임 수
    private const float ANIMATION_FRAME_RATE = 20f;

    private float[] attackSoundTime = new[]
    {
        (1f / ANIMATION_FRAME_RATE) * 9f,
        (1f / ANIMATION_FRAME_RATE) * 18f,
        (1f / ANIMATION_FRAME_RATE) * 33f,
    };

    private int attackSoundIndex = 0;
    private float elapsedTime = 0;

    // private const float ANIMATION_FRAME_RATE = 20f; // 이 애니메이션 클립의 초당 프레임 수
    //
    // // 전체 애니메이션 길이 (38개 스프라이트 = 0~37번 인덱스)
    // private const float ANIMATION_TOTAL_DURATION = (1.0f / ANIMATION_FRAME_RATE) * 38;

    public ThreePointSkillSequenceNode(int skillId) : base(skillId)
    {
        this.nodeName = "ThreePointSkillSequenceNode";
    }

    public override void InitializeSkillSequenceNode(MonsterBase monster, PlayerController target)
    {
        base.InitializeSkillSequenceNode(monster, target);

        this.nodeName = "ThreePointSkillSequenceNode";
        animator =  monster.Animator;
    }
    
    protected override bool CanPerform()
    {
        bool result;
        bool isInRange;
        bool isCooldownComplete;
        
        // 쿨다운이 다 차지 않았을 때만 시간 더함
        if(Time.time - lastUsedTime >= skillData.cooldown)
        {
            isCooldownComplete = true;
        }
        else
        {
            isCooldownComplete = false;
        }
        
        // 플레이어와의 거리 확인
        if (Vector2.Distance(monster.transform.position, target.transform.position) <= skillData.range)
        {
            isInRange = true;
        }
        else
        {
            isInRange = false;
        }

        // 두 조건이 모두 만족해야 스킬 사용 가능
        result = isInRange && isCooldownComplete;
        Debug.Log($"Skill {skillData.skillName} (ID: {skillData.skillId}) used? {result} : {Time.time - lastUsedTime} / {skillData.cooldown}");
        return result;
    }

    protected override NodeState SkillAction()
    {
        NodeState state;
        
        // 스킬이 아직 발동되지 않았다면 트리거 켜기
        if (!skillTriggered)
        {
            attackSoundIndex = 0;
            elapsedTime = 0;
            effectStarted = false;
            Debug.Log("Setting Parameter: ThreePoint");
            animator.SetTrigger(AnimatorHash.MonsterParameter.ThreePoint);

            // 상태 초기화 및 애니메이션 시작 시간 기록
            skillTriggered = true;
            FlipCharacter();
            monster.AttackController.SetDamages(skillData.damage1);
            lastUsedTime = Time.time;
        }

        if (!effectStarted)
        {
            effectStarted = true;
            EffectManager.Instance.PlayEffectsByIdAsync(skillId, EffectOrder.Monster, monster.gameObject).Forget();
        }

        // 애니메이션 출력 보장
        if (!isAnimationStarted)
        {
            isAnimationStarted = AnimatorUtility.IsAnimationStarted(monster.Animator, AnimatorHash.MonsterAnimation.ThreePoint);
            return NodeState.Running;
        }

        if (Time.time - lastUsedTime < 0.1f) //시작 직후는 무조건 Running
        {
            return NodeState.Running;
        }
        
        bool isSkillAnimationPlaying = AnimatorUtility.IsAnimationPlaying(monster.Animator, AnimatorHash.MonsterAnimation.ThreePoint);
        if (isSkillAnimationPlaying)
        {
            elapsedTime += Time.deltaTime;
            if (attackSoundIndex < attackSoundTime.Length)
            {
                if (elapsedTime >= attackSoundTime[attackSoundIndex])
                {
                    attackSoundIndex++;
                    EffectManager.Instance.PlayEffectByIdAndTypeAsync(Stage1BossEffectID.NormalAttack * 10 + (Random.Range(0, 2)), EffectType.Sound,
                        monster.gameObject).Forget();
                }    
            }
            Debug.Log($"Running skill: {skillData.skillName} (ID: {skillData.skillId})");
            state = NodeState.Running;
        }
        // 스킬 종료 처리
        // 총 애니메이션 길이만큼 시간이 지났다면 스킬을 종료
        else
        {
            Debug.Log($"Skill End: {skillData.skillName} (ID: {skillData.skillId})");

            skillTriggered = false; // 다음 스킬 사용을 위해 플래그 리셋
            monster.AttackController.ResetDamages(); //데미지 초기화
            state = NodeState.Success;
        }
        return state;
    }


}
