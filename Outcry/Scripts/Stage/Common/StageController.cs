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
    //protected Transform playerSpawnPoint;
    protected Dictionary<int, Transform> playerSpawnPoints;
    protected Dictionary<int, Transform> enemySpawnPoints;
    protected List<Transform> obstacleSpawnPoints;
    protected CinemachineVirtualCamera stageCamera; // 스테이지 카메라 참조 추가

    // 생성된 인스턴스 관리
    protected GameObject playerInstance;
    public List<GameObject> aliveMonsters = new List<GameObject>();

    public virtual void Initialize(StageManager manager, StageData data, GameObject player, List<GameObject> enemys, Dictionary<int, Transform> playerSpawns, Dictionary<int, Transform> enemySpawns, CinemachineVirtualCamera vcam, List<Transform> obstacleSpawns)
    {
        stageManager = manager;
        stageData = data;
        playerPrefab = player;
        enemyPrefabs = enemys;
        playerSpawnPoints = playerSpawns;
        enemySpawnPoints = enemySpawns;
        stageCamera = vcam;
        obstacleSpawnPoints = obstacleSpawns;

        if (enemys != null && enemys.Count > 0)
        {
            stageManager.SetTotalMonsterCount(enemys.Count);
        }
        else // 몬스터가 없거나 리스트가 null일 경우
        {
            stageManager.SetTotalMonsterCount(0); // 몬스터 수를 0으로 명확하게 설정
        }
    }

    /// <summary>
    /// 각 스테이지의 고유한 시나리오가 구현될 메인 코루틴(자식 클래스에서 반드시 구현)
    /// </summary>
    public virtual async UniTask StageSequence()
    {
        // 플레이어 스폰
        SpawnPlayer();

        // 몬스터 스폰 로직 호출
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));  // 약간의 딜레이 후 스폰
        SpawnMonstersLogic();
    }

    
    /// <summary>
    ///  보스가 생성될 때 한 번 불러주면 됨
    /// </summary>
    protected void SettingBossHpBar()
    {
        
        if (stageData.Stage_id != StageID.Tutorial && stageData.Stage_id != StageID.Village)
        {
            
            HUDUI hudUI = UIManager.Instance.GetUI<HUDUI>();

            int monsterId = aliveMonsters[0].GetComponent<MonsterBase>().MonsterData.monsterId;

            // 체력바 설정
            if (DataManager.Instance.MonsterDataList.TryGetMonsterModelData(monsterId,
                    out var monsterModel))
            {
                hudUI.SettingBossHp(monsterModel.health);
                hudUI.SetBossProtrait(stageData.Monster_ids[0]);
                EventBus.Unsubscribe(EventBusKey.ChangeBossHealth, UIManager.Instance.GetUI<HUDUI>().ChangeBossHpBar);
                EventBus.Subscribe(EventBusKey.ChangeBossHealth, UIManager.Instance.GetUI<HUDUI>().ChangeBossHpBar);
            }

            // 타이머 설정
            hudUI.SettingMaxTimer(stageData.Time_limit);
        }
    }

    /// <summary>
    /// 보스 체력을 임의로 조절
    /// </summary>
    /// <param name="newHp">새로 지정할 체력</param>
    public void ChangeBossHp(MonsterBase mon, int newHp)
    {
        if (stageData.Stage_id != StageID.Tutorial && stageData.Stage_id != StageID.Village)
        {
            mon.Condition.CurrentHealth.SetCurValue(newHp);
            UIManager.Instance.GetUI<HUDUI>().ChangeBossHpBar(newHp);
        }
    }

    protected virtual void SpawnPlayer()
    {
        SpawnPlayerAt(playerSpawnPoints[0]);
    }

    protected virtual GameObject SpawnPlayerAt(Transform spawnPoint, float heightOffset = 0f)
    {
        if (playerPrefab != null && spawnPoint != null)
        {
            Vector3 spawnPosition = spawnPoint.position;
            spawnPosition.y += heightOffset;

            playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            PlayerController playerController = playerInstance.GetComponent<PlayerController>();
            if (playerController == null)
            {
                Debug.LogError("스폰된 플레이어 프리팹에 PlayerController가 없습니다!");
                return null;
            }

            Debug.Log($"플레이어 스폰 완료: {spawnPoint.name}");

            // PlayerManager에 등록
            PlayerManager.Instance.RegisterPlayer(playerController);

            // 카메라 Follow 대상 설정
            if (stageCamera != null)
            {
                stageCamera.Follow = playerInstance.transform;
            }
            // CursorManager 초기화
            if (CursorManager.Instance != null)
            {
                CursorManager.Instance.InitializeForInGame(playerController);
            }

            // 상태 UI 표시
            UIManager.Instance.Show<HUDUI>();

            // 보유한 스킬 장착
            if (GameManager.Instance.CurrentUserData != null)
            {
                PlayerManager.Instance.player.Skill.SetSkill(GameManager.Instance.CurrentUserData.SelectSkillId);
            }

            return playerInstance;
        }
        return null;
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
        // 몬스터가 없는 상황 방어(로비 빌리지에서는 몬스터가 없을 수 있음)
        if (enemyPrefabs == null || enemySpawnPoints == null)
        {
            return;
        }

        // 리스트가 비어있는지 확인(몬스터가 없는 스테이지는 정상적인 상황)
        if (enemyPrefabs.Count == 0)
        {
            return;
        }

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
                if(monster == null)
                {
                    Debug.LogError("MonsterBase 컴포넌트가 없습니다!");
                    continue;
                }

                monster.SetMonsterData(monsterData);

                Debug.Log($"SpawnIndex {i} 위치에 몬스터(ID: {stageData.Monster_ids[i]}) 스폰 완료");

                aliveMonsters.Add(monsterInstance);
            }
        }
    }

    /// <summary>
    /// 지정된 위치에 단일 몬스터를 스폰하는 범용 메서드
    /// </summary>
    protected virtual GameObject SpawnMonsterAt(GameObject monsterPrefab, int monsterId, Transform spawnPoint)
    {
        if (monsterPrefab == null || spawnPoint == null)
        {
            Debug.LogError("몬스터 프리팹 또는 스폰 위치가 null입니다!");
            return null;
        }

        GameObject monsterInstance = Instantiate(monsterPrefab, spawnPoint.position, spawnPoint.rotation);

        if (!DataManager.Instance.MonsterDataList.TryGetMonsterModelData(monsterId, out MonsterModelBase monsterData))
        {
            Debug.LogError($"ID {monsterId}에 해당하는 몬스터 데이터를 찾을 수 없습니다!");
            Destroy(monsterInstance);
            return null;
        }

        var monster = monsterInstance.GetComponent<MonsterBase>();
        if (monster == null)
        {
            Debug.LogError("MonsterBase 컴포넌트가 없습니다!");
            Destroy(monsterInstance);
            return null;
        }

        monster.SetMonsterData(monsterData);
        Debug.Log($"SpawnPoint '{spawnPoint.name}' 위치에 몬스터(ID: {monsterId}) 스폰 완료");
        aliveMonsters.Add(monsterInstance);

        return monsterInstance;
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

    /// <summary>
    /// 스테이지 승리 시 StageManager에 의해 호출됩니다.
    /// </summary>
    public virtual void OnStageVictory()
    {
        Debug.Log("[StageController] 승리 처리 시작: 플레이어 입력 비활성화 및 몬스터 AI 정지");

        // 플레이어 입력 비활성화
        if (playerInstance != null && PlayerManager.Instance.player != null)
        {
            PlayerManager.Instance.player.runFSM = false; // FSM 멈추기
        }

        // 현재 살아있는 모든 몬스터 AI 중지
        foreach (var monsterObject in aliveMonsters)
        {
            if (monsterObject != null && monsterObject.TryGetComponent<MonsterAIBase>(out var monsterAI))
            {
                monsterAI.DeactivateBt();
            }
        }
    }

    /// <summary>
    /// 스테이지 패배 시 StageManager에 의해 호출
    /// </summary>
    public virtual void OnStageDefeat()
    {
        Debug.Log("[StageController] 패배 처리 시작: 모든 몬스터 AI 정지");

        // 현재 살아있는 모든 몬스터의 AI 중지
        foreach (var monsterObject in aliveMonsters)
        {
            if (monsterObject != null && monsterObject.TryGetComponent<MonsterAIBase>(out var monsterAI))
            {
                monsterAI.DeactivateBt();
            }
        }
    }
}
