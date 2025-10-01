using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterCondition : MonoBehaviour, IDamagable
{
    private MonsterBase monster;
     
    [field: SerializeField] public int MaxHealth { get; private set; }
    [field: SerializeField] public int CurrentHealth { get; private set; }

    public Observable<bool> IsDead;
    public bool IsInvincible { get; private set; } = false;
    
    public Action OnHealthChanged;
    public Action OnDeath;  //todo. think. BT 중지도 여기에 하면 될듯? 그럼 isDead 필요 없음? 고민해봐야할듯.

    private Coroutine animationCoroutine;
    private Color originalColor;
    
    private float stunAnimationLength;
    private float hitAnimationLength;   //일단 stun 애니메이션 길이로 맞춤.

    private float deathDelay = 1.0f; // 죽었을 때의 대기 시간, 임의로 1초로 설정, 추후 수정 가능
    
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
        hitAnimationLength = Mathf.Floor(stunAnimationLength / 0.2f) * 0.2f; //0.2초 단위로 맞춤.
        
        originalColor = monster.SpriteRenderer.color;
        
        IsDead = new Observable<bool>(EventBusKey.ChangeEnemyDead, false);
    }

    public void Initialize()    //오브젝트 풀이 필요할 것인가? 상정하고 짜뒀음.
    {
        SetMaxHealth();
    }

    private void SetMaxHealth()
    {
        MaxHealth = monster.MonsterData.health;
        CurrentHealth = MaxHealth;
    }
    
    public void TakeDamage(int damage)
    {
        if (IsDead.Value || IsInvincible)
        {
            return;
        }
        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        if (animationCoroutine != null)
        {
            monster.SpriteRenderer.color = originalColor;
            StopCoroutine(animationCoroutine);
        }

        if (CurrentHealth <= MaxHealth / 2)
        {
            monster.Animator.SetBool(AnimatorHash.MonsterParameter.IsTired, true );
            if (CurrentHealth <= 0)
            {
                Death();
                return;
            }
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
        monster.Animator.SetTrigger(AnimatorHash.MonsterParameter.Stun);
        monster.MonsterAI.DeactivateBt();
        OnHealthChanged?.Invoke();
        
        StartCoroutine(WaitForBTActivation(stunAnimationLength));
    }
    
    private IEnumerator WaitForBTActivation(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        monster.MonsterAI.ActivateBt();
    }
    
    private void Death()
    {
        CurrentHealth = 0;
        IsDead.Value = true;
        monster.MonsterAI.DeactivateBt();
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
}
