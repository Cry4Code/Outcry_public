using System;
[Serializable]
public class PlayerSkillModel
{
    public int skillId;
    public string skillName;
    public int[] damages;
    public int buffValue;
    public int stamina;
    public float cooldown;
    public float duration;
    public string desc;

    public PlayerSkillModel(
        int skillId,
        string skillName,
        int[] damages,
        int buffValue,
        int stamina,
        float cooldown,
        float duration,
        string desc)
    {
        this.skillId = skillId;
        this.skillName = skillName;
        this.damages = damages;
        this.buffValue = buffValue;
        this.stamina = stamina;
        this.cooldown = cooldown;
        this.duration = duration;
        this.desc = desc;
    }
}
