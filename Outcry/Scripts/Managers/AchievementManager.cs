using System.Collections.Generic;
using UnityEngine;

public enum EMissionType
{
    BossKillAchieve = 1,
    GetSkill = 2,
    GetSoul=3,
    NoHit=4,
    Death=7,
    CleardTutorial = 8,
    AchieveAll = 9
}

public class AchievementManager : Singleton<AchievementManager>
{
    private Dictionary<int, IData> tableData;
    public UserData currentUserData => GameManager.Instance.CurrentUserData;

    public struct AchievementEntry // 외부에서 사용하기 쉽게 만든 구조체
    {
        public int id;                 // 딕셔너리 키(= 도전과제 ID)
        public AchievementsData data;  // 실제 데이터
    }

    protected override void Awake()
    {
        base.Awake();

        tableData = DataTableManager.Instance.CollectionData[typeof(AchievementsData)] as Dictionary<int, IData>;

        if (tableData == null)
            Debug.LogError("[AchievementManager] AchievementsData 바인딩 실패 (로드/키/캐스팅 확인)");
    }

    private AchievementsData GetData(int id)
    {
        return DataTableManager.Instance.GetCollectionDataById<AchievementsData>(id);
    }

    private int getType(int skillId)
    {
        var skillType = DataTableManager.Instance.GetCollectionDataById<AchievementsData>(skillId);

        if (skillType != null)
        {
            int count = skillType.MissionType; //타입으로 바꿔야 함!!!!!!!!!!!!!!!!!!!!!!!
            return count;
        }
        else
        {
            Debug.LogWarning("해당 id 에 해당하는 타입이 없습니다");
            return 0;
        }
    }

    public void ReportEvent(EMissionType type, int value = 1)
    {
        if (currentUserData == null)
        {
            return;
        }

        // MissionType에 따라 적절한 데이터 갱신
        switch (type)
        {
            case EMissionType.BossKillAchieve:
                currentUserData.TotalBossKills += value;
                break;
            case EMissionType.GetSkill:
                // GetSkill 메서드에서 유저 데이터 값을 바로 비교하고 있음
                break;
            case EMissionType.GetSoul:
                // GetSoul 메서드에서 유저 데이터 값을 바로 비교하고 있음
                break;
            case EMissionType.NoHit:
                // NoHit은 누적 방식이 아니므로 달성 가능한 모든 NoHit 업적 즉시 완료
                int clearedStageId = value; // 클리어한 스테이지 ID

                foreach (var entry in GetAllAchievementsSorted())
                {
                    // NoHit 타입의 업적이고
                    // 업적의 TargetID가 방금 클리어한 스테이지 ID와 일치하며
                    // 아직 달성하지 않은 업적인지 확인
                    if (entry.data.MissionType == (int)EMissionType.NoHit &&
                        entry.data.TargetID == clearedStageId &&
                        !currentUserData.CompletedAchievementIds.Contains(entry.id))
                    {
                        CompleteAchievement(entry.id);
                    }
                }
                // NoHit은 CheckAllAchievements를 탈 필요가 없으므로 return
                return;
            case EMissionType.Death:
                currentUserData.TotalDeaths += value;
                break;
            case EMissionType.CleardTutorial:
                // CleardTutorial메서드에서 유저 데이터 값을 바로 비교하고 있음
                break;
        }

        CheckAllAchievements();
    }

    private void CompleteAchievement(int id)
    {
        if (currentUserData.CompletedAchievementIds.Contains(id))
        {
            return;
        }

        currentUserData.CompletedAchievementIds.Add(id);
        UGSManager.Instance.LogAchievementClear(id);
        Debug.Log($"<color=yellow>업적 달성! ID: {id}</color>");

        // TODO: UI 팝업 등 달성 연출?

        // 모든 업적 달성 같은 특수 업적을 위해 다시 한번 체크
        CheckAllAchievements();
    }

    private void CheckAllAchievements()
    {
        var allAchievements = GetAllAchievementsSorted();
        foreach (var achievement in allAchievements)
        {
            // 이미 달성한 업적이면 건너뛰기
            if (currentUserData.CompletedAchievementIds.Contains(achievement.id))
            {
                continue;
            }

            // isCleared 함수를 호출하여 달성 여부 판단
            isCleared(achievement.id);
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
            case (int)EMissionType.BossKillAchieve:
                return BossKillAchieve(id) ;

            case (int)EMissionType.GetSkill: 
                return GetSkill(id);

            case (int)EMissionType.GetSoul: 
                return GetSoul(id);

            case (int)EMissionType.NoHit: 
                return NoHit(id);

            case (int)EMissionType.Death: 
                return Death(id);

            case (int)EMissionType.CleardTutorial: 
                return CleardTutorial(id);

            case (int)EMissionType.AchieveAll: 
                return AchieveAll(id);

            default:
                return 0;
        }
    }

    public void isCleared(int id)
    {
        if (ShowPersent(id) >= 1)
        {
            CompleteAchievement(id);
        }
    }

    private float BossKillAchieve(int id)
    {
        float percent = 0f;
        var data = GetData(id);
        if (data == null) return 0f;       // ★ NRE 방지 한 줄만

        percent = (float)currentUserData.TotalBossKills / data.Condition;
        return percent;
    }

    private float GetSkill(int id)
    {
        float percent = 0f;
        var data = GetData(id);
        if (data == null) return 0f;       // ★ NRE 방지

        int size = currentUserData.AcquiredSkillIds.Count;
        Mathf.Clamp(size,0,6);
        percent = (float)size / data.Condition;   // ← 기존 공식 그대로
        Debug.LogWarning(size);
        Debug.LogWarning(percent);

        return percent;
    }

    private float GetSoul(int id)
    {
        float percent = 0f;
        var data = GetData(id);
        if (data == null) return 0f;       // ★ NRE 방지

        // 1. 현재 보유한 모든 소울의 총 개수를 계산합니다.
        int totalSoulsOwned = 0;
        foreach (var soulData in currentUserData.AcquiredSouls)
        {
            totalSoulsOwned += soulData.Count;
        }

        percent = (float)totalSoulsOwned / data.Condition;
        return percent;
    }

    private float NoHit(int id)
    {
        if (currentUserData == null)
        {
            return 0f;
        }

        // 유저의 완료된 업적 ID 목록에 이 업적의 ID가 포함되어 있는지 확인
        // 포함되어 있다면 1.0f (100%) 그렇지 않다면 0.0f (0%) 반환
        return currentUserData.CompletedAchievementIds.Contains(id) ? 1f : 0f;
    }

    private float Death(int id)
    {
        float percent = 0f;
        var data = GetData(id);
        if (data == null) return 0f;       // ★ NRE 방지

        percent = (float)currentUserData.TotalDeaths / data.Condition;
        return percent;
    }

    private float CleardTutorial(int id)
    {
        float percent = 0f;
        if (currentUserData.IsTutorialCleared == true)
        {
            percent = 1f;
        }
        else { percent = 0f; }
        return percent;
    }

    private float AchieveAll(int id)
    {
        // 데이터가 없으면 진행률 0%
        if (tableData == null || currentUserData == null)
        {
            return 0f;
        }

        // '모든 업적 달성' 업적 자신(1개)을 제외한 전체 업적 수 계산
        int totalAchievementsToComplete = tableData.Count - 1;

        // 전체 업적이 1개 이하면(즉 '모든 업적' 업적만 있다면) 0으로 나누는 오류 방지
        if (totalAchievementsToComplete <= 0)
        {
            return 0f;
        }

        // 현재 유저가 달성한 업적의 수 가져옴
        int userCompletedCount = currentUserData.CompletedAchievementIds.Count;

        // 진행률(%) 계산(현재 달성 개수 / 목표 개수)
        float percent = (float)userCompletedCount / totalAchievementsToComplete;

        return percent;
    }
}
