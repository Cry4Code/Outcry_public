using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StoreManager : Singleton<StoreManager>
{
    //데이터 테이블 매니저에서 스킬 데이터를 받아온다
    //유저 데이터를 통해서 가지고 있는 스킬, 가지고 있는 소울과 그 갯수를 가져온다.

    private Dictionary<int, SkillData> skillDict = new Dictionary<int, SkillData>();

    public UserData CurrentUserData { get; private set; }

    private void Awake()
    {
        DataTableManager.Instance.LoadCollectionData<SkillDataTable>();
        DataTableManager.Instance.LoadCollectionData<SoulDataTable>();

    }

    public Dictionary<int,SkillData> InitializeSkillData()
    {
        var dataDict = DataTableManager.Instance.CollectionData;
        if (dataDict.TryGetValue(typeof(SkillData), out object skillTable))
        {
            var originalDict = skillTable as Dictionary<int, IData>;
            skillDict = originalDict.ToDictionary(kvp => kvp.Key, kvp => (SkillData)kvp.Value);
        }
        else
        {
            skillDict = new Dictionary<int, SkillData>();
        }
        return skillDict;
    }



    public List<SkillData> GetOrderedSkills() //스킬을 순서대로 정렬
    {
        var dict = InitializeSkillData();
        return dict.OrderBy(k => k.Key).Select(k => k.Value).ToList(); //세 함수 전부 가비지컬렉터를 괴롭히는 함수(알고있자) 
    }

    public void BindSkillButtonsUnder(Transform btnParents)
    {
        var skills = GetOrderedSkills();
        var buttons = btnParents.GetComponentsInChildren<SkillBtn>(includeInactive: true);

        for (int i = 0; i < skills.Count; i++)
            buttons[i].Bind(skills[i]);
    }

    public bool HaveSoul(int soulId, int amount)
    {
        if (GameManager.Instance.CurrentUserData == null) return false;


        int index = GameManager.Instance.CurrentUserData.AcquiredSouls.FindIndex(s => s.SoulId == soulId);

        if (index == -1) // 해당 소울을 가지고 있지 않은 경우
        {
            return false;
        }

        if (GameManager.Instance.CurrentUserData.AcquiredSouls[index].Count < amount)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
