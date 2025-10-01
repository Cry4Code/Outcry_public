using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsGroundedConditionNode : ConditionNode
{
    private readonly Transform me;
    private readonly Collider2D collider;

    private readonly float footOffsetY; // collider 없을 때의 대략적인 y 좌표 보정
    private readonly float maxGroundDistance;   // 바닥에서의 최대 가능 거리  
    private readonly int groundMask;

    public IsGroundedConditionNode(Transform me, float footOffsetY = 1f, float maxGroundDistance = 0.1f, string groundLayerName = "Ground")
    {
        this.me = me;
        this.collider = me.GetComponent<Collider2D>();
        this.footOffsetY = footOffsetY;
        this.maxGroundDistance = maxGroundDistance;
        this.groundMask = LayerMask.GetMask(groundLayerName);
    }

    protected override bool IsCondition()
    {
        // 발 위치 계산
        float feetY = collider
            ? collider.bounds.min.y // 콜라이더 바닥에서
            : me.position.y - footOffsetY; // 없으면 대략 y 좌표 -footOffsetY 위치에서

        Vector2 origin = new Vector2(me.position.x, feetY);

        // 아래로 Ray로 Ground 체크
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, maxGroundDistance, groundMask);

        if (hit.collider == null) return false;
        
        return true;
    }
}
