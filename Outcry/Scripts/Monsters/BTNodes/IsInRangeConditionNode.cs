using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// range 안에 target이 있는지 확인하는 컨디션 노드입니다. 
/// </summary>
public class IsInRangeConditionNode : ConditionNode
{
    private Transform me;
    private Transform target;
    private float range;

    public IsInRangeConditionNode(Transform me, Transform target, float range)
    {
        this.me = me;
        this.target = target;
        this.range = range;

        this.nodeName = "IsInRangeConditionNode";
    }

    protected override bool IsCondition()
    {
        float rangeSqr = range * range;
        float distanceSqr = ((Vector2)me.position - (Vector2)target.position).sqrMagnitude;
        return distanceSqr <= rangeSqr;
    }
}
