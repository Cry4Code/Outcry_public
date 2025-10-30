using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    #region 데미지 관련

    [HideInInspector] public int AttackDamage = 0;
    [HideInInspector] public int AdditionalDamage = 0;
    [HideInInspector] public int[] AttackDamageList = new int[2];
    public Coroutine buffAttackCoroutine = null;
    #endregion
    
    
    #region 기본 공격 관련

    [field: Header("Normal Attack")] 
    public int AttackCount = 0;
    public int MaxAttackCount = 2;
    public bool HasJumpAttack = false;

    #endregion
    
    #region 패링 관련

    [field: Header("Parry")] 
    public bool isStartParry = false;
    public bool successParry = false;
    #endregion
    
    #region 섬단 관련

    [field: Header("Special Attack")] 
    public bool isStartSpecialAttack = false;
    public bool isStartJustAttack = false;
    public bool successJustAttack = false;
    public float justAttackStopTime = 5f;
    public Coroutine justAttackCoroutine = null;
    public Vector2 justAttackStartPosition;
    
    #endregion
    
    private PlayerController controller;

    public void Init(PlayerController controller)
    {
        this.controller = controller;
    }
    
    public void ClearAttackCount()
    {
        AttackCount = 0;
    }

    public void SetDamage(int damage)
    {
        AttackDamage = damage;
    }

    public void SetDamageList(int[] damageList)
    {
        AttackDamageList = damageList;
    }

    public void SetDamageInList(int index)
    {
        if (index < AttackDamageList.Length)
        {
            AttackDamage = AttackDamageList[index];
        }
        else
        {
            Debug.LogError($"[플레이어] {index} 는 AttackDamageList 크기를 넘어감");
            return;
        }
    }

    public void BuffDamage(int damage, float time)
    {
        if (buffAttackCoroutine != null)
        {
            StopCoroutine(buffAttackCoroutine);
        }

        buffAttackCoroutine = StartCoroutine(AddDamageInTime(damage, time));
    }

    IEnumerator AddDamageInTime(int damage, float time)
    {
        AdditionalDamage += damage;
        controller.Condition.playerBuff |= ePlayerBuff.PowerUp;
        Debug.Log($"[플레이어] 데미지 버프 됨 ! -> {AdditionalDamage}");
        yield return new WaitForSecondsRealtime(time);
        AdditionalDamage = Mathf.Max(0, AdditionalDamage - damage);
        controller.Condition.playerBuff &= ~ePlayerBuff.PowerUp;
        Debug.Log($"[플레이어] 데미지 버프 끝 ! -> {AdditionalDamage}");
    }
    

    // TODO : 얘네 나중에 이펙트 매니저로 옮겨야됨요 
    // 진짜 진짜 나중에 생각하재요
    public void JustSpecialAttack(Animator monsterAnimator)
    {
        if (justAttackCoroutine != null)
        {
            StopCoroutine(justAttackCoroutine);
        }
        justAttackCoroutine = StartCoroutine(TimeStop(monsterAnimator, justAttackStopTime));
    }

    IEnumerator TimeStop(Animator monsterAnimator, float time)
    {
        var mc = monsterAnimator.GetComponentInParent<MonsterCondition>();

        // 별로면 이 부분만 빼면 됨
        controller.Animator.animator.speed = 0f;
        monsterAnimator.speed = 0f;

        bool interrupted = false;
        void OnMonsterDeath() { interrupted = true; }
        mc.OnDeath += OnMonsterDeath;

        float elapsed = 0f;
        while (elapsed < time && !interrupted)
        {
            yield return null;
            elapsed += Time.unscaledDeltaTime;
        }
        mc.OnDeath -= OnMonsterDeath;
        //yield return new WaitForSecondsRealtime(time);

        controller.Animator.animator.speed = 1f;
        monsterAnimator.speed = 1f;
        controller.Attack.successJustAttack = true;
        justAttackCoroutine = null;
    }
}
