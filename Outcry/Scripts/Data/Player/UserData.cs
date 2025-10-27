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
    }
}