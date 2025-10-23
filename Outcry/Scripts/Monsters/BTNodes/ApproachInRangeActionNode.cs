using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApproachInRangeActionNode : MoveToTargetActionNode
{
    private readonly float range;
    private readonly Animator animator;
    private readonly MonsterAIBase ai;

    public ApproachInRangeActionNode( Rigidbody2D rb, Transform me, Transform target, float speed, float range, Animator animator, MonsterAIBase ai)
        : base(rb, me, target, speed, /*stoppingDistance*/ 0f)
    {        
        this.range = range;
        this.animator = animator;
        this.ai = ai;
    }

    protected override NodeState Act()
    {
        if (me == null || target == null) return NodeState.Failure;

        // 포션 즉발일 땐 '접근'을 스킵하고 곧바로 다음 스킬 사용
        if (ai != null && ai.blackBoard.PotionOverrideEdge)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            if (animator) animator.SetBool(AnimatorHash.MonsterParameter.Running, false);
            return NodeState.Success; // 접근 노드 통과, 즉시 스킬 시전
        }

        // 거리
        float dist = Vector2.Distance(me.position, target.position);

        // 범위 안: 이동 종료
        if (dist <= range)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            animator.SetBool(AnimatorHash.MonsterParameter.Running, false);
            return NodeState.Success;
        }

        // 범위를 벗어난 경우: 접근
        // 이동 방향
        float moveDirX = Mathf.Sign(target.position.x - me.position.x);

        // 얼굴 방향(좌우 반전)
        float baseAbs = Mathf.Abs(me.localScale.x);
        if (!Mathf.Approximately(baseAbs, 0f))
        {
            me.localScale = new Vector3(moveDirX >= 0 ? baseAbs : -baseAbs, me.localScale.y, me.localScale.z);
        }

        rb.velocity = new Vector2(moveDirX * speed, rb.velocity.y);
        animator.SetBool(AnimatorHash.MonsterParameter.Running, true);
        return NodeState.Running;
    }

    public override void Reset()
    {
        base.Reset();
        if (rb != null) rb.velocity = new Vector2(0f, rb.velocity.y);
        animator.SetBool(AnimatorHash.MonsterParameter.Running, false);
    }
}

