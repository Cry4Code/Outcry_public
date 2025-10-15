using Cinemachine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyVillageController : StageController
{
    public override async UniTask StageSequence()
    {
        await AudioManager.Instance.PlayBGM((int)SoundEnums.EBGM.Lobby);

        // 플레이어 스폰
        SpawnPlayer();

        await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
    }
}
