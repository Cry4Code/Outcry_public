using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyScene : SceneBase
{
    public override void SceneAwake()
    {
        UIManager.Instance.Show<LobbyUI>();
    }

    public override void SceneEnter()
    {
        _ = AudioManager.Instance.PlayBGM((int)SoundEnums.EBGM.Lobby);
    }

    public override void SceneExit()
    {
        UIManager.Instance.ClearUIPool();
        _ = AudioManager.Instance.StopBGM();
    }
}
