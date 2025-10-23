using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBoltController : ProjectileBase
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float arriveThreshold = 0.15f; // 목표 도달 판정 거리

    private Rigidbody2D rb;

    private bool isDissipating;

    private bool IsTriggered => animator ? animator.GetBool(AnimatorHash.ProjectileParameter.Triggered) : true;

    private bool hasLockedTarget;
    private Vector2 lockedTargetPos;
    private Vector2 moveDir;

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
        // 초기화 안전 장치
        InitFields();
        if (animator) animator.SetBool(AnimatorHash.ProjectileParameter.Triggered, true);
    }

    private void FixedUpdate()
    {
        if (isDissipating) return;

        // 첫 프레임에 타겟, 이동 방향 설정
        if (IsTriggered && !hasLockedTarget)
        {
            var player = PlayerManager.Instance ? PlayerManager.Instance.player : null;
            if (player) lockedTargetPos = player.transform.position;

            Vector2 nowPos = transform.position;
            moveDir = (lockedTargetPos - nowPos).normalized;

            // 스프라이트 정렬(우측이 정면일 때 기준)
            var t = transform.localScale;   // 뒤집힌 스프라이트 재정렬
            transform.localScale = new Vector3(Mathf.Abs(t.x), t.y, t.z);
            transform.right = moveDir;

            hasLockedTarget = true;
        }

        // 타겟이 있을 때만 전진
        if (IsTriggered && hasLockedTarget)
        {
            if (rb) rb.velocity = moveDir * speed;
        }
        else if (rb)
        {
            rb.velocity = Vector2.zero;
        }

        // 파괴 조건
        if (hasLockedTarget)
        {
            float distToTarget = Vector2.Distance(transform.position, lockedTargetPos);
            if (distToTarget <= arriveThreshold)
                StartDissipate();
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        // 소멸 중이면 스킵
        if (isDissipating) return;

        int layer = collision.gameObject.layer;
        // 플레이어 피격 시
        if ((playerLayer.value & (1 << layer)) != 0 && collision.TryGetComponent<IDamagable>(out var victim))
        {
            if (damage > 0)
            {
                victim.TakeDamage(damage);
                Debug.Log($"[{name}] Player hit - damage {damage}");
            }
            StartDissipate();
            return;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        InitFields();
    }

    public override void Init(int damage, bool isCountable = true)
    {
        base.Init(damage, isCountable);

        InitFields();
    }

    protected override void OnPrepareRelease()
    {
        if (rb) rb.velocity = Vector2.zero;
        if (animator) animator.SetBool(AnimatorHash.ProjectileParameter.Triggered, false);
    }

    //----내부 함수----
    private void StartDissipate()
    {
        if (isDissipating) return;
        isDissipating = true;
        RequestRelease();
    }
    // 내부 필드 초기화
    private void InitFields()
    {
        if (rb) rb.velocity = Vector2.zero;
        hasLockedTarget = false;
        isDissipating = false;
        lockedTargetPos = Vector2.zero;
        moveDir = Vector2.zero;
    }
}
