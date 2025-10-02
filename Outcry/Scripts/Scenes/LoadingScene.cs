using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScene : SceneBase
{
    public override void SceneAwake()
    {
        _ = AudioManager.Instance.StopBGM();
    }

    public override void SceneEnter()
    {
        LoadingManager.Instance.StartLoadingProcess();
    }

    public override void SceneExit()
    {
        UIManager.Instance.ClearUIPool();
    }
}
