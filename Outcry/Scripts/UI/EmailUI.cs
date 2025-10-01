using Firebase.Analytics;
using FirebaseEnums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum EEmailUIType
{
    SignUp, // 회원가입 및 로그인 목적
    Link    // 기존 계정(게스트)에 이메일 연동 목적
}

public class EmailUI : UIPopup
{
    [SerializeField] private Button signUpBtn;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button linkButton;
    [SerializeField] private Button exitButton;

    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;

    private string email;
    private string password;

    private void Awake()
    {
        signUpBtn.onClick.AddListener(OnClickSignUp);
        loginButton.onClick.AddListener(OnClickEmailLogin);
        linkButton.onClick.AddListener(OnClickLink);
        exitButton.onClick.AddListener(OnClickExit);
    }

    /// <summary>
    /// EmailUI를 특정 타입으로 설정하는 메서드. UI를 Show()한 직후 호출
    /// </summary>
    public void Setup(EEmailUIType type)
    {
        // 입력 필드 초기화
        emailInput.text = "";
        passwordInput.text = "";

        // 타입에 따라 버튼 활성화/비활성화
        switch (type)
        {
            case EEmailUIType.SignUp:
                signUpBtn.gameObject.SetActive(true);
                loginButton.gameObject.SetActive(true);
                linkButton.gameObject.SetActive(false);
                break;

            case EEmailUIType.Link:
                signUpBtn.gameObject.SetActive(false);
                loginButton.gameObject.SetActive(false);
                linkButton.gameObject.SetActive(true);
                break;
        }
    }

    private async void OnClickSignUp()
    {
        email = emailInput.text;
        password = passwordInput.text;

        // 입력 필드 유효성 검사
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.Log("이메일과 비밀번호를 모두 입력해주세요.");
            // TODO: 이메일, 비밀번호 입력 경고 UI 띄우기
            return;
        }

        ESignUpResult result = await FirebaseManager.Instance.SignUpWithEmailAsync(email, password);
        switch (result)
        {
            case ESignUpResult.Success:
                Debug.Log("회원가입에 성공했습니다!");
                // TODO: 가입 성공 UI
                break;
            case ESignUpResult.EmailAlreadyInUse:
                Debug.Log("이미 사용 중인 이메일입니다.");
                // TODO: 이유에 따른 경고 UI
                break;
            case ESignUpResult.WeakPassword:
                Debug.Log("비밀번호가 너무 단순합니다. (6자 이상)");
                // TODO: 이유에 따른 경고 UI
                break;
            case ESignUpResult.InvalidEmail:
                Debug.Log("올바른 이메일 형식이 아닙니다.");
                // TODO: 이유에 따른 경고 UI
                break;
            case ESignUpResult.UnknownError:
            default:
                Debug.Log("알 수 없는 오류가 발생했습니다. 잠시 후 다시 시도해주세요.");
                // TODO: 이유에 따른 경고 UI
                break;
        }
    }

    private async void OnClickEmailLogin()
    {
        email = emailInput.text;
        password = passwordInput.text;

        // 입력 필드 유효성 검사
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.Log("이메일과 비밀번호를 모두 입력해주세요.");
            // TODO: 이메일, 비밀번호 입력 경고 UI 띄우기
            return;
        }

        ESignInResult result = await FirebaseManager.Instance.SignInWithEmailAsync(email, password);
        switch (result)
        {
            case ESignInResult.Success:
                Debug.Log("Email Login Success!");

                FirebaseAnalytics.LogEvent("login_email");

                // 유저 데이터 로드
                await SaveLoadManager.Instance.LoadAllUserData();

                UIManager.Instance.Hide<EmailUI>();
                SaveLoadManager.Instance.OpenUI(ESlotUIType.Load);
                break;
            case ESignInResult.UserNotFound:
                Debug.Log("가입되지 않은 이메일입니다.");
                // TODO: 이유에 따른 경고 UI
                break;
            case ESignInResult.WrongPassword:
                Debug.Log("비밀번호가 일치하지 않습니다.");
                // TODO: 이유에 따른 경고 UI
                break;
            case ESignInResult.InvalidEmail:
                Debug.Log("올바른 이메일 형식이 아닙니다.");
                // TODO: 이유에 따른 경고 UI
                break;
            case ESignInResult.UnknownError:
            default:
                Debug.Log("알 수 없는 오류로 로그인에 실패했습니다.");
                // TODO: 이유에 따른 경고 UI
                break;
        }
    }

    private async void OnClickLink()
    {
        email = emailInput.text;
        password = passwordInput.text;

        // 입력 필드 유효성 검사
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.Log("이메일과 비밀번호를 모두 입력해주세요.");
            // TODO: 이메일, 비밀번호 입력 경고 UI 띄우기
            return;
        }

        ELinkResult result = await FirebaseManager.Instance.LinkEmailToCurrentUserAsync(email, password);
        
        switch (result)
        {
            case ELinkResult.Success:
                Debug.Log("계정이 성공적으로 연동되었습니다!");
                // TODO: 성공 UI
                break;
            case ELinkResult.NoAnonymousUser:
                Debug.Log("게스트로 로그인된 상태에서만 계정을 연동할 수 있습니다.");
                // TODO: 이유에 따른 경고 UI
                break;
            case ELinkResult.EmailAlreadyInUse:
                Debug.Log("해당 이메일은 이미 다른 계정에서 사용 중입니다.");
                // TODO: 이유에 따른 경고 UI
                break;
            case ELinkResult.CredentialAlreadyInUse:
                Debug.Log("이미 연동된 계정 정보입니다.");
                // TODO: 이유에 따른 경고 UI
                break;
            case ELinkResult.WeakPassword:
                Debug.Log("비밀번호가 너무 단순합니다. (6자 이상)");
                // TODO: 이유에 따른 경고 UI
                break;
            case ELinkResult.UnknownError:
            default:
                Debug.Log("알 수 없는 오류로 계정 연동에 실패했습니다.");
                // TODO: 이유에 따른 경고 UI
                break;
        }
    }

    private void OnClickExit()
    {
        UIManager.Instance.Hide<EmailUI>();
    }
}
