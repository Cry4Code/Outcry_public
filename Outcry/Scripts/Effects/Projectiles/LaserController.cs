using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserController : MonoBehaviour
{
    [SerializeField] private float duration = 1f;
    [SerializeField] private int damage;
    [SerializeField] private Animator animator;
    private float enableTime;
    
    private bool isAttackingPlayer = false;
    private IDamagable target = null;   //싱글 플레이어이므로 단일 변수이지만, 추후 싱글이 아닌 멀티이거나, 몬스터끼리 공격하는 상황이 발생한다면 List<IDamagable> 로 바꿀 것.
    [SerializeField] private LayerMask playerLayer;

    private void OnEnable()
    {
        enableTime = Time.time;
        if (animator != null)
        {
            animator.SetTrigger(AnimatorHash.ProjectileParameter.Triggered);
        }
        else
        {
            Debug.LogError("Animator is not assigned in LaserController.");
        }
    }

    private void Update()
    {
        Attack();
        if (Time.time - enableTime >= duration)
        {
            ObjectPoolManager.Instance.ReleaseObject(AddressablePaths.Projectile.Laser, this.gameObject);
        }
    }

    public void SetDamage(int damage)
    {
        this.damage = damage;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((playerLayer.value & (1 << other.gameObject.layer)) != 0) //(other.gameObject.layer == playerLayer)
        {
            isAttackingPlayer = other.gameObject.TryGetComponent<IDamagable>(out target);
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if ((playerLayer.value & (1 << other.gameObject.layer)) != 0) //(other.gameObject.layer == playerLayer)
        {
            // 나가는 순간 공격 상태를 무조건 종료하고 대상 null 처리
            isAttackingPlayer = false; 
            target = null;
        }
    }
    private void Attack()
    {
        if (isAttackingPlayer && target != null && damage > 0)
        {
            target.TakeDamage(damage);
        }
    }
}
