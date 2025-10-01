using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class MonsterAIBase : MonoBehaviour //MonoBehaviour 상속 안받아도 되는거 아닌감...? 근데 일단 인스펙터에서 확인해야하므로 상속 받게 함.
{
    protected MonsterBase monster;  //model은 이걸 타고 접근하는 걸로.
    
    [SerializeField] protected SelectorNode rootNode;
    [SerializeField] protected PlayerController target;

    public AIBlackBoard blackBoard = new AIBlackBoard();
    private bool subscribed;

    [SerializeField] protected bool reactToPotion = true;  // 포션 이벤트에 반응하는 몬스터는 true
    [SerializeField] protected bool useLatch = true;  // 구독 이벤트가 true가 되는 순간만 트리거로 true를 넘겨줄 때 true, 바로 동기화시 false

    private bool isAvailableToAct;
    public bool IsAttacking { get; protected set; }

    private bool prevPotionValue;
    private float spawnAnimationLength;

    public virtual void Initialize(MonsterBase monster) //외부에서 호출되어야함. - 몬스터 베이스에서 호출
    {
        target = PlayerManager.Instance.player;
        if (monster == null)
        {
            Debug.LogError("MonsterAI: MonsterBase component not found!");
            return;
        }
        this.monster = monster;

        // 구독 모드 반영
        ConfigurePotionOverrideModes();

        // 이벤트 구독
        if (reactToPotion && !subscribed)
        {
            EventBus.Subscribe(EventBusKey.GetPotion, OnPotionEvent);
            subscribed = true;
        }

        // BT 초기화
        InitializeBehaviorTree();
        IsAttacking = false;
        isAvailableToAct = false;
        
        //spawn 애니메이션 길이 가져오기
        if (!AnimatorUtility.TryGetAnimationLengthByNameHash(monster.Animator, AnimatorHash.MonsterAnimation.Spawn,
                out spawnAnimationLength) || spawnAnimationLength <= 0f)
        {
            Debug.Log($"[{gameObject.name}] AI: Spawn Animation length not found!");
        }
        
        StartCoroutine(ActivateMonster());        
    }

    private void OnDisable()
    {
        if (subscribed) // 이벤트 구독 해제
        {
            EventBus.Unsubscribe(EventBusKey.GetPotion, OnPotionEvent);
            subscribed = false;
        }
    }

    private void OnDestroy()
    {
        if (subscribed) // 이벤트 구독 해제
        {
            EventBus.Unsubscribe(EventBusKey.GetPotion, OnPotionEvent);
            subscribed = false;
        }
    }
        
    private IEnumerator ActivateMonster()
    {
        monster.Condition.SetInivincible(true);
        // 스폰 애니메이션이 있을 떄만 대기
        if (spawnAnimationLength > 0f)
            yield return new WaitForSeconds(spawnAnimationLength);
        
        isAvailableToAct = true;
        monster.Condition.SetInivincible(false);
    }

    #region AI, BT 관련 메서드
    protected abstract void InitializeBehaviorTree(); 
    
    public void UpdateAI()
    {
        if (!isAvailableToAct || monster.Animator.speed < 1f)
            return;
        if (rootNode == null)
        {
            Debug.LogWarning("Root node is not assigned.");
            return;
        }

        NodeState state = rootNode.Tick();
    }
    
    public void DeactivateBt()
    {
        isAvailableToAct = false;
    }

    public void ActivateBt()
    {
        isAvailableToAct = true;
    }
    #endregion

    #region 이벤트 관련 메서드
    /// <summary>
    /// 파생 AI에서 이벤트 반응 모드를 조정할 수 있는 메서드, reactToPotion, useLatch 수정은 이 메서드에서 하기를 권장
    /// </summary>
    protected virtual void ConfigurePotionOverrideModes()
    {

    }

    private void OnPotionEvent(object data)
    {
        bool value = (data is bool b) && b;

        // 바로 동기화
        blackBoard.PotionOverrideSync = value;

        // 엣지-래치 (false -> true 순간에만 on)
        if (useLatch && value && !prevPotionValue)
        {
            blackBoard.PotionOverrideEdge = true;
        }

        prevPotionValue = value;        
    }

    /// <summary>
    /// 스킬 시작 시 1회성 래치 소모
    /// </summary>
    /// <returns></returns>
    public bool TryConsumePotionEdge()
    {
        if (!blackBoard.PotionOverrideEdge) return false;
        
        blackBoard.PotionOverrideEdge = false;
        return true;
    }
    #endregion
}
