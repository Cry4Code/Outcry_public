using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DefeatUI : UIBase
{
    [SerializeField] private Button ExitBtn;

    private void Awake()
    {
        ExitBtn.onClick.AddListener(OnExitButtonClicked);
    }

    private void OnExitButtonClicked()
    {
        GameManager.Instance.GoToLobby();
    }
}
