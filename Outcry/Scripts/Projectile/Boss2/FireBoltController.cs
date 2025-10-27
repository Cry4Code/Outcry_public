using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBoltController : ProjectileBase
{
    [SerializeField] private LayerMask groundLayer;

    private float speed = 6f;
    private float maxDistance = 10f;

    private Rigidbody2D rb;

    private Vector2 startPos;
    private int dirSign = 1;
    private bool dirInitialized;

    private bool isHit = false;
    private bool isDisspiating = false;

    private bool IsTriggered => animator ? animator.GetBool(AnimatorHash.ProjectileParameter.Triggered) : true;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        if (rb)
        {
            if (rb.bodyType == RigidbodyType2D.Dynamic) rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            //rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // 충돌 없이 통과될 경우 활성화
        }
    }

    private void OnEnable()
    {
        startPos = transform.position;
        dirInitialized = false;
        isHit = false;
        isDisspiating = false;

        if (rb) rb.velocity = Vector2.zero;
        if (animator) animator.SetBool(AnimatorHash.ProjectileParameter.Triggered, true);
    }

    private void FixedUpdate()
    {
        // 파괴 중일 때는 무시
        if (isDisspiating) return;

        // 첫 프레임에 스케일로부터 방향 초기화
        if (!dirInitialized)
        {
            dirSign = (transform.lossyScale.x >= 0f) ? 1 : -1;
            dirInitialized = true;

            if (rb) rb.constraints &= ~RigidbodyConstraints2D.FreezePositionX;  // x축 고정 있는 경우 해제
        }

        // 트리거 True 인 동안에만 이동
        if (IsTriggered)
        {
            if (rb) rb.velocity = new Vector2(speed * dirSign, 0f);
            else transform.position += Vector3.right * (speed * dirSign * Time.deltaTime);
        }
        else if (rb) rb.velocity = Vector2.zero;

        // 파괴 타이밍 설정 - 거리 초과시 소멸
        float sqr = ((Vector2)transform.position - startPos).sqrMagnitude;
        if (sqr >= maxDistance * maxDistance || isHit)
        {
            if (isDisspiating) return;
            isDisspiating = true;
            RequestRelease();
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        // 소실 중 충돌 무시
        if (isDisspiating) return;

        int layer = collision.gameObject.layer;

        // 장애물에 닿으면 소멸            
        if ((groundLayer.value & (1 << layer)) != 0)  //(other.gameObject.layer == groundLayer)
        {
            isDisspiating = true;
            RequestRelease();
        }

        base.OnTriggerEnter2D(collision);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (rb) rb.velocity = Vector2.zero;
    }

    public override void Init(int damage, bool isCountable = true)
    {
        base.Init(damage, isCountable);

        // 재사용 안전 초기화
        isHit = false;
        isDisspiating = false;

        if (rb) rb.velocity = Vector3.zero;

        // 시작 위치 기록
        startPos = transform.position;

        // 애니메이터 트리거 설정 - 발사
        if (animator) animator.SetBool(AnimatorHash.ProjectileParameter.Triggered, true);
    }

    protected override void OnPrepareRelease()
    {
        if (rb) rb.velocity = Vector2.zero;
        if (animator) animator.SetBool(AnimatorHash.ProjectileParameter.Triggered, false);
    }

    /* 기존 방식, 스폰 시점의 플레이어 위치까지 이동
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

    */
}
