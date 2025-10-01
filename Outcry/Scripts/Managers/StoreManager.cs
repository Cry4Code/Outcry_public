using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StoreManager : Singleton<StoreManager>
{
    //데이터 테이블 매니저에서 스킬 데이터를 받아온다
    //유저 데이터를 통해서 가지고 있는 스킬, 가지고 있는 소울과 그 갯수를 가져온다.

    private Dictionary<int, SkillData> skillDict = new Dictionary<int, SkillData>();

    private void Awake()
    {
        DataTableManager.Instance.LoadCollectionData<SkillDataTable>();

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




    public void Buy()
    {
        //스킬 미리보기 버튼이 눌려있으면 해당 스킬 구매
        //유저 정보에 스킬 추가 하기
    }

    public void Sell()
    {

        //스킬 미리보기 버튼이 눌려있을 때 이미 구매한 스킬이라면 해당 스킬 판매
        //유저 정보에 스킬 추가 하기
    }

    public void ActiveBtn()//ui 에서 해야 할 수도 있음
    {
        //스킬 미리보기 버튼이 눌려있으면 해당 스킬 구매
        //유저 정보에 스킬 추가 하기
    }

    public void ActPreview(int btnNum)
    {
        //버튼이 활성화 되어있다면
        //스킬을 해금하는데 필요한 소울을 가지고 있다면 해당 소울을 가
        //만약 다른 스킬이 미리보기중이면 종료
        //누른 스킬 번호를 받아와서 미리보기창에 반복 재생
    }

}
