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
        animationLength = 
            controller.Animator.animator.runtimeAnimatorController
                .animationClips.First(c => c.name == $"{GetType().Name}").length;
    }
    
    public abstract void Enter();
    public abstract void LogicUpdate();

    public virtual void Exit()
    {
        Debug.Log($"[플레이어] 스킬 종료");
        controller.PlayerInputEnable();
        controller.Move.rb.gravityScale = 1f;
        if(useSuccessed) lastUsedTime = Time.time;
        controller.Condition.isCharge = false;
    }
}
