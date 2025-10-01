using Cinemachine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Figures;

public class StageController : MonoBehaviour
{
    // StageManager로부터 전달받을 데이터
    protected StageManager stageManager;
    protected StageData stageData;
    protected GameObject playerPrefab;
    protected List<GameObject> enemyPrefabs;
    protected Transform playerSpawnPoint;
    protected Dictionary<int, Transform> enemySpawnPoints;
    protected List<Transform> obstacleSpawnPoints;
    protected CinemachineVirtualCamera stageCamera; // 스테이지 카메라 참조 추가

    // 생성된 인스턴스 관리
    protected GameObject playerInstance;
    public List<GameObject> aliveMonsters = new List<GameObject>();

    public virtual void Initialize(StageManager manager, StageData data, GameObject player, List<GameObject> enemys, Transform playerSpawn, Dictionary<int, Transform> enemySpawns, CinemachineVirtualCamera vcam, List<Transform> obstacleSpawns)
    {
        stageManager = manager;
        stageData = data;
        playerPrefab = player;
        enemyPrefabs = enemys;
        playerSpawnPoint = playerSpawn;
        enemySpawnPoints = enemySpawns;
        stageCamera = vcam;
        obstacleSpawnPoints = obstacleSpawns;

        stageManager.SetTotalMonsterCount(enemyPrefabs.Count);
    }

    /// <summary>
    /// 각 스테이지의 고유한 시나리오가 구현될 메인 코루틴(자식 클래스에서 반드시 구현)
    /// </summary>
    public virtual async UniTask StageSequence()
    {
        Debug.Log("기본 스테이지 시퀀스 시작!");
        // 플레이어 스폰
        SpawnPlayer();

        // 몬스터 스폰 로직 호출
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));  // 약간의 딜레이 후 스폰
        SpawnMonstersLogic();
    }

    protected virtual void SpawnPlayer()
    {
        if (playerPrefab != null && playerSpawnPoint != null)
        {
            playerInstance = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);
            PlayerController playerController = playerInstance.GetComponent<PlayerController>();
            if (playerController == null)
            {
                Debug.LogError("스폰된 플레이어 프리팹에 PlayerController가 없습니다!");
                return;
            }

            Debug.Log("플레이어 스폰 완료.");

            // PlayerManager에 등록
            PlayerManager.Instance.RegisterPlayer(playerController);

            // 카메라 Follow 대상 설정
            if (stageCamera != null)
            {
                stageCamera.Follow = playerInstance.transform;
                Debug.Log($"[StageController] Virtual Camera가 '{playerInstance.name}'를 따라가도록 설정했습니다.");
            }

            // CursorManager 초기화
            if (CursorManager.Instance != null)
            {
                CursorManager.Instance.InitializeForInGame(playerController);
            }

            // 상태 UI 표시
            UIManager.Instance.Show<HUDUI>();
        }
    }

    /// <summary>
    /// 실제 스폰 로직을 담당하는 가상 메서드.
    /// 기본적으로는 모든 몬스터를 스폰합니다. 자식 클래스에서 특별한 스폰 로직이 필요하면 이 메서드를 override
    /// </summary>
    protected virtual void SpawnMonstersLogic()
    {
        SpawnAllMonsters();
    }

    /// <summary>
    /// 모든 몬스터를 한 번에 스폰
    /// </summary>
    protected virtual void SpawnAllMonsters()
    {
        Debug.Log("모든 몬스터를 한 번에 스폰합니다.");
        for (int i = 0; i < enemyPrefabs.Count; i++)
        {
            if (enemySpawnPoints.TryGetValue(i, out Transform spawnTransform))
            {
                GameObject monsterInstance = Instantiate(enemyPrefabs[i], spawnTransform.position, spawnTransform.rotation);

                // 몬스터 데이터 설정
                if (!DataManager.Instance.MonsterDataList.TryGetMonsterModelData(stageData.Monster_ids[i], out MonsterModelBase monsterData))
                {
                    Debug.LogError("Monster data not found!");
                    break;
                }

                var monster = monsterInstance.GetComponent<MonsterBase>();
                monster.SetMonsterData(monsterData);

                Debug.Log($"SpawnIndex {i} 위치에 몬스터(ID: {stageData.Monster_ids[i]}) 스폰 완료");

                aliveMonsters.Add(monsterInstance);
            }
        }
    }

    /// <summary>
    /// StageManager로부터 몬스터 사망 신호를 받았을 때 호출될 메서드
    /// </summary>
    public void CheckForDeadMonsters()
    {
        // 리스트를 순회하며 아이템을 제거할 수 있으므로 역순으로 순회하는 것이 가장 안전
        for (int i = aliveMonsters.Count - 1; i >= 0; i--)
        {
            GameObject monster = aliveMonsters[i];

            // 리스트에 null 참조가 있는지 먼저 확인
            if (monster == null)
            {
                aliveMonsters.RemoveAt(i); // 리스트에서 null 항목 제거
                continue; // 다음 아이템으로
            }

            // MonsterCondition 컴포넌트가 있는지 확인
            MonsterCondition condition = monster.GetComponent<MonsterCondition>();
            if (condition == null)
            {
                // 몬스터가 아니거나 컴포넌트가 없는 잘못된 데이터일 수 있음
                Debug.LogWarning($"'{monster.name}'에 MonsterCondition 컴포넌트가 없어 리스트에서 제외합니다.", monster);
                aliveMonsters.RemoveAt(i);
                continue;
            }

            // 몬스터가 실제로 죽었는지 확인
            // condition.IsDead가 null일 가능성도 방어
            if (condition.IsDead != null && condition.IsDead.Value)
            {
                // 찾은 몬스터를 기반으로 사망 처리 로직을 실행
                OnMonsterDied(monster);
            }
        }
    }

    public virtual void OnMonsterDied(GameObject monster)
    {
        if (aliveMonsters.Contains(monster))
        {
            aliveMonsters.Remove(monster);
            Debug.Log($"몬스터 사망 처리: {monster.name} 제거. 남은 몬스터 수: {aliveMonsters.Count}");
        }
        else
        {
            Debug.LogWarning($"알 수 없는 몬스터 사망 이벤트: {monster.name}");
        }

        // 웨이브 스폰 등 몬스터 사망 시 필요한 로직을 자식 클래스에서 구현 가능
    }
}
