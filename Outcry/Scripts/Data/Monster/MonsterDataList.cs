using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class MonsterDataList : DataListBase<MonsterModelBase>
{
    public override void Initialize()
    {
        dataList = new List<MonsterModelBase>();
    }
    
    /// <summary>
    /// 데이터리스트에 id에 해당하는 몬스터데이터(MonsterModelBase)가 있다면 true, 없다면 false 반환
    /// </summary>
    /// <param name="skillId"></param>
    /// <param name="monsterSkillData"></param>
    /// <returns></returns>
    public bool TryGetMonsterModelData(int monsterId, out MonsterModelBase monsterData)
    {
        MonsterModelBase tempData = dataList.FirstOrDefault(data => data.monsterId == monsterId);
        
        switch (tempData)
        {
            case BossMonsterModel bossMonsterModel:
                monsterData = new BossMonsterModel(bossMonsterModel);
                break;
            case CommonMonsterModel commonMonsterModel:
                monsterData = new CommonMonsterModel(commonMonsterModel);
                break;
            default:
                monsterData = null;
                break;
        }
        
        if (tempData == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
