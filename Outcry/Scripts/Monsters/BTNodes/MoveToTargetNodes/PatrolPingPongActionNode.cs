using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolPingPongActionNode : MoveToTargetActionNode
{
    private readonly Animator animator;
    private readonly float originX;
    private readonly float range;
    private readonly int obstacleMask;

    private bool goingRight = false;
    private bool reachedEnd = false; // 직전 Tick에서 끝점 도달이면 true

    private float leftX;
    private float rightX;

    private const float WALL_PADDING = 0.15f;

    public PatrolPingPongActionNode(Rigidbody2D rb, Transform me, float speed, Animator animator,
        float range = 5f, float stoppingDistance = 0.2f)
        : base(rb, me, new GameObject($"{me.name}_PatrolTarget").transform, speed, stoppingDistance)
    {
        this.animator = animator;
        this.range = Mathf.Abs(range);
        this.originX = me.position.x; // 스폰시 X 고정
        this.obstacleMask = LayerMask.GetMask("Ground");

        RecalculateBounds(); // 좌/우 끝점 계산(벽 보정 포함)
        target.position = new Vector3(rightX, me.position.y, me.position.z);
        goingRight = false;
    }

    protected override NodeState Act()
    {
        var state = base.Act(); // 이동 + 도달 판정 + 좌우 플립은 부모에서 처리

        animator.SetBool(AnimatorHash.MonsterParameter.Walking, state == NodeState.Running);

        if (state == NodeState.Success) // 끝점 도달
            reachedEnd = true;

        return state;
    }

    public override void Reset()
    {
        base.Reset();

        animator.SetBool(AnimatorHash.MonsterParameter.Walking, false);

        // 시퀀스가 한 바퀴 성공하고 Reset될 때 다음 목적지로 방향 전환
        if (reachedEnd)
        {
            goingRight = !goingRight; // 좌↔우 반전
            reachedEnd = false;
        }

        float nextX = goingRight ? rightX : leftX;
        target.position = new Vector3(nextX, me.position.y, me.position.z);
    }

    private void RecalculateBounds()
    {
        // 기본 좌/우 경계
        leftX = originX - range;
        rightX = originX + range;

        // 벽에 막히면 해당 방향 경계를 안쪽으로 당김
        Vector2 origin = new Vector2(originX, me.position.y);
        var hitRight = Physics2D.Raycast(origin, Vector2.right, range, obstacleMask);
        if (hitRight.collider != null) rightX = hitRight.point.x - WALL_PADDING;

        var hitLeft = Physics2D.Raycast(origin, Vector2.left, range, obstacleMask);
        if (hitLeft.collider != null) leftX = hitLeft.point.x + WALL_PADDING;
    }
}

