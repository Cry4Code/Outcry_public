using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Temp_CommonMonsterSpawnManager : Singleton<Temp_CommonMonsterSpawnManager>
{
    [SerializeField] private PlayerController player;
    [SerializeField] private GameObject monsterPrefeb;
                
    private MonsterBase monster;

    public int testMonsterId;

    protected override void Awake()
    {
        base.Awake();

        if (!DataManager.Instance.MonsterDataList.TryGetMonsterModelData(testMonsterId, out MonsterModelBase monsterData))
        {
            Debug.LogError("TestManager: Monster data not found!");
            return;
        }

        GameObject monsterObj = GameObject.Instantiate(monsterPrefeb);
        monster = monsterObj.GetComponent<MonsterBase>();
        monster.SetMonsterData(monsterData);
    }
}
