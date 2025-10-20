using System;
using UnityEngine;
using UnityEngine.UI;

public enum EOptionUIType
{
    Title,
    Stage,
}

public class OptionUI : UIPopup
{
    [SerializeField] private Button volumeBtn;
    [SerializeField] private Button EmailLinkBtn;
    [SerializeField] private Button ExitBtn;

    // 외부에서 전달받은 '나가기' 동작을 저장할 변수
    private Action onClickExitAction;

    private void Awake()
    {
        volumeBtn.onClick.AddListener(OnClickVolume);
        EmailLinkBtn.onClick.AddListener(OnClickEmailLink);
        ExitBtn.onClick.AddListener(OnClickExit);
    }

    private void Start()
    {
#if UNITY_WEBGL
        EmailLinkBtn.gameObject.SetActive(false);
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
        // 전달받은 동작을 내부 변수에 저장
        onClickExitAction = data.OnClickExitAction;

        // 타입에 따라 버튼 활성화/비활성화 로직은 그대로 사용
        switch (data.Type)
        {
            case EOptionUIType.Title:
                ExitBtn.gameObject.SetActive(true);
                break;

            // 스테이지에서 열었다면 Exit 버튼을 숨김
            case EOptionUIType.Stage:
                ExitBtn.gameObject.SetActive(true);
                break;

            default:
                ExitBtn.gameObject.SetActive(true);
                break;
        }
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
            Title = "Account Link Successful",
            Message = "Your account has been successfully linked.",
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
        UIManager.Instance.Show<VolumeSettingsUI>();
    }

    private void OnClickEmailLink()
    {
        Debug.Log("Email Link Clicked");

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
}
