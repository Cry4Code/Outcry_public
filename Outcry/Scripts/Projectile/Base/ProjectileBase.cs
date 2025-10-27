using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ProjectileBase : MonoBehaviour, ICountable
{
    [SerializeField] protected LayerMask playerLayer;

    protected Animator animator;
    [SerializeField] protected int damage;

    protected string poolKey;
    private bool isReleasing;

    protected bool isAttacking = false;
    protected IDamagable target;

    protected bool isCountable = true;

    protected virtual void Awake()
    {        
        animator = gameObject.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning($"{gameObject.name}에 Animator가 없습니다!");
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        int layer = collision.gameObject.layer;

        if ((playerLayer.value & (1 << layer)) != 0 //(other.gameObject.layer == playerLayer)
            && collision.gameObject.TryGetComponent<IDamagable>(out var victim)) 
        {
            target = victim;
            isAttacking = true;

            if (damage > 0)
            {
                victim.TakeDamage(damage);
                Debug.Log($"[{name}] Player hit - damamge {damage}");
            }

            OnHitAfterDamage(collision);            
        }
    }

    protected void OnTriggerExit2D(Collider2D collision)
    {
        int layer = collision.gameObject.layer;

        if ((playerLayer.value & (1 << layer)) != 0) //(other.gameObject.layer == playerLayer)
        {
            isAttacking = false;
            target = null;
        }
    }

    // 안전 장치
    protected virtual void OnDisable()
    {
        StopAllCoroutines();
    }

    /// <summary>
    /// 충돌 시, 데미지 판정 이후 어떤 동작을 할지 설정, 기본은 바로 릴리즈
    /// </summary>
    /// <param name="collision"></param>
    protected virtual void OnHitAfterDamage(Collider2D collision)
    {
        RequestRelease();
    }

    /// <summary>
    /// 생성 직후 호출됨. 내부 변수 설정 등
    /// </summary>
    public virtual void Init(int damage, bool isCountable = true)
    {
        this.damage = damage;
        this.isCountable = isCountable;
        isReleasing = false;
    }

    /// <summary>
    /// 각 투사체가 반환 직전에 해야 할 준비(속도 0, 트리거 off 등)
    /// </summary>
    protected abstract void OnPrepareRelease();

    /// <summary>
    /// 오브젝트 반환 요청 메서드, 파괴 시 이 메서드를 호출
    /// </summary>
    /// <param name="fallbackSeconds"></param>
    /// <param name="callback">파괴되고 나서 반환 직전에 실행할 함수</param>
    public void RequestRelease(float fallbackSeconds = 0.15f, Action callback = null)
    {
        if (isReleasing) return;
        OnPrepareRelease();
        StartCoroutine(ReleaseAfterCurrentState(fallbackSeconds, callback));
    }

    /// <summary>
    /// 애니메이션 길이 만큼 대기 후 릴리스, 애니메이터가 없을 시 대신 입력값만큼 대기
    /// </summary>
    /// <param name="fallbackSeconds"></param>
    /// <param name="callback">파괴되고 나서 반환 직전에 실행할 함수</param>
    /// <returns></returns>
    protected IEnumerator ReleaseAfterCurrentState(float fallbackSeconds = 0.15f, Action callback = null)
    {
        if (isReleasing) yield break;
        isReleasing = true;

        // 현재 상태 대기
        if (animator)
        {
            // 전이 종료까지 잠깐 대기
            float t0 = Time.time, timeout = 3f; // 3초 이상 전이 시, 대기 해제
            while(animator.IsInTransition(0) && Time.time - t0 < timeout)
                yield return null;

            var state = animator.GetCurrentAnimatorStateInfo(0);
            float animSpeed = Mathf.Max(0.001f, Mathf.Abs(animator.speed)); // 최소 속도 보정
            yield return new WaitForSeconds(state.length / animSpeed);
        }
        else if (fallbackSeconds > 0f)  // 입력값이 있을 시
        {
            yield return new WaitForSeconds(fallbackSeconds);
        }

        callback?.Invoke();
        // 최종 반환
        if (!string.IsNullOrEmpty(poolKey))        
            ObjectPoolManager.Instance.ReleaseObject(poolKey, gameObject);
        else
            Destroy(gameObject);    // poolKey 없을 시, 안전망
    }

    /// <summary>
    /// 카운터 어택에 대한 조건을 변경할 때 오버라이드
    /// </summary>
    /// <returns></returns>
    public virtual bool CounterAttacked()
    {
        if (isCountable)
        {
            Debug.Log($"{gameObject.name} CounterAttacked");
            return true;
        }
        return false;
    }

    /// <summary>
    /// AttackController에서 pool 키 세팅할 때 사용
    /// </summary>
    /// <param name="key"></param>
    public void SetPoolKey(string key) => poolKey = key;
}
