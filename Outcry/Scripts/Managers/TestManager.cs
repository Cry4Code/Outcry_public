using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestManager : Singleton<TestManager>
{
    public bool triggerForFH = false;
    public bool triggerForVL = false;
    public bool triggerForVL2 = false;
    public bool triggerForBatStorm = false;


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

        if (triggerForVL2)
        {
            //생성 메소드
            InstantiateVampireLordFlying();
            triggerForVL2 = false;
            Debug.Log("TestManager: Trigger for VL2");
        }

        // 3보스 QTE 테스트 코드
        
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PlayerManager.Instance.player.runFSM = false;
            PlayerManager.Instance.player.ForceChangeAnimation(AnimatorHash.PlayerAnimation.StartQTE);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            PlayerManager.Instance.player.ForceChangeAnimation(AnimatorHash.PlayerAnimation.SuccessQTE);
        }
        
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            PlayerManager.Instance.player.ForceChangeAnimation(AnimatorHash.PlayerAnimation.EndQTE);
            PlayerManager.Instance.player.runFSM = true;
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
    private async void InstantiateVampireLordFlying()
    {
        if(vampireLordPrefab == null)
        {
            vampireLordPrefab =
                await ResourceManager.Instance.LoadAssetAddressableAsync<GameObject>("Monsters/VampireLordFlying.prefab");
        }
        
        GameObject vlInstance = Instantiate(vampireLordPrefab, Vector3.zero, Quaternion.identity);
        
        // 몬스터 데이터 설정
        if (!DataManager.Instance.MonsterDataList.TryGetMonsterModelData(101206, out MonsterModelBase monsterData))
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
