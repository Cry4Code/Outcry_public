using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class TestManager : Singleton<TestManager>
{
    [SerializeField] private PlayerController player;

    // [SerializeField] private GameObject monsterPrefab;
    //
    // private MonsterModelBase monsterData;
    //
    // //todo. monsterid 받아서 instantiate하도록 변경하기.
    // public bool isSkillTestMode = false;
    // public int skillTestMonsterId;
    // public BossMonsterModel SkillTestMonsterData;
    //
    // //RumbleOfRuins 몬스터용
    // public float cooltimeForROR = 180f;
    //
    // private MonsterBase monster;
    //
    // public int EffectIdForTest;
    // public EffectOrder EffectOrderForTest;
    // public Canvas canvas;
    
    void Awake()
    {
        // DataManager.Instance.ToString();
        // CameraManager.Instance.ToString();
        // EffectManager.Instance.ToString();
        // DataTableManager.Instance.LoadCollectionData<SoundDataTable>();
        // if(!isSkillTestMode)
        // {
        //     if (!DataManager.Instance.MonsterDataList.TryGetMonsterModelData(skillTestMonsterId, out MonsterModelBase monsterData))
        //     {
        //         Debug.LogError("TestManager: Monster data not found!");
        //         return;
        //     }
        //
        //     GameObject monsterObj = GameObject.Instantiate(monsterPrefab);
        //     monster = monsterObj.GetComponent<MonsterBase>();
        //     monster.SetMonsterData(monsterData);
        // }
        // else
        // {
        //     //스킬 체크용 몬스터
        //     GameObject monsterObj2 = GameObject.Instantiate(monsterPrefab);
        //     monster = monsterObj2.GetComponent<MonsterBase>();
        //     monster.SetMonsterData(SkillTestMonsterData);
        // }
    }

    private async void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            EffectManager.Instance.PlayEffectsByIdAsync(8888, EffectOrder.Monster,player.gameObject);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            
            EffectManager.Instance.PlayEffectsByIdAsync(8888, EffectOrder.Monster,player.gameObject, Vector3.right*3);
        }

        // if (Input.GetKeyDown(KeyCode.LeftArrow))
        // {
        //     CameraManager.Instance.ShakeCamera(0.1f, 1f, 1f, EffectOrder.Player);
        // }
        //
        // if (Input.GetKeyDown(KeyCode.RightArrow))
        // {
        //     CameraManager.Instance.ShakeCamera(10f, 10f, 10f, EffectOrder.SpecialEffect);
        // }
    }
    
}
