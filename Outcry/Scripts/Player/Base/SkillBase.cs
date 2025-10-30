using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public abstract class SkillBase
{
    #region 공통
    public int skillId;
    public float cooldown;
    public int needStamina;
    public float lastUsedTime = float.MinValue;
    protected bool useSuccessed = false; // 사용 여부

    protected PlayerController controller;
    
    // 시간 쪼개기 
    protected float startStateTime;
    protected float startAttackTime = 0.01f;
    protected float animRunningTime = 0f;
    protected float animationLength;
    #endregion
    
    #region 딜링기
    public int[] damages;
    #endregion
    
    #region 버프

    public int buffValue;
    public float duration;
    #endregion

    public void Init(int skillId, int[] damages, int buffValue, int needStamina, float cooldown, float duration)
    {
        this.skillId = skillId;
        this.damages = damages;
        this.buffValue = buffValue;
        this.needStamina = needStamina;
        this.cooldown = cooldown;
        this.duration = duration;
    }

    public void SettingController(PlayerController controller)
    {
        this.controller = controller;
        
        var clips = controller.Animator.animator.runtimeAnimatorController.animationClips;

        string animationName = GetType().Name;
        
        var matchedClips = clips.Where(c => c.name.Contains(animationName)).ToList();
        
        if (matchedClips.Count > 0)
        {
            animationLength = matchedClips.Sum(c => c.length);
        }
        else
        {
            // 매칭되는 게 없으면 기본 동작
            animationLength = 0;
        }
    }

    public virtual void Enter()
    {
        useSuccessed = false;
        if (!ConditionCheck())
        {
            if (controller.Move.rb.velocity.y != 0)
            {
                controller.ChangeState<FallState>();
            }
            else
            {
                controller.ChangeState<IdleState>();
            }
            return;
        }
        useSuccessed = true;
        
        controller.PlayerInputDisable();
        controller.Move.rb.velocity = Vector2.zero;
        
        controller.isLookLocked = true;
        controller.Move.ForceLook(controller.transform.localScale.x < 0);
        controller.Condition.isCharge = false;
        controller.Condition.isSuperArmor = true;
        
        animRunningTime = 0f;
        lastUsedTime = Time.time;
        UIManager.Instance.GetUI<HUDUI>()?.StartSkillCooldownById(skillId);
        
        controller.Animator.SetIntAniamtion(AnimatorHash.PlayerAnimation.AdditionalAttackID, skillId);
        controller.Animator.SetTriggerAnimation(AnimatorHash.PlayerAnimation.AdditionalAttack);
    }
    public abstract void LogicUpdate();

    public virtual bool ConditionCheck()
    {
        // 쿨타임 / 스태미나 체크
        bool canUse = controller.Condition.CheckCooldown(lastUsedTime, cooldown) &&
                      controller.Condition.TryUseStamina(needStamina);

        return canUse;        
    }

    public virtual void Exit()
    {
        Debug.Log($"[플레이어] 스킬 종료");
        controller.PlayerInputEnable();
        controller.Move.rb.gravityScale = 1f;
        if (useSuccessed)
        {
            UGSManager.Instance.LogSkillUsage(StageManager.Instance.CurrentStageData.Stage_id, skillId);
        }
        controller.Condition.isCharge = false;
        controller.Condition.isSuperArmor = false;
    }
}
