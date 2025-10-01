using System;
using System.Collections.Generic;
using System.Linq;


[Serializable]
public class PlayerSkillDataList : DataListBase<PlayerSkillModel>
{
    public override void Initialize()
    {
        dataList = new List<PlayerSkillModel>();
    }

    public bool TryGetPlayerSkillModelData(int skillId, out PlayerSkillModel skillModel)
    {
        skillModel = dataList.FirstOrDefault(data => data.skillId == skillId);
        return skillModel != null;
    }
    
}
