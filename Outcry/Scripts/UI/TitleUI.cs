using Cysharp.Threading.Tasks;
using StageEnums;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleUI : UIBase
{
    [SerializeField] private Button guestLoginBtn;
    [SerializeField] private TextMeshProUGUI guestLoginTxt;
    [SerializeField] private Button emailLoginBtn;
    [SerializeField] private Button settingsBtn;
    [SerializeField] private Button quitBtn;

    // UPA 로그인 시도를 추적하기 위한 플래그
    private bool isAttemptingUPALogin = false;

    private Action onLoginSuccessHandler;

    private void Awake()
    {
        guestLoginBtn.onClick.AddListener(OnClickGuestLogin);
        emailLoginBtn.onClick.AddListener(OnClickEmailLogin);
        settingsBtn.onClick.AddListener(OnClickSettings);
        quitBtn.onClick.AddListener(OnClickQuit);

        onLoginSuccessHandler = () => HandleLoginSuccess().Forget();
    }

    private void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        emailLoginBtn.gameObject.SetActive(false);
#endif

        // UGSManager가 준비되었는지 확인 후 이벤트 구독
        if (UGSManager.Instance != null)
        {
            UGSManager.Instance.OnLoginSuccess += onLoginSuccessHandler;
            UGSManager.Instance.OnLoginFailure += HandleLoginFailure;
        }
    }

    private void OnDestroy()
    {
        // UGSManager가 TitleUI보다 먼저 파괴될 수 있는 예외적인 경우(예: 게임 종료 시) 대비
        if (UGSManager.Instance != null)
        {
            UGSManager.Instance.OnLoginSuccess -= onLoginSuccessHandler;
            UGSManager.Instance.OnLoginFailure -= HandleLoginFailure;
        }
    }

    // 게임 창 포커스 변경 시 호출되는 Unity 이벤트 메서드
    private void OnApplicationFocus(bool hasFocus)
    {
        // 게임 창이 다시 활성화되었을 때(포커스를 얻었을 때)
        if (hasFocus)
        {
            // 이메일 로그인을 시도하다가 돌아왔고 아직 로그인이 안 된 상태라면
            if (isAttemptingUPALogin && !UGSManager.Instance.IsLoggedIn)
            {
                Debug.Log("Login cancelled by user. Re-enabling buttons.");
                // 버튼을 다시 활성화하고 플래그를 리셋
                SetButtonsInteractable(true);
                isAttemptingUPALogin = false;
            }
        }
    }

    /// <summary>
    /// 로그인 성공 이벤트가 발생하면 호출될 핸들러
    /// </summary>
    private async UniTask HandleLoginSuccess()
    {
        Debug.Log("Login successful! Loading user data and transitioning UI.");

        isAttemptingUPALogin = false;

        await SaveLoadManager.Instance.LoadAllUserData();

        var allSlotsData = SaveLoadManager.Instance.SlotsData;

        // 게스트 로그인은 바로 튜토리얼 시작
        if (UGSManager.Instance.IsAnonymousUser && allSlotsData.Count == 0)
        {
            GameManager.Instance.StartNewGameAsaGuest().Forget();
            return;
        }

        // SaveLoadUI의 나가기 버튼에 주입할 동작 정의
        var exitActionData = new SaveLoadUIData
        {
            OnClickExitAction = async () =>
            {
                // UI 닫고
                UIManager.Instance.Hide<SaveLoadUI>();

                // UGS 로그아웃 비동기 실행
                await UGSManager.Instance.SignOutAsync();

                // TitleUI의 버튼들 다시 활성화
                SetButtonsInteractable(true);

                Debug.Log("Signed out and returned to Title Screen.");
            }
        };

        // 정의된 동작(exitActionData)과 함께 SaveLoadUI 열기
        SaveLoadManager.Instance.OpenUI(ESlotUIType.Load, exitActionData);
    }

    /// <summary>
    /// 로그인 실패 이벤트가 발생하면 호출될 핸들러
    /// </summary>
    private void HandleLoginFailure(object sender, LoginErrorArgs args)
    {
        Debug.LogWarning($"Login failed: {args.Title} - {args.Message}");

        isAttemptingUPALogin = false;

        // 에러 팝업UI 을 띄워 사용자에게 실패 원인을 알려줌
        var popup = UIManager.Instance.Show<ConfirmUI>();

        // 로그인 실패 메시지를 담은 데이터 생성
        var popupData = new ConfirmPopupData
        {
            Title = args.Title,     // UGSManager에서 전달받은 제목
            Message = args.Message, // UGSManager에서 전달받은 메시지
            Type = EConfirmPopupType.OK, // 확인 버튼만 필요
            OnClickOK = null // 확인 버튼 클릭 시 특별한 동작은 없음
        };

        // 데이터로 팝업UI 설정
        popup.Setup(popupData);

        // 사용자가 다시 시도할 수 있도록 버튼 활성화
        SetButtonsInteractable(true);
    }

    private void OnClickGuestLogin()
    {
        Debug.Log("Guest Login Clicked");
        EffectManager.Instance.ButtonSound();
        var popup = UIManager.Instance.Show<ConfirmUI>();
#if UNITY_WEBGL  && !UNITY_EDITOR
        popup.Setup(new ConfirmPopupData
        {
            Title = LocalizationUtility.GetLocalizedValueByKey(LocalizationStrings.UI.WARNING),
            Message = LocalizationUtility.ChooseLocalizedString(
                "Saving is not available in the WebGL version.\n Please launch the Windows version and log in to save your game.",
                "WebGL 버전에서는 저장 기능을 사용할 수 없습니다.\n게임을 저장하려면 Windows 버전을 실행하고 로그인해 주십시오."),
            Type = EConfirmPopupType.OK_CANCEL,
            OnClickOK = () =>
            {
                SetButtonsInteractable(false);
                UGSManager.Instance.SwitchToNewGuestAccountAsync().Forget();
            },
            OnClickCancel = null
        });
#else
        popup.Setup(new ConfirmPopupData
        {
            // Title = "Warning",
            // Message = "Guest accounts do not save data.\n You can save after linking your account.",
            Title = LocalizationUtility.GetLocalizedValueByKey(LocalizationStrings.UI.WARNING),
            Message = LocalizationUtility.GetLocalizedValueByKey(LocalizationStrings.SaveLoad.MESSAGE_GUESTWARNING),
            Type = EConfirmPopupType.OK_CANCEL,
            OnClickOK = () =>
            {
                SetButtonsInteractable(false);
                UGSManager.Instance.SwitchToNewGuestAccountAsync().Forget();
            },
            OnClickCancel = null
        });
#endif
    }

    private async void OnClickEmailLogin()
    {
        Debug.Log("Email Login Clicked");
        EffectManager.Instance.ButtonSound();
        // 로그인 시도 플래그 true로 설정하고 버튼 비활성화
        isAttemptingUPALogin = true;

        SetButtonsInteractable(false);
        await UGSManager.Instance.SignInWithUPAAsync();
    }

    public void SetButtonsInteractable(bool isInteractable)
    {
        guestLoginBtn.interactable = isInteractable;
        emailLoginBtn.interactable = isInteractable;
        settingsBtn.interactable = isInteractable;
        quitBtn.interactable = isInteractable;
    }

    private void OnClickSettings()
    {
        EffectManager.Instance.ButtonSound();
        var optionPopup = UIManager.Instance.Show<OptionUI>();
        optionPopup.Setup(new OptionUIData
        {
            // ExitText = "Exit",
            ExitText = LocalizationUtility.GetLocalizedValueByKey(LocalizationStrings.UI.EXIT),
            Type = EOptionUIType.Title,
            OnClickExitAction = null // 기본 동작(UI 닫기)
        });
    }

    private void OnClickQuit()
    {
        GameManager.Instance.QuitGame();
    }
}
