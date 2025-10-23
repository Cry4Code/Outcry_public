using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct TimedWayPoint
{
    public Vector2 position;
    public float time;
}

public class WayPointFlySequenceNode : SequenceNode
{
    private Rigidbody2D rb;
    private Transform me;
    private Transform waypointTarget;
    private FlyToTargetActionNode flyNode;

    [SerializeField] private List<TimedWayPoint> wayPoints;
    private float speed;
    private float stoppingDistance;
    
    private float waitTimer;
    private bool isWaiting;
    
    private bool ignoreNextSuccess = false;
    
    public WayPointFlySequenceNode(Transform me, Rigidbody2D rb, TimedWayPoint[] wayPointsArray, float speed, float stoppingDistance)
    {
        this.me = me;
        this.rb = rb;
        this.speed = speed;
        this.stoppingDistance = stoppingDistance;
        this.waitTimer = 0f;
        this.isWaiting = false;
        
        this.wayPoints = wayPointsArray != null ? new List<TimedWayPoint>(wayPointsArray) : new List<TimedWayPoint>();

        // 1. 임시 Target Transform 생성
        GameObject targetObject = new GameObject($"WaypointTarget_{me.name}");
        this.waypointTarget = targetObject.transform;

        // 초기 목표 설정
        if (this.wayPoints.Count > 0)
        {
            waypointTarget.position = wayPoints[0].position;
        }        
        else
        {
            waypointTarget.position = me.position;
        }

        // 2. FlyToTargetActionNode 생성: 생성자에서 받은 실제 speed 값 사용!
        this.flyNode = new FlyToTargetActionNode(rb, me, waypointTarget, speed, stoppingDistance);
        flyNode.SetTarget(waypointTarget);
        children.Add(flyNode);
    }
    
    public override NodeState Tick()
    {
        if (wayPoints == null || wayPoints.Count == 0)
        {
            return NodeState.Failure;
        }
        
        // 1. 현재(첫) 웨이포인트
        TimedWayPoint current = wayPoints[0];

        // 2. 대기 로직
        if (isWaiting)
        {
            // 지정된 대기 시간을 채웠는지 검사
            if (Time.time >= waitTimer)
            {
                // 대기 완료 -> 다음 웨이포인트로 넘어가기 준비
                wayPoints.RemoveAt(0);
                isWaiting = false;
                waitTimer = 0f;

                // 다음 웨이포인트가 남아 있다면, FlyNode의 목표를 갱신
                if (wayPoints.Count > 0)
                {
                    waypointTarget.position = wayPoints[0].position;
                    flyNode.SetTarget(waypointTarget);
                    flyNode.Reset();
                    ignoreNextSuccess = true;
                    return NodeState.Running;
                }
                else
                {
                    // 모든 웨이포인트 소모 -> Failure 반환
                    return NodeState.Failure;
                }
            }

            return NodeState.Failure; // 대기 중에는 Failure 반환 >> 다른 공격 노드 실행 위함.
        }

        // 3. 자식 노드(FlyNode) 실행
        // FlyNode는 현재 waypointTarget을 향해 speed로 비행합니다.
        NodeState state = flyNode.Tick();
        
        // 목표 갱신 직후 오는 Success를 한 번 무시
        if (state == NodeState.Success && ignoreNextSuccess)
        {
            ignoreNextSuccess = false;
            return NodeState.Running;
        }

        if (state == NodeState.Running)
        {
            return NodeState.Running;
        }

        if (state == NodeState.Failure)
        {
            Reset(); 
            return NodeState.Failure;
        }

        // state == NodeState.Success 일 때 (FlyNode가 현재 웨이포인트에 도달했을 때)
        if (state == NodeState.Success)
        {
            // 4. 웨이포인트 도달 -> 대기 시작
            
            // 만약 현재 웨이포인트의 대기 시간이 0보다 크다면 대기 로직 시작
            if (current.time > 0f)
            {
                rb.velocity = Vector2.zero;
                isWaiting = true;
                waitTimer = Time.time + current.time;
                //도달 후 시간이 남아있는 상태이므로 Failure 반환 >> 다른 공격 노드 실행 위함.
                return NodeState.Failure;
            }
            else
            {
                // 대기 시간이 없으면 즉시 제거하고 다음으로
                wayPoints.RemoveAt(0);

                if (wayPoints.Count == 0)
                {
                    // 모든 웨이포인트 소모 -> Failure 반환
                    return NodeState.Failure;
                }

                waypointTarget.position = wayPoints[0].position;
                flyNode.SetTarget(waypointTarget);
                flyNode.Reset(); 
                ignoreNextSuccess = true;
                return NodeState.Running;
            }
        }
        
        Debug.LogError("[WayPointFlySequenceNode] Unexpected state reached.");
        return NodeState.Running; 
    }
    
    // public override void Reset()
    // {
    //     base.Reset();
    //     // if (wayPoints != null)
    //     // {
    //     //     wayPoints.Clear();
    //     // }
    //
    //     waitTimer = 0f;
    //     isWaiting = false;
    //     ignoreNextSuccess = false;
    //
    //     // waypointTarget은 현재 남아있는 첫 웨이포인트 또는 me 위치로 맞춤
    //     waypointTarget.position = (wayPoints != null && wayPoints.Count > 0)
    //         ? wayPoints[0].position
    //         : (me != null ? me.position : Vector3.zero);
    //
    //     if (flyNode != null)
    //     {
    //         flyNode.SetTarget(waypointTarget);
    //         flyNode.Reset();
    //     }
    // }
}
