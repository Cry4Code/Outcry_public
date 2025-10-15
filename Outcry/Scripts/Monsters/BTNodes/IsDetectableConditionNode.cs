using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 벽 등 장애물이 사이에 없고, 시아 거리(detect range) 이내일 떄만 true
/// </summary>
public class IsDetectableConditionNode : ConditionNode
{
    private readonly Transform me;
    private readonly Transform target;
    private readonly float range;
    private readonly int obstacleMask;

    public IsDetectableConditionNode(Transform me, Transform target, float detectRange, string obstacleMaskName = "Ground")
    {
        this.me = me;
        this.target = target;
        this.range = detectRange;
        this.obstacleMask = LayerMask.GetMask(obstacleMaskName);
        this.nodeName = "IsDetactableConditionNode";
    }

    protected override bool IsCondition()
    {
        // 탐지 거리 우선 체크
        float rangeSqr = range * range;
        float distanceSqr = ((Vector2)me.position - (Vector2)target.position).sqrMagnitude;
        if (distanceSqr > rangeSqr) return false;

        // 시야(레이)에 장애물이 있으면 false
        Vector2 origin = (Vector2) me.position;
        Vector2 destination = (Vector2) target.position;
        var hit = Physics2D.Linecast(origin, destination, obstacleMask);
        return hit.collider == null;
    }
}
