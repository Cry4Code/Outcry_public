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
        SpawnMonstersLogic();
        SettingBossHpBar();

        await UniTask.Delay(TimeSpan.FromSeconds(1f));

        await AudioManager.Instance.PlayBGM((int)SoundEnums.EBGM.AbandonedMine);
    }
}
