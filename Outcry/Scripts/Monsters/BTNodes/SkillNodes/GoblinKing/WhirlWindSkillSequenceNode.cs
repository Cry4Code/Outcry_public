using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class WhirlWindSkillSequenceNode : SkillSequenceNode
{
    // 애니메이션 클립 초당 프레임 수
    private const float ANIMATION_FRAME_RATE = 20f;

    private float[] attackSoundTime = new[]
    {
        (1f / ANIMATION_FRAME_RATE) * 8f,
        (1f / ANIMATION_FRAME_RATE) * 20f,
        (1f / ANIMATION_FRAME_RATE) * 31f,
    };

    private int attackSoundIndex = 0;
    private float elapsedTime = 0;
    
    public WhirlWindSkillSequenceNode(int skillId) : base(skillId)
    {
        this.nodeName = "WhirlWindSkillSequenceNode";
    }

    protected override bool CanPerform()
    {
        bool result;
        bool isInRange;
        bool isCooldownComplete;

        //플레이어와 거리 이내에 있을때
        if (Vector2.Distance(monster.transform.position, target.transform.position) <= skillData.range)
        {
            isInRange = true;
        }
        else
        {
            isInRange = false;
        }

        //쿨다운 확인
        if (Time.time - lastUsedTime >= skillData.cooldown)
        {
            isCooldownComplete = true;
        }
        else
        {
            isCooldownComplete = false;
        }

        result = isInRange && isCooldownComplete;
        Debug.Log($"Skill {skillData.skillName} used? {result} : {Time.time - lastUsedTime} / {skillData.cooldown}");
        return result;
    }

    protected override NodeState SkillAction()
    {
        NodeState state;

        // - **플레이어 대응**
        //     - 회피 사용 가능
        //     - 패링 사용 가능

        if (!skillTriggered)
        {
            attackSoundIndex = 0;
            elapsedTime = 0;
            effectStarted = false;
            lastUsedTime = Time.time;
            FlipCharacter();
            monster.Animator.SetTrigger(AnimatorHash.MonsterParameter.WhirlWind);
            
            monster.AttackController.SetDamages(skillData.damage1);
            skillTriggered = true;
        }
        
        if (!effectStarted)
        {
            effectStarted = true;
            EffectManager.Instance.PlayEffectsByIdAsync(skillId, EffectOrder.Monster, monster.gameObject).Forget();
        }

        // 애니메이션 출력 보장
        if (!isAnimationStarted)
        {
            isAnimationStarted = AnimatorUtility.IsAnimationStarted(monster.Animator, AnimatorHash.MonsterAnimation.WhirlWind);
            return NodeState.Running;
        }

        //시작 직후는 무조건 Running
        if (Time.time - lastUsedTime < 0.1f)
        {
            return NodeState.Running;
        }

        
            
        bool isSkillAnimationPlaying = AnimatorUtility.IsAnimationPlaying(monster.Animator, AnimatorHash.MonsterAnimation.WhirlWind);
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
        else
        {
            Debug.Log($"Skill End: {skillData.skillName} (ID: {skillData.skillId})");

            monster.AttackController.SetDamages(0); //데미지 초기화.
            skillTriggered = false;
            state = NodeState.Success;
        }

        return state;
    }


}
