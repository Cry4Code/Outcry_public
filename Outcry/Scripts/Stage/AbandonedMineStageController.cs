using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbandonedMineStageController : StageController
{
    public override async UniTask StageSequence()
    {
        // TODO: 스테이지 기믹 있으면 추가
        // 플레이어 스폰
        SpawnPlayer();

        PlayerManager.Instance.player.runFSM = false;

        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

        InitializeInGameCursor();
        PlayerManager.Instance.player.runFSM = true;

        SpawnMonstersLogic();
        SettingBossHpBar();

        await UniTask.Delay(TimeSpan.FromSeconds(1f));

        await AudioManager.Instance.PlayBGM((int)SoundEnums.EBGM.AbandonedMine);
    }
}
