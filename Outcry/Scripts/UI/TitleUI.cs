using Firebase.Analytics;
using UnityEngine;
using UnityEngine.UI;

public class TitleUI : UIBase
{
    [SerializeField] private Button guestLoginBtn;
    [SerializeField] private Button emailLoginBtn;
    [SerializeField] private Button settingsBtn;
    [SerializeField] private Button quitBtn;

    private void Awake()
    {
        guestLoginBtn.onClick.AddListener(OnClickGuestLogin);
        emailLoginBtn.onClick.AddListener(OnClickEmailLogin);
        settingsBtn.onClick.AddListener(OnClickSettings);
        quitBtn.onClick.AddListener(OnClickQuit);
    }

    private async void OnClickGuestLogin()
    {
        Debug.Log("Guest Login Clicked");

        // 이미 다른 계정으로 로그인 되어 있다면 먼저 로그아웃
        if (FirebaseManager.Instance.IsLoggedIn && !FirebaseManager.Instance.IsAnonymousUser)
        {
            Debug.Log("이메일 계정 로그아웃 후, 새로운 게스트 계정으로 전환합니다.");
            await FirebaseManager.Instance.SignOutAsync();
        }

        // 현재 로그인 되어 있지 않다면 (기존에 로그아웃했거나 원래 로그아웃 상태) 익명 로그인 시도
        if (!FirebaseManager.Instance.IsLoggedIn)
        {
            bool success = await FirebaseManager.Instance.SignInAnonymouslyAsync();
            if (!success)
            {
                // TODO: 게스트 로그인 실패 UI 처리
                Debug.LogError("Guest Login Failed!");
                return; // 로그인에 실패했으므로 여기서 중단
            }
        }

        // 성공적으로 게스트로 로그인된 상태 보장
        Debug.Log("게스트 계정으로 로그인되었습니다. 데이터 로드 UI로 이동합니다.");
        FirebaseAnalytics.LogEvent("login_anonymous"); // 게스트 로그인 이벤트 로깅

        // 유저 데이터 로드 후 다음 UI로 이동
        await SaveLoadManager.Instance.LoadAllUserData();
        SaveLoadManager.Instance.OpenUI(ESlotUIType.Load);
    }

    private void OnClickEmailLogin()
    {
        Debug.Log("Email Login Clicked");

        if (FirebaseManager.Instance.IsLoggedIn)
        {
            FirebaseManager.Instance.SignOut();
        }

        EmailUI email = UIManager.Instance.Show<EmailUI>();
        email.Setup(EEmailUIType.SignUp);
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
        UIManager.Instance.Show<OptionUI>();
    }

    private void OnClickQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
