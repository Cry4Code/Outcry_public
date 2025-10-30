using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;

[Flags]
public enum ePlayerBuff
{
    None = 0,
    PowerUp = 1 << 1,
    DeadHard = 1 << 2
}


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
    public Observable<bool> cantUseCuzStamina;
    public Observable<bool> cantUseCuzCooldown;

    [Header("Invincible Settings")] 
    public bool GodMode;
    private bool isInvincible;
    public float invincibleTime; // 한 대 맞았을 때 무적 초 (일단은 1초)
    public bool isCharge = false;
    public bool isSuperArmor = false;
    private WaitForSecondsRealtime waitInvisible;
    
    [Header("Obstacle Settings")]
    public LayerMask obstacleMask;

    private bool needCheckObstacle = false;
    private Vector3 boundX;
    private Vector3 boundY;
    [HideInInspector] public Observable<bool> behindObstacle;


    [Header("Potion Settings")] 
    public int potionInitialCount = 3;
    public int potionHealthRecovery = 3;

    [HideInInspector] public Observable<int> potionCount;
    [HideInInspector] public Observable<bool> getPotion; // 포션 먹기 시작했는지!

    [Header("Damaged Feedback Settings")] 
    public float flashTime;
    public float flashSpeed;
    
    [Header("Buff Settings")]
    [SerializeField] public ePlayerBuff playerBuff;
    
    private PlayerController controller;
    private Coroutine invincibleCoroutine;
    [HideInInspector] public Observable<bool> isDead;

    public bool WasHitThisStage;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        canStaminaRecovery = new Observable<bool>(EventBusKey.ChangeStaminaRecovery, false);
        potionCount = new Observable<int>(EventBusKey.ChangePotionCount, potionInitialCount);
        cantUseCuzStamina = new Observable<bool>(EventBusKey.CantUseCuzStamina, false);
        cantUseCuzCooldown = new Observable<bool>(EventBusKey.CantUseCuzCooldown, false);
        isDead = new Observable<bool>(EventBusKey.ChangePlayerDead, false);
        getPotion = new Observable<bool>(EventBusKey.GetPotion, false);
        behindObstacle = new Observable<bool>(EventBusKey.ChangeHideObstacle, false);

        WasHitThisStage = false;
    }

    private void ConditionSettings()
    {
        EventBus.Unsubscribe(EventBusKey.ChangeStaminaRecovery, OnStaminaRecoveryChanged);
        EventBus.Unsubscribe(EventBusKey.ChangeStamina, OnStaminaChanged);
        EventBus.Unsubscribe(EventBusKey.ChangeHideObstacle, OnHideObstacleChanged);
        EventBus.Unsubscribe(EventBusKey.CantUseCuzStamina, CannotActionCuzStamina);
        EventBus.Unsubscribe(EventBusKey.CantUseCuzCooldown, CannotActionCuzCooldown);
        
        health.maxValue = controller.Data.maxHealth;
        stamina.maxValue = controller.Data.maxStamina;
        
        health.Init(EventBusKey.ChangeHealth, health.maxValue);
        stamina.Init(EventBusKey.ChangeStamina, stamina.maxValue);
        
        EventBus.Subscribe(EventBusKey.ChangeStaminaRecovery, OnStaminaRecoveryChanged);
        EventBus.Subscribe(EventBusKey.ChangeStamina, OnStaminaChanged);
        EventBus.Subscribe(EventBusKey.ChangeHideObstacle, OnHideObstacleChanged);
        EventBus.Subscribe(EventBusKey.CantUseCuzStamina, CannotActionCuzStamina);
        EventBus.Subscribe(EventBusKey.CantUseCuzCooldown, CannotActionCuzCooldown);
    }
    
    private void OnDisable()
    {
        EventBus.Unsubscribe(EventBusKey.ChangeStaminaRecovery, OnStaminaRecoveryChanged);
        EventBus.Unsubscribe(EventBusKey.ChangeStamina, OnStaminaChanged);
        EventBus.Unsubscribe(EventBusKey.ChangeHideObstacle, OnHideObstacleChanged);
        EventBus.Unsubscribe(EventBusKey.CantUseCuzStamina, CannotActionCuzStamina);
        EventBus.Unsubscribe(EventBusKey.CantUseCuzCooldown, CannotActionCuzCooldown);
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
            Debug.Log("[StageManager] 체력 소모로 인한 플레이어 사망");
            Die();
        }
    }



    public void TakeDamage(int damage)
    {
        Debug.Log($"[TakeDamage] 불림. invincibleCoroutine = {invincibleCoroutine != null} |  isCharge = {isCharge} | isSuperArmor = {isSuperArmor}");
        if (GodMode) return;
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
            return;
        }
        if(!isCharge) invincibleCoroutine = StartCoroutine(Invincible());
        Debug.Log("[플레이어] 플레이어 데미지 받음");
        health.Substract(damage);

        WasHitThisStage = true;

        controller.Animator.DamagedFeedback(flashTime, flashSpeed);
        EffectManager.Instance.PlayEffectsByIdAsync(PlayerEffectID.Damaged, EffectOrder.Player, controller.gameObject)
            .Forget();
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

    public void DeadHard(float time)
    {
        StartCoroutine(Invincible(time, true));
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

    IEnumerator Invincible(float time, bool isBuff = false)
    {
        if (isBuff)
        {
            controller.Condition.playerBuff |= ePlayerBuff.DeadHard;
        }
        Debug.Log("[플레이어] 플레이어 무적 시작");
        isInvincible = true;
        yield return new WaitForSecondsRealtime(time);
        // 만약에 이 코루틴을 부른 이유가 버프가 아니고
        // 지금 DeadHard가 켜져있는 상태면 무적을 끄면 안됨.
        // ex) 데드하드 킨 채로 패링이면 무적 끄면 안됨
        if (!isBuff && controller.Condition.playerBuff.HasFlag(ePlayerBuff.DeadHard))
        {
            yield break;
        }
        isInvincible = false;
        invincibleCoroutine = null;
        if (isBuff)
        {
            controller.Condition.playerBuff &= ~ePlayerBuff.DeadHard;
        }
        Debug.Log("[플레이어] 플레이어 무적 끝");
    }

    public bool TryUseStamina(int useStamina)
    {
        if (GodMode) return true;
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
            // 스태미나 때문에 사용 불가능한거 티내기
            cantUseCuzStamina.Value = true;
            Debug.Log($"[플레이어] 스태미나 {useStamina} 사용 불가");
            canStaminaRecovery.Value = true;
            cantUseCuzStamina.Value = false;
            return false;
        }
        
    }

    public bool CheckCooldown(float lastUsedTime, float cooldown)
    {
        if (Time.time - lastUsedTime < cooldown)
        {
            cantUseCuzCooldown.Value = true;
            cantUseCuzCooldown.Value = false;
            return false;
        }
        return true;
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

    private void CannotActionCuzStamina(object o)
    {
        if ((bool)o)
        {
            Debug.Log("[CantUse] Because You have no stamina");
        }
    }

    private void CannotActionCuzCooldown(object o)
    {
        if ((bool)o)
        {
            Debug.Log("[CantUse] Because It's Cooldown");
        }
    }
    
    
}
