using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbandonedMineStageController : StageController
{
    public override async UniTask StageSequence()
    {
        await AudioManager.Instance.PlayBGM((int)SoundEnums.EBGM.RuinsOfTheFallenKing);

        // TODO: 배경음악 바꾸기, 보스 스폰, 스테이지 기믹 있으면 추가
        // 플레이어 스폰
        SpawnPlayer();

        await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
    }
}
