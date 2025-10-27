using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MonsterCondition : MonoBehaviour, IDamagable
{
    private MonsterBase monster;
    
    [field: SerializeField] public int MaxHealth { get; private set; }
    [field: SerializeField] public Condition CurrentHealth { get; private set; }

    public Observable<bool> IsDead;
    public bool IsInvincible { get; private set; } = false;
    
    public Action OnHealthChanged;
    public Action OnDeath;  //todo. think. BT 중지도 여기에 하면 될듯? 그럼 isDead 필요 없음? 고민해봐야할듯.

    private Coroutine animationCoroutine;
    private Color originalColor;
    
    private float stunAnimationLength;
    private float hitAnimationLength;   // 1번 깜박일 정도의 시간으로 수정, stun 애니메이션의 1/6 길이 정도

    private float deathDelay = 1.0f; // 죽었을 때의 대기 시간, 임의로 1초로 설정, 추후 수정 가능

    private List<int> checkRatioList;
    private int lastCheckIndex = 0;
    
    private Coroutine btActivationCoroutine;    

    private void Start()
    {
        monster = GetComponent<MonsterBase>();
        if (monster == null)
        {
            Debug.LogError("MonsterCondition: MonsterBase component not found!");
            return;
        }

        Initialize();
        
        //spawn 애니메이션 길이 가져오기
        if (!AnimatorUtility.TryGetAnimationLengthByNameHash(monster.Animator, AnimatorHash.MonsterAnimation.Stun,
                out stunAnimationLength) || stunAnimationLength <= 0f)
        {
            Debug.LogError("MonsterAI: Animation length not found!");
        }
        // (stunAnimationLength / 0.2f(한 프레임) ) / 6f(스턴 애니메이션 길이의 약 1/6배)
        hitAnimationLength = Mathf.Floor(stunAnimationLength * 0.83f) * 0.2f;   // 0.2초 단위로 맞춤.

        originalColor = monster.SpriteRenderer.color;
        
        IsDead = new Observable<bool>(EventBusKey.ChangeEnemyDead, false);

        // spawn 애니메이션 있으면 애니메이션 실행 동안은 좌우 반전
        if (AnimatorUtility.TryGetAnimationLengthByNameHash(monster.Animator, AnimatorHash.MonsterAnimation.Spawn,
            out float spawnAnimationLength))
        {
            StartCoroutine(FlipWhileSpawn());
        }
    }

    public void Initialize()    //오브젝트 풀이 필요할 것인가? 상정하고 짜뒀음.
    {
        SetMaxHealth();
    }


    private void OnEnable()
    {
        EventBus.Subscribe(EventBusKey.ChangeBossHealth, BossHpRatioCheck);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(EventBusKey.ChangeBossHealth, BossHpRatioCheck);
    }

    private void SetMaxHealth()
    {
        MaxHealth = monster.MonsterData.health;
        CurrentHealth.Init(EventBusKey.ChangeBossHealth, MaxHealth);
        checkRatioList = Enumerable
            .Range(0, 9)
            .Select(i => (90 - i * 10))
            .ToList();
        // [90, 80, 70, 60 ... 10]
        lastCheckIndex = 0;
        
    }

    public void BossHpRatioCheck(object o)
    {
        if (StageManager.Instance.CurrentStageData.Stage_id == StageID.Tutorial) return;
        int currentHealth = (int)o;
        float currentRatio = (float)currentHealth / MaxHealth;
        int currentPercent = (int)(currentRatio * 100f);
        if (lastCheckIndex < checkRatioList.Count)
        {
            if (currentPercent < checkRatioList[lastCheckIndex])
            {
                Debug.Log($"[BossHP] Boss hp : {checkRatioList[lastCheckIndex]} %");
                UGSManager.Instance.LogInGameBossHp(StageManager.Instance.CurrentStageData.Stage_id, checkRatioList[lastCheckIndex], StageManager.Instance.GetElapsedTime());
                lastCheckIndex++;
            }
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (IsDead.Value || IsInvincible)
        {
            return;
        }
        CurrentHealth.Substract(damage);
        if (animationCoroutine != null)
        {
            monster.SpriteRenderer.color = originalColor;
            StopCoroutine(animationCoroutine);
        }

        if (monster is BossMonster bm && CurrentHealth.CurValue() <= MaxHealth / 2f)
        {
            if (bm.Animator.GetBool(AnimatorHash.MonsterParameter.IsTired))
                bm.Animator.SetBool(AnimatorHash.MonsterParameter.IsTired, true);
        }

        bool isPlayerInLeft = PlayerManager.Instance.player.transform.position.x < monster.transform.position.x;
        
        EffectManager.Instance.PlayEffectsByIdAsync(PlayerEffectID.NormalAttack,  EffectOrder.Monster, 
            null, 
            (Vector2)(monster.transform.position) + ((isPlayerInLeft ? -Vector2.right : Vector2.right) * monster.transform.localScale.y)).Forget();
        
        if (CurrentHealth.CurValue() <= 0)
        {
            Death();
            return;
        }

        animationCoroutine = StartCoroutine(HitAnimation(hitAnimationLength));
    }
    
    //빨갛게 점멸하는 이펙트 코루틴
    private IEnumerator HitAnimation(float duration)
    {
        Color hitColor = new Color(1f, 0f, 0f, 1f);
        float flashDuration = 0.1f;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            monster.SpriteRenderer.color = monster.SpriteRenderer.color == Color.red ? originalColor : hitColor;
            yield return new WaitForSeconds(flashDuration);
            elapsedTime += flashDuration;
        }

        monster.SpriteRenderer.color = originalColor;
    }

    public void Stunned()
    {
        if(IsDead.Value || IsInvincible)
            return;

        Debug.Log($"[MonsterCondition] Stun start t={Time.time:0.00}");
        monster.Animator.SetTrigger(AnimatorHash.MonsterParameter.Stun);
        monster.MonsterAI.DeactivateBt();
        OnHealthChanged?.Invoke();

        if (btActivationCoroutine != null)
        {
            StopCoroutine(btActivationCoroutine);
            monster.MonsterAI.ActivateBt();
        }
        StartCoroutine(WaitForBTActivation(stunAnimationLength));
    }
    
    private IEnumerator WaitForBTActivation(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);      

        monster.MonsterAI.ActivateBt();
        Debug.Log("WaitForBTActivation: Activated BT");
    }
    
    private void Death()
    {
        CurrentHealth.SetCurValue(0);
        IsDead.Value = true;
        monster.MonsterAI.DeadCeremony();
        monster.Animator.SetTrigger(AnimatorHash.MonsterParameter.Dead);
        OnDeath?.Invoke();

        StartCoroutine(RemoveAfterDeath());
    }

    private IEnumerator RemoveAfterDeath()
    {
        int deadHash = AnimatorHash.MonsterAnimation.Death;
        AnimatorUtility.TryGetAnimationLengthByNameHash(monster.Animator, deadHash, out float deadLength);

        // 사망 애니메이션 +1초 대기
        yield return new WaitForSeconds(deadLength);
        yield return new WaitForSeconds(deathDelay);

        // 보스 제외, 일반 몬스터만 제거
        if (monster?.MonsterData is BossMonsterModel)
            yield break;

        Destroy(monster.gameObject);
    }

    public void SetInivincible(bool value)    
    {
        IsInvincible = value;
    }

    // 스폰 애니메이션 동안 대기 코루틴
    private IEnumerator FlipWhileSpawn()
    {
        int spawnHash = AnimatorHash.MonsterAnimation.Spawn;
        var anim = monster.Animator;

        // 스폰 진입 짧게 대기
        float waitEnter = 0.1f;
        while (waitEnter > 0f &&
            anim.GetCurrentAnimatorStateInfo(0).shortNameHash != spawnHash)
        {
            waitEnter -= Time.deltaTime;
            yield return null;
        }

        // 반전 적용
        bool originFlipX = monster.SpriteRenderer.flipX;
        monster.SpriteRenderer.flipX = true;

        // 스폰 끝날 때까지 대기
        while (anim.IsInTransition(0) ||
            anim.GetCurrentAnimatorStateInfo(0).shortNameHash == spawnHash)
        {
            yield return null;
        }

        // 반전 복구
        monster.SpriteRenderer.flipX = originFlipX;
    }
}
