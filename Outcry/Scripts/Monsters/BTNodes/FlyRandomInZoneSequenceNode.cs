// csharp
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FlyRandomInZoneSequenceNode : SequenceNode
{
    private Rigidbody2D rb;
    private Transform me;
    private ZoneMarker zone;
    private Transform waypointTarget;
    private FlyToTargetActionNode flyNode;

    private float speed;
    private float stoppingDistance;
    private float minWait;
    private float maxWait;

    private float waitTimer;
    private bool isWaiting;

    private bool ignoreNextSuccess = false;
    private bool hasTarget = false;

    public FlyRandomInZoneSequenceNode(Transform me, Rigidbody2D rb, ZoneMarker zone, float speed, float stoppingDistance, float minWait = 0f, float maxWait = 0f)
    {
        this.me = me;
        this.rb = rb;
        this.zone = zone;
        this.speed = speed;
        this.stoppingDistance = stoppingDistance;
        this.minWait = Mathf.Max(0f, minWait);
        this.maxWait = Mathf.Max(this.minWait, maxWait);
        this.waitTimer = 0f;
        this.isWaiting = false;

        GameObject targetObject = new GameObject($"RandomAreaTarget_{(me != null ? me.name : "Unknown")}");
        this.waypointTarget = targetObject.transform;
        this.waypointTarget.position = me != null ? me.position : Vector3.zero;

        this.flyNode = new FlyToTargetActionNode(rb, me, waypointTarget, speed, stoppingDistance);
        flyNode.SetTarget(waypointTarget);
        children.Add(flyNode);
    }

    private void PickRandomWaypoint()
    {
        if (zone == null) return;
        Vector2 p = GetRandomPointInZone(zone);
        waypointTarget.position = p;
        flyNode.SetTarget(waypointTarget);
        flyNode.Reset();
        ignoreNextSuccess = true;
        hasTarget = true;
    }

    public override NodeState Tick()
    {
        if (zone == null || me == null || rb == null) return NodeState.Failure;

        if (!hasTarget)
        {
            PickRandomWaypoint();
            if (!hasTarget) return NodeState.Failure;
        }

        if (isWaiting)
        {
            if (Time.time >= waitTimer)
            {
                isWaiting = false;
                waitTimer = 0f;
                PickRandomWaypoint();
                return NodeState.Running;
            }
            // 대기 중이면 다른 노드 허용을 위해 Failure 반환
            return NodeState.Failure;
        }

        NodeState state = flyNode.Tick();

        if (state == NodeState.Success && ignoreNextSuccess)
        {
            ignoreNextSuccess = false;
            return NodeState.Running;
        }

        if (state == NodeState.Running) return NodeState.Running;

        if (state == NodeState.Failure)
        {
            Reset();
            return NodeState.Failure;
        }

        // 목표 도달 처리
        if (state == NodeState.Success)
        {
            // 정지
            if (rb != null) rb.velocity = Vector2.zero;

            float wait = Random.Range(minWait, maxWait);
            if (wait > 0f)
            {
                isWaiting = true;
                waitTimer = Time.time + wait;
                return NodeState.Failure;
            }
            else
            {
                PickRandomWaypoint();
                return NodeState.Running;
            }
        }

        return NodeState.Running;
    }
    
    private Vector2 GetRandomPointInZone(ZoneMarker zone)
    {
        float x = Random.Range(-zone.rangeX, zone.rangeX);
        float y = Random.Range(-zone.rangeY, zone.rangeY);
        return (Vector2)zone.transform.position + new Vector2(x, y);
    }

    public override void Reset()
    {
        base.Reset();
        waitTimer = 0f;
        isWaiting = false;
        ignoreNextSuccess = false;
        hasTarget = false;

        if (waypointTarget != null && me != null)
        {
            waypointTarget.position = me.position;
        }

        if (flyNode != null)
        {
            flyNode.SetTarget(waypointTarget);
            flyNode.Reset();
        }
    }
}