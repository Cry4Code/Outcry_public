using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InGameScene : SceneBase
{
    public override void SceneAwake() { }

    public override void SceneEnter()
    {
        var stageDataProvider = GameManager.Instance as IStageDataProvider;
        if (stageDataProvider == null)
        {
            return;
        }

        var stageData = stageDataProvider.GetStageData();
        if (stageData.Stage_id == (int)StageEnums.EStageType.Village)
        {
            GameManager.Instance.CurrentGameState = EGameState.Lobby;
        }
        else
        {
            GameManager.Instance.CurrentGameState = EGameState.Battle;
        }

        var allEnemyData = stageDataProvider.GetCurrentStageEnemyData(); // 모든 몬스터 데이터 가져오기
        var orderedMonsterPrefabs = new List<GameObject>();
        if (allEnemyData != null)
        {
            foreach (var enemyData in allEnemyData)
            {
                var prefab = ResourceManager.Instance.GetLoadedAsset<GameObject>(enemyData.Enemy_path);
                orderedMonsterPrefabs.Add(prefab); // 리스트에 순서대로 추가

                if (prefab == null)
                {
                    Debug.LogError($"ID {enemyData.ID}에 해당하는 몬스터 프리팹을 찾을 수 없습니다.");
                }
            }
        }

        var map = ResourceManager.Instance.GetLoadedAsset<GameObject>(stageData.Map_path);
        var player = ResourceManager.Instance.GetLoadedAsset<GameObject>(Paths.Prefabs.Player);

        // StageManager에 모든 정보를 전달
        StageManager.Instance.InitializeStage(stageData, map, player, orderedMonsterPrefabs);
    }

    public override void SceneExit()
    {
        UIManager.Instance.ClearUIPool();
        _ = AudioManager.Instance.StopBGM();
    }
}
