using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TutorialStageController : StageController
{
    private int currentWaveIndex = 0;

    public override async UniTask StageSequence()
    {
        SpawnPlayer();

        PlayerManager.Instance.player.runFSM = false;

        await UniTask.Yield(PlayerLoopTiming.Update);
        await UniTask.Yield(PlayerLoopTiming.Update);
        InitializeInGameCursor();

        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

        PlayerManager.Instance.player.runFSM = true;

        // 몬스터 한 번에 스폰하지 않고 첫 번째 웨이브만 시작
        SpawnMonstersLogic();

        await AudioManager.Instance.PlayBGM((int)SoundEnums.EBGM.Tutorial);
    }

    protected override void SpawnMonstersLogic()
    {
        SpawnNextMonster(); // 첫 몬스터만 스폰
    }

    /// <summary>
    /// 순서에 맞는 다음 몬스터를 스폰하는 내부 메서드
    /// </summary>
    private void SpawnNextMonster()
    {
        // 스폰할 몬스터가 있고 스폰 위치가 있는지 확인
        if (currentWaveIndex < enemyPrefabs.Count && enemySpawnPoints.TryGetValue(currentWaveIndex, out Transform spawnTransform))
        {
            GameObject monsterPrefab = enemyPrefabs[currentWaveIndex];
            GameObject monsterInstance = Instantiate(monsterPrefab, spawnTransform.position, spawnTransform.rotation);

            // 몬스터 데이터 설정
            if (!DataManager.Instance.MonsterDataList.TryGetMonsterModelData(stageData.Monster_ids[currentWaveIndex], out MonsterModelBase monsterData))
            {
                Debug.LogError("Monster data not found!");
            }

            var monster = monsterInstance.GetComponent<MonsterBase>();
            monster.SetMonsterData(monsterData);

            aliveMonsters.Add(monsterInstance);
            Debug.Log($"{currentWaveIndex + 1}번째 몬스터 스폰 완료.");

            currentWaveIndex++; // 다음 웨이브를 위해 인덱스 증가
        }
    }

    public override void OnMonsterDied(GameObject monster)
    {
        base.OnMonsterDied(monster); // 부모의 리스트 제거 로직 실행
        Debug.Log($"{monster.name} 처치 완료!");

        // 다음 웨이브 몬스터가 남아있다면 스폰
        if (currentWaveIndex < enemyPrefabs.Count)
        {
            Debug.Log("다음 몬스터를 스폰합니다.");
            SpawnNextMonster();
        }
        else
        {
            Debug.Log("모든 튜토리얼 몬스터를 처치했습니다! 스테이지 클리어!");
        }
    }
}
