using System;
using System.Collections.Generic;
using System.Linq;

public class UserData
{
    public string Nickname;
    public List<int> ClearedBossIds;
    public bool IsTutorialCleared;

    // 기본 생성자
    public UserData(string nickname)
    {
        Nickname = nickname;
        ClearedBossIds = new List<int>();
        IsTutorialCleared = false;
    }

    // Firestore에서 받아온 Dictionary로부터 UserData 객체를 생성하는 생성자
    public UserData(Dictionary<string, object> data)
    {
        Nickname = data["Nickname"].ToString();
        IsTutorialCleared = (bool)data["IsTutorialCleared"];

        // Firestore는 정수를 long으로 반환하므로 long 리스트를 int 리스트로 변환
        ClearedBossIds = ((List<object>)data["ClearedBossIds"])
                         .Select(id => Convert.ToInt32(id))
                         .ToList();
    }

    // UserData 객체를 Firestore에 저장할 Dictionary 형태로 변환하는 메서드
    public Dictionary<string, object> ToDictionary()
    {
        return new Dictionary<string, object>
        {
            { "Nickname", Nickname },
            { "IsTutorialCleared", IsTutorialCleared },
            { "ClearedBossIds", ClearedBossIds }
        };
    }
}