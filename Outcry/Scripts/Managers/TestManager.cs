using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestManager : Singleton<TestManager>
{
    public bool triggerForFH = false;
    public bool triggerForVL = false;

    private GameObject vampireLordPrefab;

    protected override void Awake()
    {
        base.Awake();
        if (PlayerManager.Instance.player == null)
        {
            var playerInstance = FindAnyObjectByType<PlayerController>();
            if (playerInstance != null)
                PlayerManager.Instance.RegisterPlayer(playerInstance);
        }

        DataManager.Instance.ToString();
    }

    protected void Update()
    {
        if (triggerForVL)
        {
            //생성 메소드
            InstantiateVampireLord();
            triggerForVL = false;
            Debug.Log("TestManager: Trigger for VL");
        }
    }

    private async void InstantiateVampireLord()
    {
        if(vampireLordPrefab == null)
        {
            vampireLordPrefab =
                await ResourceManager.Instance.LoadAssetAddressableAsync<GameObject>("Monsters/VampireLord.prefab");
        }
        
        GameObject vlInstance = Instantiate(vampireLordPrefab, Vector3.zero, Quaternion.identity);
        
        // 몬스터 데이터 설정
        if (!DataManager.Instance.MonsterDataList.TryGetMonsterModelData(101205, out MonsterModelBase monsterData))
        {
            Debug.LogError("Monster data not found!");
        }
        
        var monster = vlInstance.GetComponent<MonsterBase>();
        if(monster == null)
        {
            Debug.LogError("MonsterBase 컴포넌트가 없습니다!");
        }
        monster.SetMonsterData(monsterData);    
    }
}
