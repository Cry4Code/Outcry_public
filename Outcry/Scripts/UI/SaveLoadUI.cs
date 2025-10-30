using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveLoadUI : UIPopup
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button[] slotBtns;
    [SerializeField] private Button[] deleteBtns;
    [SerializeField] private TMP_Text[] nicknameTexts;
    [SerializeField] private TMP_Text[] skillCountsTexts;
    [SerializeField] private GameObject[] emptySlotObjects;
    [SerializeField] private GameObject[] characterSlotObjects;
    [SerializeField] private Button exitBtn;

    // 외부에서 전달받은 나가기 동작을 저장할 변수
    private Action onClickExitAction;

    private void Start()
    {
        for(int i = 0; i < slotBtns.Length; i++)
        {
            int slotIndex = i; // 클로저 문제 방지
            slotBtns[i].onClick.AddListener(() =>
            {
                SaveLoadManager.Instance.SelectSlot(slotIndex);
            });

            // deleteBtns 배열이 null이 아니고 인덱스가 유효할 때만 리스너 추가
            if (deleteBtns != null && i < deleteBtns.Length && deleteBtns[i] != null)
            {
                deleteBtns[i].onClick.AddListener(() =>
                {
                    SaveLoadManager.Instance.DeleteSlotData(slotIndex);
                });
            }
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
        //titleText.text = type.ToString();
        // titleText.text = "Save / Load";
        titleText.text = LocalizationUtility.GetLocalizedValueByKey(LocalizationStrings.SaveLoad.SAVELOAD);

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
                skillCountsTexts[i].text = $"Skills: {slotData.AcquiredSkillIds.Count}";

                emptySlotObjects[i].SetActive(false);
                characterSlotObjects[i].SetActive(true);
                nicknameTexts[i].gameObject.SetActive(true);
            }
            else
            {
                emptySlotObjects[i].SetActive(true);
                characterSlotObjects[i].SetActive(false);
                nicknameTexts[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 외부에서 데이터를 받아 UI의 동작을 설정하는 메서드
    /// </summary>
    public void Setup(SaveLoadUIData data)
    {
        // 전달받은 데이터가 null이 아니면 나가기 동작 저장
        if (data != null)
        {
            this.onClickExitAction = data.OnClickExitAction;
        }
    }

    private void OnClickExit()
    {
        // 저장된 나가기 동작이 있으면 실행 없으면 기본 동작(숨기기) 수행
        if (onClickExitAction != null)
        {
            onClickExitAction.Invoke();
            // 동작 실행 후에는 초기화하여 다음 Open 시에 영향이 없도록 함
            onClickExitAction = null;
        }
        else
        {
            // 기본 동작 UI 숨기기
            UIManager.Instance.Hide<SaveLoadUI>();
        }
    }
}
