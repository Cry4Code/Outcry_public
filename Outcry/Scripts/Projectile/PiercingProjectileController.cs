using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PiercingProjectileController : ProjectileBase
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private int soundNumber;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float maxDistance = 10f;

    private Rigidbody2D rb;

    private Vector2 startPos;

    // 목표 방향 벡터와 설정 여부
    private Vector2 moveDirection = Vector2.right;
    private bool hasMoveDirection = false;

    private bool dirInitialized;

    private bool isDisspiating = false;

    private bool IsTriggered => animator? animator.GetBool(AnimatorHash.ProjectileParameter.Triggered) : true;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        if (rb)
        {
            if (rb.bodyType == RigidbodyType2D.Dynamic) rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // 충돌 누락 방지
        }
    }

    private void OnEnable()
    {
        dirInitialized = false;
        isDisspiating = false;
        hasMoveDirection = false; // 재사용 시 초기화

        if (rb) rb.velocity = Vector2.zero;
        if (animator) animator.SetBool(AnimatorHash.ProjectileParameter.Triggered, true);
        EffectManager.Instance.PlayEffectByIdAndTypeAsync(soundNumber, EffectType.Sound, gameObject).Forget();
    }

    private void FixedUpdate()
    {

        // 첫 프레임에 방향 초기화
        if (!dirInitialized)
        {
            // moveDirection이 지정되어 있으면 그것을 사용, 아니면 localScale로 수평 방향 설정
            if (!hasMoveDirection)
            {
                moveDirection = (transform.localScale.x >= 0f) ? Vector2.right : Vector2.left;
            }
            else
            {
                moveDirection = moveDirection.normalized;
            }
            startPos = transform.position;

            dirInitialized = true;

            if (rb) rb.constraints &= ~RigidbodyConstraints2D.FreezePositionX;  // x축 고정 있는 경우 해제
        }

        // 트리거 True 인 동안에만 이동
        if (IsTriggered)
        {
            if (rb) rb.velocity = moveDirection * speed;
            else transform.position += (Vector3)moveDirection * (speed * Time.deltaTime);
        }
        else if (rb) rb.velocity = Vector2.zero;

        // 파괴 타이밍 설정 - 거리 초과시 소멸
        float sqr = ((Vector2)transform.position - startPos).sqrMagnitude;
        if (sqr >= maxDistance * maxDistance)
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

        // 플레이어인 경우, 데미지 주기
        if ((playerLayer.value & (1 << layer)) != 0 &&
            collision.TryGetComponent<IDamagable>(out var victim)) //(other.gameObject.layer == playerLayer)
        {
            if (damage > 0)
            {
                victim.TakeDamage(damage);
                Debug.Log($"[{name}] Playerlayer hit - damage {damage}");
            }
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (rb) rb.velocity = Vector2.zero;
    }

    public override void Init(int damage, bool isCountable = true)
    {
        base.Init(damage, isCountable);
        isDisspiating = false;

        if (rb) rb.velocity = Vector3.zero;

        // 애니메이터 트리거 설정 - 발사
        if (animator) animator.SetBool(AnimatorHash.ProjectileParameter.Triggered, true);
    }

    // 새 공개 메서드: 특정 월드 위치를 향해 발사하도록 설정
    public void AimAt(Vector2 targetWorldPosition)
    {
        // 방향을 시작 위치 기준으로 계산
        moveDirection = (targetWorldPosition - (Vector2)transform.position).normalized;
        if (moveDirection == Vector2.zero) moveDirection = Vector2.right;
        hasMoveDirection = true;
    }

    // 오버로드: Transform을 넘겨서 목표 지정
    public void AimAt(Transform targetTransform)
    {
        if (targetTransform != null) AimAt(targetTransform.position);
    }

    protected override void OnPrepareRelease()
    {
        if (rb) rb.velocity = Vector2.zero;
        if (animator) animator.SetBool(AnimatorHash.ProjectileParameter.Triggered, false);
    }
}
