using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.Serialization;

public class PlayerCondition : MonoBehaviour, IDamagable
{
    [Header("Stat Settings")]
    public Condition health;
    public Condition stamina;
    public float startRecoveryStaminaTime; // 스태미나 회복 최소 시간
    public Observable<bool> canStaminaRecovery;
    private float recoveryElapsedTime;
    private float recoveryFullTime;
    private float recoveryStaminaThresholdTime;
    private float k; // k = (최대 스태미나) / (최대 회복시간)^2

    [Header("Invincible Settings")] 
    private bool isInvincible;
    public float invincibleTime; // 한 대 맞았을 때 무적 초 (일단은 1초)
    public bool isCharge = false;
    public bool isSuperArmor = false;
    private WaitForSecondsRealtime waitInvisible;
    
    [Header("Obstacle Settings")]
    [HideInInspector] public Observable<bool> behindObstacle;

    private bool needCheckObstacle = false;
    private Vector3 boundX;
    private Vector3 boundY;
    public LayerMask obstacleMask;


    [Header("Potion Settings")] 
    [HideInInspector] public Observable<bool> getPotion; // 포션 먹기 시작했는지!
    
    public int potionCount = 1;
    public int potionHealthRecovery = 3;

    [Header("Damaged Feedback Settings")] 
    public float flashTime;
    public float flashSpeed;
    
    
    
    private PlayerController controller;
    private Coroutine invincibleCoroutine;
    [HideInInspector] public Observable<bool> isDead;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        canStaminaRecovery = new Observable<bool>(EventBusKey.ChangeStaminaRecovery, false);
        isDead = new Observable<bool>(EventBusKey.ChangePlayerDead, false);
        getPotion = new Observable<bool>(EventBusKey.GetPotion, false);
        behindObstacle = new Observable<bool>(EventBusKey.ChangeHideObstacle, false);
    }
    
    

    private void ConditionSettings()
    {
        EventBus.Unsubscribe(EventBusKey.ChangeStaminaRecovery, OnStaminaRecoveryChanged);
        EventBus.Unsubscribe(EventBusKey.ChangeStamina, OnStaminaChanged);
        EventBus.Unsubscribe(EventBusKey.ChangeHideObstacle, OnHideObstacleChanged);
        // 테스트 코드
        /*EventBus.Unsubscribe(EventBusKey.GetPotion, TestGetPotion);*/
        
        
        health.maxValue = controller.Data.maxHealth;
        stamina.maxValue = controller.Data.maxStamina;
        
        health.Init(EventBusKey.ChangeHealth, health.maxValue);
        stamina.Init(EventBusKey.ChangeStamina, stamina.maxValue);
        
        EventBus.Subscribe(EventBusKey.ChangeStaminaRecovery, OnStaminaRecoveryChanged);
        EventBus.Subscribe(EventBusKey.ChangeStamina, OnStaminaChanged);
        EventBus.Subscribe(EventBusKey.ChangeHideObstacle, OnHideObstacleChanged);
        // 테스트 코드
        /*EventBus.Subscribe(EventBusKey.GetPotion, TestGetPotion);*/
    }
    
    private void OnDisable()
    {
        EventBus.Unsubscribe(EventBusKey.ChangeStaminaRecovery, OnStaminaRecoveryChanged);
        EventBus.Unsubscribe(EventBusKey.ChangeStamina, OnStaminaChanged);
        EventBus.Unsubscribe(EventBusKey.ChangeHideObstacle, OnHideObstacleChanged);
    }

    void Start()
    {
        ConditionSettings();
        invincibleTime = controller.Data.invincibleTime;
        startRecoveryStaminaTime = controller.Data.rateStamina;
        recoveryFullTime = controller.Data.fullStamina;
        k = (stamina.maxValue) / Mathf.Pow(recoveryFullTime, 2);
        Debug.Log($"[플레이어] 스태미나 상수 k = {k}");
        waitInvisible = new WaitForSecondsRealtime(invincibleTime);
        invincibleCoroutine = null;
        canStaminaRecovery.Value = true;
        isDead.Value = false;
        boundX = new Vector2(controller.Move.boxCollider.size.x * 0.3f, 0);
        boundY = new Vector2(0, controller.Move.boxCollider.size.y * 0.3f);
    }

    void FixedUpdate()
    {
        /*try
        {
            stamina.CurValue();
        }
        catch (Exception e)
        {
            Debug.LogError("[플레이어] 스태미나 초기화 안됨");
            ConditionSettings();
        }*/
        if(canStaminaRecovery.Value && stamina.CurValue() < stamina.maxValue)
        {
            if (Time.time > recoveryStaminaThresholdTime)
            {
                recoveryElapsedTime += Time.fixedDeltaTime;
                var tempStamina = Mathf.FloorToInt(k * (Mathf.Pow(recoveryElapsedTime, 2)));
                stamina.SetCurValue(Mathf.Min(stamina.maxValue, tempStamina));
            }
        }

        /*if (needCheckObstacle)
        {
            Debug.Log("[장애물] 플레이어가 장애물 충돌 확인 중");
            behindObstacle = IsBehindObstacle();
        }*/
        
        behindObstacle.Value = IsBehindObstacle();

        if (behindObstacle.Value)
        {
            Debug.Log("[장애물] 플레이어가 장애물 안에 있음");
        }
        else
        {
            Debug.Log("[장애물] 플레이어가 장애물 안에 없음");
        }
        

        if (health.CurValue() <= 0f && !isDead.Value)
        {
            Die();
        }
    }



    public void TakeDamage(int damage)
    {
        Debug.Log($"[TakeDamage] 불림. invincibleCoroutine = {invincibleCoroutine != null} |  isCharge = {isCharge} | isSuperArmor = {isSuperArmor}");
        if (isInvincible) return;
        if (isDead.Value)
        {
            /*if (!controller.Animator.animator.GetAnimatorTransitionInfo(0).IsName("Die"))
            {
                controller.ChangeState<DieState>();
            }*/
            return;
        }
        if (controller.Attack.successParry)
        {
            if (invincibleCoroutine != null) return;
            invincibleCoroutine = StartCoroutine(Invincible(controller.Data.parryInvincibleTime));
        }
        if(!isCharge) invincibleCoroutine = StartCoroutine(Invincible());
        Debug.Log("[플레이어] 플레이어 데미지 받음");
        health.Substract(damage);
        controller.Animator.DamagedFeedback(flashTime, flashSpeed);
        Debug.Log($"[플레이어] 플레이어 현재 체력 : {health.CurValue()}");            
        if(!controller.IsCurrentState<DamagedState>() && !isSuperArmor) controller.ChangeState<DamagedState>();
    }

    public void SetInvincible(float time)
    {
        if (!isInvincible)
        {
            StartCoroutine(Invincible(time));
        }
    }

    public void NoMoreInvincible()
    {
        isInvincible = false;
        if(invincibleCoroutine != null) StopCoroutine(invincibleCoroutine);
    }

    IEnumerator Invincible()
    {
        Debug.Log("[플레이어] 플레이어 무적 시작");
        isInvincible = true;
        yield return waitInvisible;
        isInvincible = false;
        invincibleCoroutine = null;
        Debug.Log("[플레이어] 플레이어 무적 끝");
    }

    IEnumerator Invincible(float time)
    {
        Debug.Log("[플레이어] 플레이어 무적 시작");
        isInvincible = true;
        yield return new WaitForSecondsRealtime(time);
        isInvincible = false;
        invincibleCoroutine = null;
        Debug.Log("[플레이어] 플레이어 무적 끝");
    }

    public bool TryUseStamina(int useStamina)
    {
        if (stamina.CurValue() - useStamina >= 0)
        {
            canStaminaRecovery.Value = false;
            stamina.Substract(useStamina);
            Debug.Log($"[플레이어] 스태미나 {useStamina} 사용. 현재 스태미나 {stamina.CurValue()}");
            canStaminaRecovery.Value = true;
            return true;
        }
        else
        {
            Debug.Log($"[플레이어] 스태미나 {useStamina} 사용 불가");
            canStaminaRecovery.Value = true;
            return false;
        }
        
    }

    private void Die()
    {
        isDead.Value = true;
        Debug.Log("[플레이어] 죽음!");
        controller.ChangeState<DieState>();
        controller.Inputs.Player.Disable();
    }

    private void OnStaminaRecoveryChanged(object data)
    {
        // RecoveryChanged가 True 로 바꼈을 때
        if ((bool)data)
        {
            // Debug.Log("[플레이어] 스태미나 리커버리 켜짐");
            recoveryElapsedTime = Mathf.Sqrt(stamina.CurValue() / k);
            recoveryStaminaThresholdTime = Time.time + startRecoveryStaminaTime; // 1초 뒤에 시작할 수 있게
            // Debug.Log($"[플레이어] 스태미나 상수 k 에 곱해진 현재 스태미나 : {Mathf.Pow(stamina.CurValue(), 2)}");
            // Debug.Log($"[플레이어] 스태미나 리커버리 시간 {recoveryElapsedTime}");
        }
        else
        {
            recoveryElapsedTime = 0;
            // Debug.Log("[플레이어] 스태미나 리커버리 꺼짐");
        }
    }

    private void OnStaminaChanged(object data)
    {
        if ((int)data < stamina.maxValue)
        {
            canStaminaRecovery.Value = true;
        }
        else
        {
            canStaminaRecovery.Value = false;
        }
    }

    private bool IsBehindObstacle()
    {
        Vector2[] origins = new Vector2[4]
        {
            transform.position + boundX,
            transform.position - boundX,
            transform.position + boundY,
            transform.position - boundY
        };

        for (int i = 0; i < 4; i++)
        {
            if (!Physics2D.Raycast(origins[i], Vector2.up, 0.01f, obstacleMask))
            {
            
                return false;
            }
        }

        return true;
    }

    private void OnHideObstacleChanged(object data)
    {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        controller.Hitbox.spriteRenderer.GetPropertyBlock(mpb);
        if ((bool)data)
        {
            mpb.SetColor("_Color", new Color(1, 1, 1, 0.5f));
        }
        else
        {
            mpb.SetColor("_Color", Color.white);
        }
        controller.Hitbox.spriteRenderer.SetPropertyBlock(mpb);
    }

    
    // 테스트 코드. 참고하세요
    /*
    public void TestGetPotion(object data)
    {
        if ((bool)data)
        {
            Debug.Log("[플레이어] 포션 먹기 시작");
        }
        else
        {
            Debug.Log("[플레이어] 포션 먹기 끝");
        }
    }*/
    
    
}
