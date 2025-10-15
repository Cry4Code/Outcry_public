using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NicknameUI : UIPopup
{
    [SerializeField] private TMP_InputField nicknameInputField;
    [SerializeField] private Button startBtn;
    [SerializeField] private Button exitBtn;

    private void Awake()
    {
        startBtn.onClick.AddListener(OnStartButtonClicked);
        exitBtn.onClick.AddListener(OnExitButtonClicked);
    }

    private void OnStartButtonClicked()
    {
        GameManager.Instance.CreateNewGame(nicknameInputField.text);
    }

    private void OnExitButtonClicked()
    {
        UIManager.Instance.Hide<NicknameUI>();
    }
}
