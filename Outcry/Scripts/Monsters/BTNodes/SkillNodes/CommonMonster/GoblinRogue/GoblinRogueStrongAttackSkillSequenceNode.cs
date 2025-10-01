using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoblinRogueStrongAttackSkillSequenceNode : SkillSequenceWithChaseNode
{
    public GoblinRogueStrongAttackSkillSequenceNode(int skillId) : base(skillId)
    {
        this.nodeName = "GoblinRogueStrongAtkNode";
    }

    protected override bool CanPerform()
    {
        bool result;
        bool isCooldownComplete;

        // 쿨다운 체크
        if (Time.time - lastUsedTime >= skillData.cooldown)
        {
            isCooldownComplete = true;
        }
        else
        {
            isCooldownComplete = false;
        }

        result = isCooldownComplete;
        Debug.Log($"[{monster.name}] Skill {skillData.skillName} usable?" +
            $" {result} : Cooldown {Time.time - lastUsedTime} / {skillData.cooldown}");
        return result;
    }

    protected override NodeState SkillAction()
    {
        NodeState state;

        /*
        기본 피해 : 1

         - **플레이어 대응**
             - 패링 사용 불가
         */

        // 패리 불가 불값 수정
        monster.AttackController.SetIsCountable(false);

        // 스킬 트리거 켜기
        if (!skillTriggered)
        {
            lastUsedTime = Time.time;
            FlipCharacter();
            monster.Animator.SetTrigger(AnimatorHash.MonsterParameter.StrongAttack);
            monster.AttackController.SetDamages(skillData.damage1); // 플레이어 데미지 주가

            skillTriggered = true;
        }

        // 시작 직후 Running 강제
        if (Time.time - lastUsedTime < 0.1f)
        {
            return NodeState.Running;
        }

        // 애니메이션 중 Running 리턴 고정
        bool isSkillAnimationPlaying = AnimatorUtility.IsAnimationPlaying(monster.Animator, AnimatorHash.MonsterAnimation.StrongAttack);
        if (isSkillAnimationPlaying)
        {
            Debug.Log($"[{monster.name}] Running skill: {skillData.skillName} (ID: {skillData.skillId})");
            state = NodeState.Running;
        }
        else
        {
            Debug.Log($"[{monster.name}] Skill End: {skillData.skillName} (ID: {skillData.skillId})");

            monster.AttackController.SetDamages(0); //데미지 초기화
            monster.AttackController.SetIsCountable(true); // 카운터블 변수 초기화
            skillTriggered = false;
            state = NodeState.Success;
        }

        return state;
    }
}
