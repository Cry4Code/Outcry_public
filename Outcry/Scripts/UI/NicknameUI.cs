using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NicknameUI : UIPopup
{
    [SerializeField] private TMP_InputField nicknameInputField;
    [SerializeField] private Button startBtn;

    private void Awake()
    {
        startBtn.onClick.AddListener(OnStartButtonClicked);
    }

    private void OnStartButtonClicked()
    {
        GameManager.Instance.CreateNewGame(nicknameInputField.text);
    }
}
