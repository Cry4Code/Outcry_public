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
    public bool IsTutorialCleared;
    public List<int> ClearedBossIds;
    public List<int> AcquiredSkillIds;
    public List<UserSoulData> AcquiredSouls;

    public UserData() { }

    public UserData(string nickname)
    {
        Nickname = nickname;
        IsTutorialCleared = false;
        ClearedBossIds = new List<int>();
        AcquiredSkillIds = new List<int>();
        AcquiredSouls = new List<UserSoulData>();
    }
}