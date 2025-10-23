using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AchievementManager : Singleton<AchievementManager>
{
    private Dictionary<int, SkillData> skillDict = new Dictionary<int, SkillData>();

    public UserData CurrentUserData { get; private set; }
    public AchievementsData AchievementsData { get; private set; }

    private int BossKillCount = 0;
    private int deathCount = 0;
    private int SkillGetCount = 0;
    private int SoulGetCount = 0;
    private bool CleardTuto = false;
    private int ClearAchieve = 0;


    private float BossKillAchieve(int id)
    {
        float percent = 0f;
        percent = BossKillCount / AchievementsData.Condition; //보스 킬 카운트 누적 데이터 필요
        return percent;
    }
    private float DoRangeB(int id)
    {
        float percent = 0f;
        int size = CurrentUserData.AcquiredSkillIds.Count;
        percent = size / AchievementsData.Condition;
        return percent;
    }
    private float DoRangeC(int id)
    {
        float percent = 0f;
        percent = SoulGetCount / AchievementsData.Condition;//누적 소울 저장하는 기능 필요
        return percent;
    }
    private float DoRangeD(int id)
    {
        float percent = 0f;

        return percent;
    }
    private float Do107012(int id)
    {
        float percent = 0f;

        return percent;
    }
    private float Do107013(int id)
    {
        float percent = 0f;
        if (CurrentUserData.IsTutorialCleared == true)
        {
            percent = 1f;
        }
        return percent;
    }
    private float Do107014(int id)
    {
        float percent = 0f;

        return percent;
    }
}
