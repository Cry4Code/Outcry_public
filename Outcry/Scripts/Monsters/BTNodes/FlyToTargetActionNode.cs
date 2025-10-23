using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyToTargetActionNode : ActionNode 
{
    private Rigidbody2D rb; 
    private Transform me;
    private Transform target;
    private float flySpeed;
    private float stoppingDistance;
    private float minY;
    private float minYOffset = 10f;
    private float maxYOffset = 10f;
    private float maxY;
    
    private float altitudeThreshold = 0.5f; 
    
    public FlyToTargetActionNode(Rigidbody2D rb, Transform me, Transform target, float speed, float stoppingDistance) 
    {
        this.rb = rb;
        this.me = me;
        this.target = target;
        this.flySpeed = speed;
        this.stoppingDistance = stoppingDistance;
    }
    protected override NodeState Act()
    {
        if (rb == null || me == null)
        {
            if (rb != null) rb.velocity = Vector2.zero;
            return NodeState.Failure;
        }
        if (target == null)
        {
            rb.velocity = Vector2.zero;
            return NodeState.Failure;
        }

        if (Camera.main != null)
        {
            float camCenterY = Camera.main.transform.position.y;
            minY = camCenterY - minYOffset;
            maxY = camCenterY + maxYOffset;
        }
        
        float clampedTargetY = Mathf.Clamp(target.position.y, minY, maxY);

        Vector2 toTarget = new Vector2(target.position.x - me.position.x, clampedTargetY - me.position.y);
        float distanceToTarget = toTarget.magnitude;

        if (distanceToTarget <= stoppingDistance)
        {
            rb.velocity = Vector2.zero;
            return NodeState.Success;
        }

        Vector2 direction = (target.position - me.position).normalized;
        
        float currentY = me.position.y;
        float targetY = target.position.y; 

        float verticalMovement = direction.y; 
        bool isAltitudeControlled = false;

        // 고도 제어 로직 및 상태 업데이트
        if (currentY < targetY - altitudeThreshold)
        {
            verticalMovement = 1f; 
            isAltitudeControlled = true;
            Debug.Log("rise"); 
        }
        else if (currentY > targetY + altitudeThreshold)
        {
            verticalMovement = -1f; 
            isAltitudeControlled = true;
            Debug.Log("Descent"); 
        }
        
        Vector2 finalDirection = isAltitudeControlled
            ? new Vector2(direction.x, verticalMovement).normalized
            : direction;

        rb.velocity = finalDirection * flySpeed;

        FlipCharacter();
        
        return NodeState.Running;
    }
    
    protected void FlipCharacter()
    {
        float originalScaleX = me.transform.localScale.x;
        if (me.transform.position.x < target.transform.position.x)
            me.transform.localScale = new Vector3(Mathf.Abs(originalScaleX), me.transform.localScale.y, me.transform.localScale.z);
        else
            me.transform.localScale = new Vector3(-Mathf.Abs(originalScaleX), me.transform.localScale.y, me.transform.localScale.z);
    }
    // 리셋 시 상태도 초기화
    public override void Reset()
    {
        rb.velocity = Vector2.zero;
        base.Reset();
    }


    public void SetTarget(Transform target)
    {
        this.target = target;
    }
}