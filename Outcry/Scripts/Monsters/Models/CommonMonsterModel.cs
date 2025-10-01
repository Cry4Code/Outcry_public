using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CommonMonsterModel : MonsterModelBase
{
    public int[] commonSkillsIds;

    public float disdetectRange;
    public float patrolSpeed;

    public CommonMonsterModel(
        int monsterId, string monsterName, int health, float chaseSpeed, float approachRange, float detectRange,
        float disdetectRange, float patrolSpeed,
        int[] commonSkillIds) :
        base(monsterId, monsterName, health, chaseSpeed, approachRange, detectRange)
    {
        this.disdetectRange = disdetectRange;
        this.patrolSpeed = patrolSpeed;

        this.commonSkillsIds = commonSkillIds;
    }

    public CommonMonsterModel(CommonMonsterModel commonMonsterModel) : base(commonMonsterModel)
    {
        this.commonSkillsIds = (int[])commonMonsterModel.commonSkillsIds.Clone();
        this.disdetectRange = commonMonsterModel.disdetectRange;
        this.patrolSpeed = commonMonsterModel.patrolSpeed;
    }
}
