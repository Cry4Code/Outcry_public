using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스킬 시퀀스, 추격 있는 버전입니다. 일반 몬스터가 사용합니다.
/// 스킬 사거리는 추격에서 관리하기 때문에 CanPerform에서는 사거리 외의 조건만 확인합니다.
/// </summary>
[Serializable]
public abstract class SkillSequenceWithChaseNode : SkillSequenceNode
{
    public SkillSequenceWithChaseNode(int skillId): base(skillId)
    {

    }

    public override void InitializeSkillSequenceNode(MonsterBase monster, PlayerController target)
    {
        this.monster = monster;
        this.target = target;

        ConditionNode canPerform = new ConditionNode(CanPerform);
        ChaseActionNode chaseAction = new ChaseActionNode(monster.Rb2D, monster.transform, target.transform, monster.MonsterData.chaseSpeed, 
            skillData.range,    // 스킬 사거리 만큼 접근
            monster.Animator);
        // 추격 해제 조건
        float loseRange = skillData.range + 0.1f; // 스킬 사거리 +0.1 만큼 멀어지면 추격 종료
        var keepChasingCondition = new IsInRangeConditionNode(monster.transform, target.transform, loseRange);
        var chaseGuarded = new WhileTrueDecorator(keepChasingCondition, chaseAction);
        ActionNode skillAction = new ActionNode(SkillAction);

        //노드 이름 설정 (디버깅용)
        canPerform.nodeName = "CanPerform";
        chaseAction.nodeName = "ChaseAction";
        chaseGuarded.nodeName = "WhlieTrue(KeepChasing)";
        skillAction.nodeName = "SkillAction";

        children.Clear();
        AddChild(canPerform);
        AddChild(chaseGuarded);
        AddChild(skillAction);

        nodeName = skillData.skillName + skillData.skillId;
        lastUsedTime = Time.time - skillData.cooldown;
    }
}
