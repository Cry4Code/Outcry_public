using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VictoryUI : UIPopup
{
    [SerializeField] private Button ExitBtn;

    private void Awake()
    {
        ExitBtn.onClick.AddListener(OnExitButtonClicked);
    }

    private void OnExitButtonClicked()
    {
        Debug.Log($"[Button] 클리어 버튼 눌림");
        GameManager.Instance.GoToLobby();
    }
}
