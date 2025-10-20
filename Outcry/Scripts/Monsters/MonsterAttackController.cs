using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class MonsterAttackController : MonoBehaviour, ICountable
{
    private MonsterBase monster;
    [SerializeField] private LayerMask playerLayer;

    private bool isCountable = true;
    private Coroutine counterCoroutine;

    private int currentDamage;
    private int[] damages = new int[3];

    private bool isAttackingPlayer = false;
    private IDamagable target = null;   //싱글 플레이어이므로 단일 변수이지만, 추후 싱글이 아닌 멀티이거나, 몬스터끼리 공격하는 상황이 발생한다면 List<IDamagable> 로 바꿀 것.

    private void Start()
    {
        monster = GetComponentInParent<MonsterBase>();
        if (monster == null)
        {
            Debug.LogError("MonsterAttackController: MonsterBase component not found!");
            return;
        }
        playerLayer = LayerMask.GetMask("Player");
    }

    private void FixedUpdate()
    {
        Attack();
    }
    public void ResetDamages()
    {
        this.damages[0] = 0;
        this.damages[1] = 0;
        this.damages[2] = 0;
        currentDamage = 0;
    }
    public void SetDamages(int damage1, int damage2 = 0, int damage3 = 0)
    {
        this.damages[0] = damage1;
        this.damages[1] = damage2;
        this.damages[2] = damage3;
        currentDamage = damage1;
    }

    protected void SetCurrentDamageAsDamage1()
    {
        currentDamage = this.damages[0];
    }

    protected void SetCurrentDamageAsDamage2()
    {
        currentDamage = this.damages[1];
    }

    protected void SetCurrentDamageAsDamage3()
    {
        currentDamage = this.damages[2];
    }

    /// <summary>
    /// 투사체 생성 메서드 (몬스터 로컬 좌표) 투사체 파괴는 각 투사체가 자체적으로 함.
    /// </summary>
    /// <param name="fullPath"></param> <param name="localPos"></param> <param name="faceRight"></param> <param name="damage"></param> <param name="isCountable"></param>
    public void InstantiateProjectile(string fullPath, Vector3 localPos, bool faceRight, int damage, bool isCountable = true)
    {        
        // 투사체 생성
        GameObject go = ObjectPoolManager.Instance.GetObject(fullPath);
        if (go == null)
        {
            Debug.LogError($"[Projectile] 풀에서 꺼내기 실패 : {fullPath}");
            return;
        }
        
        // 월드 위치 배치 (localPos는 몬스터 기준 로컬 좌표)
        Vector3 worldPos = transform.TransformPoint(localPos);
        go.transform.position = worldPos;
        go.transform.rotation = Quaternion.identity;

        // 스프라이트 좌/우 스케일 결정
        var scale = go.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (faceRight ? 1f : -1f);
        go.transform.localScale = scale;

        // 투사체 Init 호출
        if (!go.TryGetComponent<ProjectileBase>(out var projectileBase))
        {
            Debug.LogError($"{go.name} 에 ProjectileBase가 없습니다!");
            return;
        }
        projectileBase.SetPoolKey(fullPath);
        projectileBase.Init(damage, isCountable);
    }

    /// <summary>
    /// 투사체 생성 메서드 (월드 좌표) 투사체 파괴는 각 투사체가 자체적으로 함.
    /// </summary>
    /// <param name="fullPath"></param> <param name="worldPos"></param> <param name="faceRight"></param> <param name="damage"></param> <param name="isCountable"></param>
    public void InstantiateProjectileAtWorld(string fullPath, Vector3 worldPos, bool faceRight, int damage, bool isCountable = true)
    {
        // 투사체 생성
        GameObject go = ObjectPoolManager.Instance.GetObject(fullPath);
        if (go == null)
        {
            Debug.LogError($"[Projectile] 풀에서 꺼내기 실패 : {fullPath}");
            return;
        }

        // 월드 위치 배치
        go.transform.position = worldPos;
        go.transform.rotation = Quaternion.identity;

        // 스프라이트 좌/우 스케일 결정
        var scale = go.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (faceRight ? 1f : -1f);
        go.transform.localScale = scale;

        // 투사체 Init 호출
        if (!go.TryGetComponent<ProjectileBase>(out var projectileBase))
        {
            Debug.LogError($"{go.name} 에 ProjectileBase가 없습니다!");
            return;
        }
        projectileBase.SetPoolKey(fullPath);
        projectileBase.Init(damage, isCountable);
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
        if (isAttackingPlayer && target != null && currentDamage > 0)
        {
            target.TakeDamage(currentDamage);
            if (counterCoroutine == null)
            {
                counterCoroutine = StartCoroutine(DelayForNextCounter(PlayerManager.Instance.player.Data.invincibleTime));
            }
        }
    }

    private IEnumerator DelayForNextCounter(float delay)
    {
        isCountable = false;
        yield return new WaitForSeconds(delay);
        isCountable = true;
        counterCoroutine = null;
    }

    public bool CounterAttacked()
    {
        Debug.Log($"[몬스터] CounterAttacked called");
        if (isCountable)
        {
            Debug.Log($"[몬스터] {monster.MonsterData.monsterName} (ID: {monster.MonsterData.monsterId}) was counterattacked!");
            if (PlayerManager.Instance.player.Attack.successJustAttack || PlayerManager.Instance.player.Attack.successParry)
            {
                monster.Condition.Stunned();
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    public void SetIsCountable(bool boolen) => isCountable = boolen;
    public bool GetIsCountable() => isCountable;
}
