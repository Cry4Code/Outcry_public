using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class FireballController : ProjectileBase
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

    private bool IsTriggered => animator? animator.GetBool(AnimatorHash.ProjectileParameter.Triggered) : true;

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
        EffectManager.Instance.PlayEffectByIdAndTypeAsync(1032021,  EffectType.Sound, gameObject).Forget();
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
}
