using System;
using UnityEngine;

/// <summary>
/// 기본 이동 노드
/// me가 target을 향해 speed 속도로 이동
/// target과 me의 거리가 stoppingDistance 이내가 되면 성공
/// </summary>
public class MoveToTargetActionNode : ActionNode
{
    protected Rigidbody2D rb;
    protected Transform me;
    protected Transform target;
    protected float speed;
    protected float stoppingDistance;
    protected float originalScaleX;

    public MoveToTargetActionNode(Rigidbody2D rb, Transform me, Transform target, float speed, float stoppingDistance)
    {
        this.rb = rb;
        this.me = me;
        this.target = target;
        this.speed = speed;
        this.stoppingDistance = stoppingDistance;
        this.originalScaleX = me.localScale.x;
    }
        
    protected override NodeState Act()
    {
        Debug.Log($"[{me.name}] MoveToTarget");
        if (target == null)
        {
            return NodeState.Failure;
        }

        Vector2 targetPosition = new Vector2(target.position.x, me.position.y);
        float distance = Vector2.Distance(me.position, targetPosition);
        if (distance <= stoppingDistance)
        {
            return NodeState.Success;
        }
        else
        {
            if (me.position.x < target.position.x)
                me.localScale = new Vector3(Mathf.Abs(originalScaleX) , me.localScale.y, me.localScale.z);
            else
                me.localScale = new Vector3(-Mathf.Abs(originalScaleX), me.localScale.y, me.localScale.z);
            
            Vector2 newPosition = Vector2.MoveTowards(
                rb.position,
                targetPosition,
                speed * Time.deltaTime
            );
            rb.MovePosition(newPosition);
            return NodeState.Running;
        }
    }
}
