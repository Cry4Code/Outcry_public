using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum MissionType
{
    bossKillAchieve = 0,
    getSkill,
    getSoul,
    noHit,
    death,
    cleardTutorial,
    achieveAll
}

public class AchievementManager : Singleton<AchievementManager>
{
    private Dictionary<int, SkillData> skillDict = new Dictionary<int, SkillData>();
    private Dictionary<int, IData> tableData;
    public UserData CurrentUserData { get; private set; }
    public AchievementsData AchievementsData { get; private set; }

    private int bossKillCount = 0;
    private int deathCount = 0;
    private int skillGetCount = 0;
    private int soulGetCount = 0;
    private int clearAchieve = 0;

    public struct AchievementEntry // 외부에서 사용하기 쉽게 만든 구조체
    {
        public int id;                 // 딕셔너리 키(= 도전과제 ID)
        public AchievementsData data;  // 실제 데이터
    }


    private void Awake()
    {
        DataTableManager.Instance.LoadCollectionData<AchievementsDataTable>();

        var col = DataTableManager.Instance.CollectionData;
        tableData = col != null
            && col.TryGetValue(typeof(AchievementsData), out var raw)
            && raw is Dictionary<int, IData> d ? d : null;

        if (tableData == null)
            Debug.LogError("[AchievementManager] AchievementsData 바인딩 실패 (로드/키/캐스팅 확인)");
    }

    private int getType(int skillId)
    {
        var skillType = DataTableManager.Instance.GetCollectionDataById<AchievementsData>(skillId);

        if (skillType != null)
        {
            int count = skillType.Counts; //타입으로 바꿔야 함!!!!!!!!!!!!!!!!!!!!!!!
            return count;
        }
        else
        {
            Debug.LogWarning("해당 id 에 해당하는 타입이 없습니다");
            return 0;
        }
    }

    /// <summary>
    /// 내부 테이블에서 AchievementsData만 뽑아
    /// (id, data) 리스트로 만들어 ID 오름차순 정렬해서 반환.
    /// </summary>
    public List<AchievementEntry> GetAllAchievementsSorted()
    {
        var result = new List<AchievementEntry>();

        if (tableData == null)
        {
            Debug.LogWarning("[AchievementManager] tableData가 비어있습니다.");
            return result;
        }

        foreach (var kv in tableData)
        {
            if (kv.Value is AchievementsData a)
            {
                result.Add(new AchievementEntry { id = kv.Key, data = a });
            }
        }

        result.Sort((x, y) => x.id.CompareTo(y.id));
        return result;
    }


    public float ShowPersent(int id)
    {

        switch (getType(id))
        { 
            case (int)MissionType.bossKillAchieve:
                return BossKillAchieve(id) ;

            case (int)MissionType.getSkill: 
                return GetSkill(id);

            case (int)MissionType.getSoul: 
                return GetSoul(id);

            case (int)MissionType.noHit: 
                return NoHit(id);

            case (int)MissionType.death: 
                return Death(id);

            case (int)MissionType.cleardTutorial: 
                return CleardTutorial(id);

            case (int)MissionType.achieveAll: 
                return AchieveAll(id);

            default:
                return 0;
        }
    }

    public void isCleared(int id)
    {
        if (ShowPersent(id) == 1)
        {
            //유저데이터에 해당 id 퀘스트 추가
        }
    }

    private float BossKillAchieve(int id)
    {
        float percent = 0f;
        percent = bossKillCount / AchievementsData.Condition; //보스 킬 카운트 누적 데이터 필요
        return percent;
    }
    private float GetSkill(int id)
    {
        float percent = 0f;
        int size = CurrentUserData.AcquiredSkillIds.Count;
        percent = size / AchievementsData.Condition;
        return percent;
    }
    private float GetSoul(int id)
    {
        float percent = 0f;
        percent = soulGetCount / AchievementsData.Condition;//누적 소울 저장하는 기능 필요
        return percent;
    }
    private float NoHit(int id)
    {
        float percent = 0f;
        //이 부분은 확실히 튜터님께 가봐야 할듯
        return percent;
    }
    private float Death(int id)
    {
        float percent = 0f;
        //플레이어 데스 카운트 받아오기
        percent = deathCount / AchievementsData.Condition;
        return percent;
    }
    private float CleardTutorial(int id)
    {
        float percent = 0f;
        if (CurrentUserData.IsTutorialCleared == true)
        {
            percent = 1f;
        }
        else { percent = 0f; }
        return percent;
    }
    private float AchieveAll(int id)
    {
        int count = tableData.Count-1;//전체 도전과제 갯수
        //달성한 도전과제 갯수(아마 유저데이터에 추가할듯)을 받아와서 퍼센트 계산
        float percent = 0f;

        return percent;
    }


}