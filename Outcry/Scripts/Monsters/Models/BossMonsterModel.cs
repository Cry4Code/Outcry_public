using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BossMonsterModel : MonsterModelBase
{
    public int[] specialSkillIds;
    public int[] commonSkillIds;

    public BossMonsterModel(
        int monsterId, string monsterName, int health, float chaseSpeed, float approachRange, float detectRange,
        int[] specialSkillIds, int[] commonSkillIds) : 
        base(monsterId, monsterName, health, chaseSpeed, approachRange, detectRange)
    {
        this.specialSkillIds = specialSkillIds;
        this.commonSkillIds = commonSkillIds;
    }

    public BossMonsterModel(BossMonsterModel monsterModel) : base(monsterModel)
    {
        this.specialSkillIds = (int[])monsterModel.specialSkillIds.Clone();
        this.commonSkillIds = (int[])monsterModel.commonSkillIds.Clone();
    }
}
