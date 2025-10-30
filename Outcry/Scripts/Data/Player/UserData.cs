using System;
using System.Collections.Generic;

[Serializable]
public struct UserSoulData
{
    public int SoulId;
    public int Count;
}

[Serializable]
public class UserData
{
    public string Nickname;
    public string UniquePlayerName; // UGS가 부여한 이름#태그
    public bool IsTutorialCleared;
    public List<int> ClearedBossIds;
    public int SelectSkillId;
    public List<int> AcquiredSkillIds;
    public List<UserSoulData> AcquiredSouls;

    // 업적 ID 저장할 리스트
    public List<int> CompletedAchievementIds;

    // 누적 통계 데이터
    public int TotalBossKills;
    public int TotalDeaths;

    public UserData() { }

    public UserData(string nickname)
    {
        Nickname = nickname;
        UniquePlayerName = UGSManager.Instance.PlayerDisplayName;
        IsTutorialCleared = false;
        ClearedBossIds = new List<int>();
        SelectSkillId = 0;
        AcquiredSkillIds = new List<int>();
        AcquiredSouls = new List<UserSoulData>();

        CompletedAchievementIds = new List<int>();
        TotalBossKills = 0;
        TotalDeaths = 0;
    }
}