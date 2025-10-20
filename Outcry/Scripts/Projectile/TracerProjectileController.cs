using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TracerProjectileController : ProjectileBase
{
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private float speed = 15f;
    [SerializeField] private int traceCount = 3;
    [SerializeField] private float traceInterval = 1f;

    private Rigidbody2D rb;

    private Vector2 targetPosition;
    private Vector2 direction;

    private Coroutine traceCoroutine;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        if (rb)
        {
            if (rb.bodyType == RigidbodyType2D.Dynamic) rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void OnEnable()
    {
        targetPosition = Vector2.zero;
        direction = Vector2.zero;

        if (traceCoroutine != null) StopCoroutine(traceCoroutine);
        traceCoroutine = StartCoroutine(TraceTarget());
    }

    private void OnDisable()
    {
        if (traceCoroutine != null)
        {
            StopCoroutine(traceCoroutine);
            traceCoroutine = null;
        }
        if(rb != null)
            rb.velocity = Vector2.zero;
        targetPosition = Vector2.zero;
        direction = Vector2.zero;
    }

    private IEnumerator TraceTarget()
    {
        int traced = 0;
        
        while (traced < traceCount)
        {
            //플레이어 위치 설정
            if (targetPosition == Vector2.zero)
            {
                var player = PlayerManager.Instance.player;
                if (player != null)
                    targetPosition = player.transform.position;
                else
                    yield break; // 플레이어도 없으면 종료
            }
            
            // 목표를 향해 이동하다가 도달하면 대기 후 다음 추적으로
            while (!ArrivedToTarget())
            {
                // 현재 위치 기준으로 방향 재계산 (타겟이 움직이면 따라감)
                direction = (targetPosition - (Vector2)transform.position).normalized;
                transform.right = direction;

                if (rb != null)
                    rb.velocity = direction * speed;
                else
                    transform.Translate(direction * speed * Time.deltaTime, Space.World);

                yield return null;
            }

            // 도달 처리: 속도 정지, 인터벌 대기
            if (rb != null) rb.velocity = Vector2.zero;
            yield return new WaitForSeconds(traceInterval);

            traced++;

            // 다음 타겟을 플레이어로 설정 (없으면 루프 종결)
            var nextPlayer = PlayerManager.Instance.player;
            if (nextPlayer != null)
                targetPosition = nextPlayer.transform.position;
            else
                break;
        }
        
        if (rb != null) rb.velocity = Vector2.zero;
        traceCoroutine = null;
        ObjectPoolManager.Instance.ReleaseObject(poolKey, this.gameObject);
    }


    private bool ArrivedToTarget()
    {
        if (targetPosition == Vector2.zero) return false;
        float distanceToTarget = Vector2.Distance(transform.position, targetPosition);
        return distanceToTarget <= 0.2f; // 도착 판정 거리
    }

    protected override void OnHitAfterDamage(Collider2D collision)
    {
        //아무것도 하지 않음
    }

    protected override void OnPrepareRelease()
    {
        return; // 할게 없음.
    }
}
