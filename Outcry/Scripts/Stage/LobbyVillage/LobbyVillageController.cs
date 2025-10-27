using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class LobbyVillageController : StageController
{
    public override async UniTask StageSequence()
    {
        // 플레이어 스폰
        SpawnPlayer();

        await UniTask.Delay(TimeSpan.FromSeconds(0.1f));

        await AudioManager.Instance.PlayBGM((int)SoundEnums.EBGM.Lobby);
    }

    protected override void SpawnPlayer()
    {
        if (GameManager.Instance.HasEnteredDungeon)
        {
            // 던전에 다녀온 경우 1번 스폰 포인트(던전 앞)에서 스폰
            SpawnPlayerAt(playerSpawnPoints[1]);
        }
        else
        {
            // 처음 게임을 시작한 경우 0번 스폰 포인트(마을 중앙)에서 스폰
            SpawnPlayerAt(playerSpawnPoints[0]);
        }
    }
}
