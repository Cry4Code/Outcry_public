using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 순찰용 노드, MoveToTargetNode를 상속받습니다. 50 퍼센트로 좌 혹은 우로 이동합니다.
/// </summary>
public class PatrolActionNode : MoveToTargetActionNode
{
    private Animator animator;
    private Collider2D collider;

    private const float STOPPING_DISTANCE = 2f;

    private readonly float moveDistance = 10f;  // 움질일 최대 거리
    private readonly int obstacleMask;  // 벽 레이어 -> 현재 Ground 사용 중
    private readonly float wallPadding = 0.15f; // 벽 앞 여유

    private int? nextDirSign = null; // null이면 랜덤, 값이 있으면 그쪽으로
    private bool isHitWall = false;
   

    public PatrolActionNode(Rigidbody2D rb, Transform me, float speed, Animator animator, string groundLayerName = "Ground") : base(rb, me,
        new GameObject($"{me.name}_PatrolTarget").transform,    // 순찰 목표 위치용 오브젝트 생성
        speed, STOPPING_DISTANCE)
    {
        this.animator = animator;
        this.collider = me.GetComponent<Collider2D>();
        this.obstacleMask = LayerMask.GetMask(groundLayerName);

        target.position = me.position;
        Reset();
    }

    protected override NodeState Act()
    {
        NodeState state = base.Act();
        
        if (state == NodeState.Running)
        {
            animator.SetBool(AnimatorHash.MonsterParameter.Walking, true);
        }
        else
        {
            animator.SetBool(AnimatorHash.MonsterParameter.Walking, false);
        }
        return state;
    }

    public override void Reset()
    {
        base.Reset();

        // 애니메이터 파라미터 리셋
        animator.SetBool(AnimatorHash.MonsterParameter.Walking, false); 
        
        // 순찰 포인트 설정
        int dirSign = nextDirSign ?? ((Random.Range(0, 2) == 0) ? 1 : -1);   // 순찰 방향
        target.position = MakeReachablePatrolPoint(dirSign);        
        nextDirSign = isHitWall ? -dirSign : (int?) null;   // 벽에 막히면 다음은 반대 방향으로

        Debug.Log($"[patrol Action Reset] Target Postion X: {target.position.x:F2} Monster Postion X: {me.position.x:F2}");
    }

    private Vector3 MakeReachablePatrolPoint(int dirSign)
    {
        Vector3 origin = me.position;
        Vector3 dir = new Vector3(dirSign, 0, 0);

        // Ray로 벽 탐지
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, moveDistance, obstacleMask);

        float targetX;
        if (hit.collider != null) // 벽 충돌 시, 벽에서 보정치 만큼 앞에서 멈춤
        {
            targetX = hit.point.x - dirSign * wallPadding;
            isHitWall = true;
        }
        else // 충돌 없을 시, 원래 이동 거리만큼 이동
        {
            targetX = me.position.x + dirSign * moveDistance;   
            isHitWall = false;
        }

        return new Vector3(targetX, me.position.y, me.position.z);
    }
}
