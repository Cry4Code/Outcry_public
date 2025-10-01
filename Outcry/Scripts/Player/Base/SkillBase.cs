using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class SkillBase
{
    #region 공통
    public int skillId;
    public float cooldown;
    public int needStamina;
    public float lastUsedTime;
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
    
    public abstract void Enter(PlayerController controller);
    public abstract void HandleInput(PlayerController controller);
    public abstract void LogicUpdate(PlayerController controller);
    public abstract void Exit(PlayerController controller);
}
