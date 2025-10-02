using Cysharp.Threading.Tasks;
using StageEnums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class FallenKingStageController : StageController
{
    private string rockObstacleAddress = "Stages/RuinsOfTheFallenKing/Ruins_Ground_Object.prefab";

    private GameObject loadedRockObstaclePrefab; // 로드된 프리팹을 저장할 변수
    private GoblinKingAI goblinKingInstance; // 이벤트 구독 해제를 위한 인스턴스 저장

    public override async UniTask StageSequence()
    {
        // 스테이지에 필요한 에셋 미리 로드
        await LoadAssetsCoroutine();

        SpawnPlayer();
        SpawnMonstersLogic();

        await UniTask.Delay(TimeSpan.FromSeconds(2f));

        await AudioManager.Instance.PlayBGM((int)SoundEnums.EBGM.RuinsOfTheFallenKing);
    }

    /// <summary>
    /// 이 스테이지에서 사용할 어드레서블 에셋을 로드하는 코루틴
    /// </summary>
    private IEnumerator LoadAssetsCoroutine()
    {
        Task<GameObject> loadTask = ResourceManager.Instance.LoadAssetAddressableAsync<GameObject>(rockObstacleAddress);
        yield return new WaitUntil(() => loadTask.IsCompleted);

        if (loadTask.Status == TaskStatus.RanToCompletion)
        {
            loadedRockObstaclePrefab = loadTask.Result;
            Debug.Log($"'{rockObstacleAddress}' 에셋 로딩 성공!");
        }
        else
        {
            Debug.LogError($"'{rockObstacleAddress}' 에셋 로딩 실패!");
        }
    }

    /// <summary>
    /// 부모의 기본 스폰 로직을 사용하고 스폰 후에 이벤트를 구독하는 추가 로직 실행
    /// </summary>
    protected override void SpawnMonstersLogic()
    {
        base.SpawnMonstersLogic();

        // 스폰된 몬스터 리스트에서 고블린 킹을 찾아 이벤트 구독
        foreach (GameObject monsterObject in aliveMonsters)
        {
            if (monsterObject.TryGetComponent<GoblinKingAI>(out goblinKingInstance))
            {
                goblinKingInstance.OnFallingRocksPattern += SpawnRocks;
                Debug.Log("[StageController] 스폰된 리스트에서 고블린 킹을 찾아 이벤트를 구독합니다.");
                break;
            }
        }

        if (goblinKingInstance == null)
        {
            Debug.LogError("[StageController] 스폰된 몬스터 중 고블린 킹을 찾을 수 없습니다!");
        }
    }

    /// <summary>
    /// 고블린 킹의 이벤트 호출에 의해 실행될 장애물 생성 메서드
    /// </summary>
    private void SpawnRocks()
    {
        if (loadedRockObstaclePrefab == null || obstacleSpawnPoints.Count == 0)
        {
            Debug.LogWarning("장애물 프리팹이 로드되지 않았거나 스폰 위치가 없습니다.");
            return;
        }

        Debug.Log("[StageController] 고블린 킹의 신호를 받아 장애물을 생성합니다!");
        int rocksToSpawn = 2;
        if (obstacleSpawnPoints.Count < rocksToSpawn)
        {
            rocksToSpawn = obstacleSpawnPoints.Count;
        }

        // 스폰 위치 리스트를 복사하고 랜덤하게 섞음
        // Fisher-Yates 알고리즘을 사용한 셔플링
        List<Transform> shuffledPoints = obstacleSpawnPoints.ToList();
        for (int i = 0; i < shuffledPoints.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, shuffledPoints.Count);

            Transform temp = shuffledPoints[i];
            shuffledPoints[i] = shuffledPoints[randomIndex];
            shuffledPoints[randomIndex] = temp;
        }

        for (int i = 0; i < rocksToSpawn; i++)
        {
            Transform spawnPoint = shuffledPoints[i];
            GameObject rock = Instantiate(loadedRockObstaclePrefab, spawnPoint.position, spawnPoint.rotation);
            StartCoroutine(RockLifecycle(rock, 6f)); // 6초 후에 장애물 제거
        }
    }

    private IEnumerator RockLifecycle(GameObject rock, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (rock != null)
        {
            Destroy(rock);
        }
    }

    /// <summary>
    /// 컨트롤러가 파괴될 때 이벤트 구독 해제하여 메모리 누수 방지
    /// </summary>
    private void OnDestroy()
    {
        if (goblinKingInstance != null)
        {
            goblinKingInstance.OnFallingRocksPattern -= SpawnRocks;
        }

        // 이 컨트롤러에서만 사용한 에셋이 있다면 직접 해제해주는 것이 안전
        if (!string.IsNullOrEmpty(rockObstacleAddress))
        {
            ResourceManager.Instance.UnloadAddressableAsset(rockObstacleAddress);
        }
    }
}
