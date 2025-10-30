using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum EOptionUIType
{
    Title,
    Lobby,
    Stage,
}

public class OptionUI : UIPopup
{
    [SerializeField] private Button volumeBtn;
    [SerializeField] private Button emailLinkBtn;
    [SerializeField] private Button exitBtn;
    [SerializeField] private TextMeshProUGUI exitText;
    [SerializeField] private Button stageOptionExitBtn;
    [SerializeField] private Button englishBtn;
    [SerializeField] private Button koreanBtn;

    // 외부에서 전달받은 나가기 동작을 저장할 변수
    private Action onClickExitAction;
    private Action onClickStageOptionExit;

    private void Awake()
    {
        volumeBtn.onClick.AddListener(OnClickVolume);
        emailLinkBtn.onClick.AddListener(OnClickEmailLink);
        exitBtn.onClick.AddListener(OnClickExit);
        stageOptionExitBtn.onClick.AddListener(OnClickStageOptionExit);
        englishBtn.onClick.AddListener(OnClickEnglishButton);
        koreanBtn.onClick.AddListener(OnClickKoreanButton);
    }

    private void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        emailLinkBtn.gameObject.SetActive(false);
#endif
    }

    private void OnEnable()
    {
        if (UGSManager.Instance != null)
        {
            UGSManager.Instance.OnLinkSuccess += HandleLinkSuccess;
            UGSManager.Instance.OnLoginFailure += HandleLinkFailure;
        }
    }

    private void OnDisable()
    {
        if (UGSManager.Instance != null)
        {
            UGSManager.Instance.OnLinkSuccess -= HandleLinkSuccess;
            UGSManager.Instance.OnLoginFailure -= HandleLinkFailure;
        }
    }

    public void Setup(OptionUIData data)
    {
        exitText.text = data.ExitText;

        // 전달받은 동작을 내부 변수에 저장
        onClickExitAction = data.OnClickExitAction;
        onClickStageOptionExit = data.OnClickStageOptionExitAction;

        // 타입에 따라 버튼 활성화/비활성화 로직은 그대로 사용
        switch (data.Type)
        {
            case EOptionUIType.Title:
                exitBtn.gameObject.SetActive(true);
                stageOptionExitBtn.gameObject.SetActive(false);
                break;

            case EOptionUIType.Lobby:
                exitBtn.gameObject.SetActive(true);
                stageOptionExitBtn.gameObject.SetActive(true);
                break;

            // 스테이지에서 열었다면 Exit 버튼을 숨김
            case EOptionUIType.Stage:
                exitBtn.gameObject.SetActive(true);
                stageOptionExitBtn.gameObject.SetActive(true);
                break;

            default:
                exitBtn.gameObject.SetActive(true);
                stageOptionExitBtn.gameObject.SetActive(false);
                break;
        }

        bool isEn = LocalizationUtility.IsCurrentLanguage("en");
        englishBtn.gameObject.SetActive(!isEn);
        koreanBtn.gameObject.SetActive(isEn);
    }

    /// <summary>
    /// 계정 연동 성공 시 호출될 핸들러
    /// </summary>
    private void HandleLinkSuccess()
    {
        Debug.Log("Account linking successful!");

        // 성공 팝업 표시
        var popup = UIManager.Instance.Show<ConfirmUI>();
        popup.Setup(new ConfirmPopupData
        {
            // Title = "Account Link Successful",
            // Message = "Your account has been successfully linked.",
            Title = LocalizationUtility.GetLocalizedValueByKey(LocalizationStrings.LinkAccount.TITLE_SUCCESS),
            Message = LocalizationUtility.GetLocalizedValueByKey(LocalizationStrings.LinkAccount.MESSAGE_SUCCESS),
            Type = EConfirmPopupType.OK
        });
    }

    /// <summary>
    /// 계정 연동 실패 시 호출될 핸들러
    /// </summary>
    private void HandleLinkFailure(object sender, LoginErrorArgs args)
    {
        Debug.LogWarning($"Link failed: {args.Title} - {args.Message}");

        // 실패 팝업 표시
        var popup = UIManager.Instance.Show<ConfirmUI>();
        popup.Setup(new ConfirmPopupData
        {
            Title = args.Title,
            Message = args.Message,
            Type = EConfirmPopupType.OK
        });
    }

    private void OnClickVolume()
    {
        Debug.Log("Volume Clicked");
        EffectManager.Instance.ButtonSound();
        UIManager.Instance.Show<VolumeSettingsUI>();
    }

    private void OnClickEmailLink()
    {
        Debug.Log("Email Link Clicked");
        EffectManager.Instance.ButtonSound();

        _ = UGSManager.Instance.LinkWithUPAAsync();
    }

    private void OnClickExit()
    {
        Debug.Log("Exit Clicked");
        // 저장된 동작이 있다면 실행하고 없다면 기본 동작(숨기기) 수행
        if (onClickExitAction != null)
        {
            onClickExitAction.Invoke();
        }
        else
        {
            // 기본 동작 UI 숨기기
            UIManager.Instance.Hide<OptionUI>();
        }

        // 실행 후에는 참조 초기화
        onClickExitAction = null;
    }

    private void OnClickStageOptionExit()
    {
        Debug.Log("StageOptionExit Clicked");

        if (onClickStageOptionExit != null)
        {
            onClickStageOptionExit.Invoke();
        }
        else
        {
            // 기본 동작 UI 숨기기
            UIManager.Instance.Hide<OptionUI>();
        }

        // 실행 후에는 참조 초기화
        onClickStageOptionExit = null;
    }

    private void OnClickEnglishButton()
    {
        LocalizationUtility.SetLanguage("en");
        englishBtn.gameObject.SetActive(false);
        koreanBtn.gameObject.SetActive(true);
    }
    private void OnClickKoreanButton()
    {
        LocalizationUtility.SetLanguage("ko");
        englishBtn.gameObject.SetActive(true);
        koreanBtn.gameObject.SetActive(false);
    }
}
