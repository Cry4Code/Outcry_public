using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UpperSlashSkillSequenceNode : SkillSequenceNode
{
    private int animationHash = AnimatorHash.MonsterParameter.UpperSlash;  
    
    public UpperSlashSkillSequenceNode(int skillId) : base(skillId)
    {
        this.nodeName = "UpperSlashSequenceNode";
    }

    protected override bool CanPerform()
    {
        bool result;
        bool isInRange;
        bool isCooldownComplete;

        // 플레이어와 거리 2m 이내에 있을때
        // MonsterSkillModel 수정 필요 (Stomp 스킬 참조)
        if (Vector2.Distance(monster.transform.position, target.transform.position) <= skillData.range)
        {
            isInRange = true;
        }
        else
        {
            isInRange = false;
        }

        // 쿨다운 체크
        if (Time.time - lastUsedTime >= skillData.cooldown)
        {
            isCooldownComplete = true;
        }
        else
        {
            isCooldownComplete = false;
        }

        result = isInRange && isCooldownComplete;
        Debug.Log($"Skill {skillData.skillName} (ID: {skillData.skillId}) used? {result} : {Time.time - lastUsedTime} / {skillData.cooldown}");
        return result;
    }

    protected override NodeState SkillAction()
    {
        NodeState state;

        // 기본 피해 : HP 2칸 감소
        // 넉백 추가는 미정

        // ** 플레이어 대응 **
        //      - 회피 사용 가능
        //      - 패링 사용 가능

        if (!skillTriggered)
        {
            lastUsedTime = Time.time;
            FlipCharacter();
            monster.Animator.SetTrigger(animationHash);

            monster.AttackController.SetDamages(skillData.damage1);

            skillTriggered = true;
        }

        // 시작 직후 Running 강제
        if (Time.time - lastUsedTime < 0.1f)
        {            
            return NodeState.Running;
        }

        bool isSkillAnimationPlaying = AnimatorUtility.IsAnimationPlaying(monster.Animator, AnimatorHash.MonsterAnimation.UpperSlash);
        if (isSkillAnimationPlaying)
        {
            Debug.Log($"Running skill: {skillData.skillName} (ID: {skillData.skillId})");
            state = NodeState.Running;
        }
        else
        {
            Debug.Log($"Skill End: {skillData.skillName} (ID: {skillData.skillId})");

            monster.AttackController.ResetDamages();  // 데미지 초기화
            skillTriggered = false;
            state = NodeState.Success;
        }

        return state;
    }
}
