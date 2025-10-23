using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmUI : UIPopup
{
    [SerializeField] private TextMeshProUGUI titleTxt;
    [SerializeField] private TextMeshProUGUI messageTxt;
    [SerializeField] private Button okBtn;
    [SerializeField] private TextMeshProUGUI okBtnTxt;
    [SerializeField] private Button cancelBtn;
    [SerializeField] private TextMeshProUGUI cancelBtnTxt;
    [SerializeField] private Image itemSlot;
    [SerializeField] private Image itemSprite;

    // 버튼 클릭 시 실행될 Action을 저장할 변수
    private Action onClickOkAction;
    private Action onClickCancelAction;

    private void Awake()
    {
        okBtn.onClick.AddListener(OnClickOkBtn);
        cancelBtn.onClick.AddListener(OnClickCancelBtn);
    }

    /// <summary>
    /// ConfirmPopupData를 받아 UI를 설정하는 핵심 메서드
    /// </summary>
    public void Setup(ConfirmPopupData data)
    {
        // 텍스트 설정
        titleTxt.text = data.Title;
        messageTxt.text = data.Message;

        // 버튼 텍스트 설정
        okBtnTxt.text = data.OkButtonText == null ? "OK" : data.OkButtonText;
        cancelBtnTxt.text = "Cancel";

        // 버튼 클릭 액션 저장
        onClickOkAction = data.OnClickOK;
        onClickCancelAction = data.OnClickCancel;

        // ItemSprite가 있는지 확인하고 UI에 적용
        if (data.ItemSprite != null)
        {
            itemSprite.gameObject.SetActive(true);
            itemSprite.sprite = data.ItemSprite;
        }
        else
        {
            itemSprite.gameObject.SetActive(false);
        }

        // 버튼 활성화 상태 설정
        switch (data.Type)
        {
            case EConfirmPopupType.OK:
                okBtn.gameObject.SetActive(true);
                cancelBtn.gameObject.SetActive(false);
                break;
            case EConfirmPopupType.OK_CANCEL:
                okBtn.gameObject.SetActive(true);
                cancelBtn.gameObject.SetActive(true);
                break;
            case EConfirmPopupType.SOUL_ACQUIRE_OK:
                okBtn.gameObject.SetActive(true);
                cancelBtn.gameObject.SetActive(false);
                itemSlot.gameObject.SetActive(true);
                break;
            case EConfirmPopupType.SKILL_ACQUIRE_OK_CANCEL:
                okBtn.gameObject.SetActive(true);
                cancelBtn.gameObject.SetActive(true);
                itemSlot.gameObject.SetActive(true);
                break;
        }
    }

    private void OnClickOkBtn()
    {
        // 저장된 액션이 있다면 실행
        onClickOkAction?.Invoke();
        // 실행 후에는 참조를 초기화하여 중복 실행 방지
        onClickOkAction = null;

        // UI 닫기
        UIManager.Instance.Hide<ConfirmUI>();
    }

    private void OnClickCancelBtn()
    {
        // 저장된 액션이 있다면 실행
        onClickCancelAction?.Invoke();
        onClickCancelAction = null;

        // UI 닫기
        UIManager.Instance.Hide<ConfirmUI>();
    }
}
