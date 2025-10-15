using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EOptionUIType
{
    Stage,
}

public class OptionUI : UIPopup
{
    [SerializeField] private Button volumeBtn;
    [SerializeField] private Button keyBindBtn;
    [SerializeField] private Button EmailLinkBtn;
    [SerializeField] private Button ExitBtn;

    private void Awake()
    {
        volumeBtn.onClick.AddListener(OnClickVolume);
        keyBindBtn.onClick.AddListener(OnClickKeyBind);
        EmailLinkBtn.onClick.AddListener(OnClickEmailLink);
        ExitBtn.onClick.AddListener(OnClickExit);
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
            Title = "연동 성공",
            Message = "계정이 성공적으로 연동되었습니다.",
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

    /// <summary>
    /// Enum 타입에 따라 OptionUI의 상태 설정
    /// </summary>
    public void Setup(EOptionUIType type)
    {
        // 타입에 따라 버튼 활성화/비활성화
        switch (type)
        {
            // 스테이지에서 열었다면 Exit 버튼을 숨김
            case EOptionUIType.Stage:
                ExitBtn.gameObject.SetActive(false);
                break;

            default:
                ExitBtn.gameObject.SetActive(true);
                break;
        }
    }

    private void OnClickVolume()
    {
        Debug.Log("Volume Clicked");
        UIManager.Instance.Show<VolumeSettingsUI>();
    }

    private void OnClickKeyBind()
    {
        Debug.Log("Key Bind Clicked");

        // TODO: KeyBindUI 구현 후 연결
    }

    private void OnClickEmailLink()
    {
        Debug.Log("Email Link Clicked");

        _ = UGSManager.Instance.LinkWithUPAAsync();
    }

    private void OnClickExit()
    {
        Debug.Log("Exit Clicked");
        UIManager.Instance.Hide<OptionUI>();
    }
}
