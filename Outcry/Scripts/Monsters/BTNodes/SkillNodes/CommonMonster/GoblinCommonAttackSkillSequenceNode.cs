using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 임시 - 수정 필요
public class GoblinCommonAttackSkillSequenceNode : SkillSequenceWithChaseNode
{

    public GoblinCommonAttackSkillSequenceNode(int skillId) : base(skillId)
    {
        this.nodeName = "GoblinCommonAtkNode";
    }

    protected override bool CanPerform()
    {
        bool result;

        // 쿨타임 체크
        bool isCooldownComplete = (Time.time - lastUsedTime) >= skillData.cooldown;
        
        result = isCooldownComplete;
        Debug.Log($"[{monster.name}] Skill {skillData.skillName} usable? " +
            $"{result} : InRange {result}, CoolDown {Time.time - lastUsedTime} / {skillData.cooldown}");
        return result;
    }

    protected override NodeState SkillAction()
    {
        NodeState state;

        /*
        기본 피해 : 1

         - **플레이어 대응**
             - 회피 사용 가능
             - 패링 사용 가능
         */

        // 스킬 트리거 켜기
        if (!skillTriggered)
        {
            lastUsedTime = Time.time;
            FlipCharacter();
            monster.Animator.SetTrigger(AnimatorHash.MonsterParameter.NormalAttack);
            monster.AttackController.SetDamages(skillData.damage1); // 플레이어 데미지 주가

            skillTriggered = true;
        }

        // 시작 직후 Running 강제
        if (Time.time - lastUsedTime < 0.1f)
        {
            return NodeState.Running;
        }

        // 애니메이션 출력 보장
        if (!isAnimationStarted)
        {
            isAnimationStarted = AnimatorUtility.IsAnimationStarted(monster.Animator, AnimatorHash.MonsterAnimation.NormalAttack);
            return NodeState.Running;
        }

        // 애니메이션 중 Running 리턴 고정
        bool isSkillAnimationPlaying = AnimatorUtility.IsAnimationPlaying(monster.Animator, AnimatorHash.MonsterAnimation.NormalAttack);
        if (isSkillAnimationPlaying)
        {
            Debug.Log($"[{monster.name}] Running skill: {skillData.skillName} (ID: {skillData.skillId})");
            state = NodeState.Running;
        }
        else
        {
            Debug.Log($"[{monster.name}] Skill End: {skillData.skillName} (ID: {skillData.skillId})");

            monster.AttackController.SetDamages(0); //데미지 초기화.
            skillTriggered = false;
            state = NodeState.Success;
        }

        return state;
    }
}
