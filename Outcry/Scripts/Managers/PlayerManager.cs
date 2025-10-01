using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    public PlayerController player { get; private set; }

    /// <summary>
    /// PlayerController가 생성될 때 스스로를 등록하기 위해 호출하는 메서드
    /// </summary>
    public void RegisterPlayer(PlayerController playerController)
    {
        this.player = playerController;
        Debug.Log($"[PlayerManager] '{playerController.name}'가 등록되었습니다.");
    }
}
