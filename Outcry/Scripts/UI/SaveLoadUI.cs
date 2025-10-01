using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveLoadUI : UIPopup
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button[] slotBtns;
    [SerializeField] private TMP_Text[] nicknameTexts;
    [SerializeField] private GameObject[] characterSlotObjects;
    [SerializeField] private Button exitBtn;

    private void Start()
    {
        for(int i = 0; i < slotBtns.Length; i++)
        {
            int slotIndex = i; // 클로저 문제 방지
            slotBtns[i].onClick.AddListener(() => SaveLoadManager.Instance.SelectSlot(slotIndex));
        }

        exitBtn.onClick.AddListener(OnClickExit);
    }

    private void OnEnable()
    {
        if(SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.OnSlotsDataUpdated += UpdateUI;
        }
    }

    private void OnDisable()
    {
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.OnSlotsDataUpdated -= UpdateUI;
        }
    }

    public override void Open()
    {
        base.Open();

        // TODO: UI가 처음 열릴 때는 간단한 로딩UI 설정?

        foreach (var button in slotBtns)
        {
            button.interactable = false;
        }
    }

    /// <summary>
    /// 매니저로부터 데이터를 받아 UI를 채우는 역할만 수행
    /// </summary>
    private void UpdateUI(ESlotUIType type, Dictionary<int, UserData> data)
    {
        titleText.text = type.ToString();

        // 버튼 활성화
        foreach (var button in slotBtns)
        {
            button.interactable = true;
        }

        for (int i = 0; i < slotBtns.Length; i++)
        {
            if (data.TryGetValue(i, out UserData slotData))
            {
                nicknameTexts[i].text = slotData.Nickname;
                characterSlotObjects[i].SetActive(true);
                nicknameTexts[i].gameObject.SetActive(true);
            }
            else
            {
                nicknameTexts[i].gameObject.SetActive(false);
                characterSlotObjects[i].SetActive(false);
            }
        }
    }

    private void OnClickExit()
    {
        UIManager.Instance.Hide<SaveLoadUI>();
    }
}
