using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Unity.Services.Analytics;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using Unity.Services.RemoteConfig;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

// 로그인 실패 정보를 전달할 이벤트 인자 클래스
public class LoginErrorArgs : EventArgs
{
    public string Title { get; set; }
    public string Message { get; set; }
}

public class UGSManager : Singleton<UGSManager>
{
    public bool IsInit { get; private set; } = false;
    public bool IsLoggedIn => AuthenticationService.Instance.IsSignedIn;
    public string CurrentUID => IsLoggedIn ? AuthenticationService.Instance.PlayerId : null;
    public string RemoteContentVersion { get; private set; }
    public string SessionId { get; private set; }

    // 로그인 성공 시 외부에 알리기 위한 이벤트
    public event Action OnLoginSuccess;
    // 로그인 실패 이벤트
    public event EventHandler<LoginErrorArgs> OnLoginFailure;
    public event Action OnLinkSuccess;

    // 로그아웃 비동기 작업을 제어하기 위함
    private UniTaskCompletionSource<bool> signOutUcs;

    private string localContentVersion = "1.0.0";

    private bool isLinking = false;

    public bool IsAnonymousUser
    {
        get
        {
            // 로그인 상태가 아니면 무조건 false
            if (!IsLoggedIn)
            {
                return false;
            }

            // 연결된 외부 자격 증명이 하나라도 있으면(Count > 0) 익명 유저가 아니다.
            // 순수한 익명 유저는 연결된 자격 증명이 없으므로 Count가 0이어야 한다.
            return AuthenticationService.Instance.PlayerInfo.Identities.Count == 0;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        // 애플리케이션이 백그라운드(비활성) 상태에서도 계속 실행되도록 설정
        // 이것이 로그인 콜백을 즉시 처리하고 창을 앞으로 가져오는데 도움
        Application.runInBackground = true;

        InitializeUGSAsync().Forget();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (AuthenticationService.Instance != null)
        {
            AuthenticationService.Instance.SignedIn -= OnSignedIn;
            AuthenticationService.Instance.SignedOut -= OnSignedOut;
            AuthenticationService.Instance.SignInFailed -= OnSignInFailed;
        }

        if (PlayerAccountService.Instance != null)
        {
            PlayerAccountService.Instance.SignedIn -= SignInWithUgsAuth;
        }
    }

    #region INITIALIZATION
    public async UniTask InitializeUGSAsync()
    {
        if (IsInit)
        {
            return;
        }

        try
        {
            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += OnSignedIn;
            AuthenticationService.Instance.SignedOut += OnSignedOut;
            AuthenticationService.Instance.SignInFailed += OnSignInFailed;

            PlayerAccountService.Instance.SignedIn += SignInWithUgsAuth;

            SessionId = Guid.NewGuid().ToString();

            AnalyticsService.Instance.StartDataCollection();
            Debug.Log($"Analytics data collection started. Session ID: {SessionId}");

            IsInit = true;
            Debug.Log("UGS initialization success.");

            if (IsLoggedIn)
            {
                OnSignedIn();
            }
        }
        catch (Exception e)
        {
            IsInit = false;
            Debug.LogError($"UGS initialization failed: {e}");
        }
    }

    // Event Handlers
    private void OnSignedIn()
    {
        Debug.Log($"Player signed in successfully! PlayerID: {CurrentUID}");

        // 로그인 성공 시 게임 창을 맨 앞으로 가져오기
        WindowUtils.FocusGameWindow();
        LogLoginEvent();

        // 외부에 로그인 성공을 알림. UI는 이 이벤트를 받아 후처리 진행
        OnLoginSuccess?.Invoke();

        // fire-and-forget 방식으로 호출
        _ = CheckForContentUpdateAsync();
    }

    private void OnSignedOut()
    {
        Debug.Log("Player signed out.");
    }

    private void OnSignInFailed(RequestFailedException e)
    {
        Debug.LogError($"Sign-in failed: {e.Message} (Error Code: {e.ErrorCode})");

        string title = "Login Failed";
        string message;
        int errorCode = e.ErrorCode;

        // static readonly: 프로그램이 실행될 때(런타임) 값이 초기화
        // static readonly 필드는 switch-case에서 사용할 수 없으므로 if-else if 구문으로 처리
        if (errorCode == AuthenticationErrorCodes.InvalidParameters)
        {
            message = "Incorrect login information.";
        }
        else if (errorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
        {
            message = "This social account is already linked to another account.";
        }
        else // 위에서 정의한 특정 인증 에러가 아닌 경우 (네트워크 오류 등)
        {
            string lowerCaseMessage = e.Message.ToLower();
            if (lowerCaseMessage.Contains("timeout"))
            {
                message = "Server response time has timed out.\nPlease check your network connection and try again.";
            }
            else if (lowerCaseMessage.Contains("network") || lowerCaseMessage.Contains("transport") || lowerCaseMessage.Contains("connection"))
            {
                message = "A network connection error occurred.\nPlease check your internet status.";
            }
            else
            {
                message = $"An unknown error occurred.\nPlease try again later. (Code: {errorCode})";
            }
        }

        // 외부 UI에 실패 사실과 메시지를 이벤트로 전달
        OnLoginFailure?.Invoke(this, new LoginErrorArgs { Title = title, Message = message });
    }
    #endregion

    #region AUTHENTICATION (게스트 및 유니티 플레이어 어카운트 로그인)
    /// <summary>
    /// 현재 계정을 완전히 로그아웃하고 로컬 세션까지 삭제한 뒤
    /// 완전히 새로운 게스트 계정으로 로그인합니다.
    /// </summary>
    public async UniTask SwitchToNewGuestAccountAsync()
    {
        // 현재 로그인 상태인지 확인
        if (IsLoggedIn)
        {
            // 현재 세션을 안전하게 종료하고 기다림
            await SignOutAsync();
        }

        // PC에 저장된 이전 사용자 세션 토큰을 완전히 삭제
        AuthenticationService.Instance.ClearSessionToken();
        Debug.Log("Local session token has been cleared.");

        // 완전히 새로운 익명 사용자로 로그인 시도
        Debug.Log("Signing in as a new anonymous user...");
        await SignInAnonymouslyAsync();
    }

    public async UniTask<bool> SignInAnonymouslyAsync()
    {
        if (!IsInit || IsLoggedIn)
        {
            return false;
        }

        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Anonymous sign-in failed: {e}");
            return false;
        }
    }

    /// <summary>
    /// Unity Player Accounts 로그인 절차 시작(UI 버튼에서 호출)
    /// </summary>
    public async UniTask<bool> SignInWithUPAAsync()
    {
        if (!IsInit)
        {
            return false;
        }

        // 이미 다른 UPA 계정으로 로그인 되어 있다면 먼저 로그아웃
        if (IsLoggedIn && !IsAnonymousUser)
        {
            Debug.Log("Already signed in with UPA. Signing out first.");
            await SignOutAsync();
        }

        try
        {
            isLinking = false;
            // 브라우저 기반의 UPA 로그인 UI 연다
            await PlayerAccountService.Instance.StartSignInAsync();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Unity Player Accounts sign-in failed to start: {e}");
            return false;
        }
    }

    /// <summary>
    /// PlayerAccountService.Instance.SignedIn 이벤트가 호출할 콜백 메서드.
    /// UPA 로그인이 성공하면 자동으로 UGS 인증을 시도합니다.
    /// </summary>
    private async void SignInWithUgsAuth()
    {
        if (!IsInit)
        {
            return;
        }

        try
        {
            if (isLinking)
            {
                await AuthenticationService.Instance.LinkWithUnityAsync(PlayerAccountService.Instance.AccessToken);
                Debug.Log("Account successfully linked with Unity Player Account.");

                LogAccountLink("UnityPlayerAccount");

                OnLinkSuccess?.Invoke();
            }
            else
            {
                // UPA로부터 받은 액세스 토큰으로 UGS Authentication에 로그인
                await AuthenticationService.Instance.SignInWithUnityAsync(PlayerAccountService.Instance.AccessToken);
            }
        }
        catch (AuthenticationException ex)
        { 
            Debug.LogException(ex);
            // 서버 연동이 실패 -> 로컬 UPA 로그인을 취소(로그아웃)하여 상태 초기화
            if (PlayerAccountService.Instance.IsSignedIn)
            {
                PlayerAccountService.Instance.SignOut();
            }

            if (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
            {
                OnLoginFailure?.Invoke(this, new LoginErrorArgs
                {
                    Title = "Linking Failed",
                    Message = "The selected Unity account is already linked to other game data."
                });
            }
            else
            {
                // 그 외 다른 인증 관련 에러 처리
                OnLoginFailure?.Invoke(this, new LoginErrorArgs
                {
                    Title = "Linking Failed",
                    Message = $"Authentication failed. (코드: {ex.ErrorCode})"
                });
            }
        }
        catch (RequestFailedException ex) 
        { 
            Debug.LogException(ex);
            OnLoginFailure?.Invoke(this, new LoginErrorArgs
            {
                Title = "Request Failed",
                Message = $"Server request failed.\nPlease check your network status. (코드: {ex.ErrorCode})"
            });
        }
        finally
        {
            // 게임 창을 맨 앞으로 가져오기
            WindowUtils.FocusGameWindow();
        }
    }

    /// <summary>
    /// 게스트 계정을 Unity Player Accounts와 연결
    /// </summary>
    public async UniTask LinkWithUPAAsync()
    {
        if (!IsLoggedIn)
        {
            Debug.LogWarning("Cannot link because no user is signed in to UGS Authentication.");
            return;
        }

        try
        {
            isLinking = true;

            // 연동을 위해서도 먼저 UPA 로그인을 시도해야 토큰을 얻을 수 있음
            await PlayerAccountService.Instance.StartSignInAsync();
        }
        catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
        {
            Debug.LogError("This UPA is already linked with another account.");
            OnLoginFailure?.Invoke(this, new LoginErrorArgs
            {
                Title = "Linking Failed",
                Message = "This Unity account is already linked to another game account."
            });
        }
        catch (PlayerAccountsException ex)
        {
            // 이미 UPA에 로그인 되어 있는 상태일 때
            Debug.LogError($"Link with UPA failed: {ex.Message}");
            OnLoginFailure?.Invoke(this, new LoginErrorArgs
            {
                Title = "Linking Failed",
                Message = "You are already logged in to a Unity account."
            });
        }
        catch (Exception e)
        {
            Debug.LogError($"Link with UPA failed: {e}");
            
            OnLoginFailure?.Invoke(this, new LoginErrorArgs
            {
                Title = "Linking Failed",
                Message = $"An unknown error occurred.\nPlease check your network status and try again."
            });
        }
    }

    /// <summary>
    /// UGS 및 Unity Player Accounts에서 모두 로그아웃
    /// </summary>
    public void SignOut()
    {
        _ = SignOutAsync(); // fire-and-forget
    }

    /// <summary>
    /// 비동기적으로 로그아웃하고 완료될 때까지 기다림
    /// </summary>
    public async UniTask SignOutAsync()
    {
        if (!IsLoggedIn)
        {
            return;
        }

        // UPA 로그아웃 (로그인 상태일 때만)
        if (PlayerAccountService.Instance.IsSignedIn)
        {
            PlayerAccountService.Instance.SignOut();
        }

        // TaskCompletionSource 초기화
        signOutUcs = new UniTaskCompletionSource<bool>();

        // 로그아웃 완료를 처리할 이벤트 구독
        AuthenticationService.Instance.SignedOut += OnSignOutCompletedForTask;

        // 로그아웃 시작
        AuthenticationService.Instance.SignOut(true);

        // 멤버 필드의 Task가 완료되기를 대기
        await signOutUcs.Task;
    }

    /// <summary>
    /// SignOutAsync의 Task를 완료시키는 전용 이벤트 핸들러
    /// </summary>
    private void OnSignOutCompletedForTask()
    {
        // 이벤트가 중복 호출되지 않도록 즉시 구독 해제
        AuthenticationService.Instance.SignedOut -= OnSignOutCompletedForTask;

        // null 체크 후 Task를 성공 상태로 전환
        signOutUcs?.TrySetResult(true);
    }
    #endregion

    #region ANALYTICS
    // 모든 이벤트에 세션 ID를 포함시키는 범용 이벤트 기록 메서드
    private void RecordAnalyticsEvent(string eventName, Dictionary<string, object> parameters)
    {
        if (!IsInit)
        {
            return;
        }

        // CustomEvent는 이름으로만 생성한 뒤 파라미터를 하나씩 추가
        try
        {
            var eventToRecord = new CustomEvent(eventName);
            foreach (var param in parameters)
            {
                eventToRecord.Add(param.Key, param.Value);
            }

            AnalyticsService.Instance.RecordEvent(eventToRecord);
            Debug.Log($"Analytics: Logged '{eventName}'.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error recording analytics event '{eventName}': {e}");
        }
    }

    private void LogLoginEvent()
    {
        var identities = AuthenticationService.Instance.PlayerInfo.Identities;
        Debug.LogWarning($"[UGS DEBUG] 현재 플레이어는 {identities.Count}개의 자격 증명을 가지고 있습니다.");
        foreach (var identity in identities)
        {
            Debug.LogWarning($"[UGS DEBUG] ---> 자격 증명 타입 ID: {identity.TypeId}");
        }

        string loginMethod = IsLoggedIn && IsAnonymousUser ? "Anonymous" : "Unity";
        RecordAnalyticsEvent("player_login", new Dictionary<string, object>
        {
            { "login_method", loginMethod }
        });
    }

    /// <summary>
    /// 계정 연동 성공 이벤트 기록
    /// </summary>
    /// <param name="providerName">연동한 서비스 이름 (예: "UnityPlayerAccount")</param>
    public void LogAccountLink(string providerName)
    {
        RecordAnalyticsEvent("account_linked", new Dictionary<string, object>
    {
        { "link_method", providerName }
    });
    }

    public void LogStageStart(int stageId)
    {
        RecordAnalyticsEvent("stage_start", new Dictionary<string, object>
        {
            { "stage_id", stageId }
        });
    }

    public void LogStageResult(int stageId, bool isClear, float playTime, int bossHpRatio)
    {
        RecordAnalyticsEvent("stage_result", new Dictionary<string, object>
        {
            { "stage_id", stageId },
            { "result", isClear },
            { "play_time", playTime },
            { "boss_hp_percent", bossHpRatio}
        });
    }

    /// <summary>
    /// 스킬이 성공적으로 실행되었을 때 발생함.
    /// </summary>
    /// <param name="stageId"></param>
    /// <param name="skillId"></param>
    public void LogSkillUsage(int stageId, int skillId)
    {
        RecordAnalyticsEvent("skill_use", new Dictionary<string, object>
        {
            { "stage_id", stageId },
            { "skill_id", skillId }
        });
    }

    /// <summary>
    /// 스킬 말고 다른 행동을 할 때 발생함.
    /// </summary>
    /// <param name="stageId"></param>
    /// <param name="actionId"></param>
    public void LogDoAction(int stageId, int actionId)
    {
        RecordAnalyticsEvent("do_action", new Dictionary<string, object>
        {
            { "stage_id", stageId },
            { "action_id", actionId }
        });
    }


    /// <summary>
    /// 특정 퍼센티지(100%, 90%, 80%...)마다 불릴 것
    /// </summary>
    /// <param name="stageId"></param>
    /// <param name="bossHpRatio"></param>
    /// <param name="elapsedTime"></param>
    public void LogInGameBossHp(int stageId, int bossHpRatio, float elapsedTime)
    {
        RecordAnalyticsEvent("in_game_boss_hp", new Dictionary<string, object>
        {
            { "stage_id", stageId },
            { "boss_hp_percent", bossHpRatio },
            { "play_time", elapsedTime }
        });
    }
    
    #endregion

    #region CLOUD SAVE
    public async UniTask<bool> SaveUserDataAsync(int slotIndex, UserData data)
    {
        if (!IsLoggedIn)
        {
            return false;
        }

        try
        {
            string jsonData = JsonUtility.ToJson(data);
            var dataToSave = new Dictionary<string, object> { { $"save_slot_{slotIndex}", jsonData } };
            await CloudSaveService.Instance.Data.Player.SaveAsync(dataToSave);
            Debug.Log($"User data for slot {slotIndex} saved successfully.");
            return true;
        }
        catch (Exception e) { Debug.LogError($"Error saving user data: {e}"); return false; }
    }

    public async UniTask<Dictionary<int, UserData>> LoadAllUserDataAsync()
    {
        var allSlotsData = new Dictionary<int, UserData>();
        if (!IsLoggedIn)
        {
            return allSlotsData;
        }

        try
        {
            var savedData = await CloudSaveService.Instance.Data.Player.LoadAllAsync();
            for (int i = 0; i < 3; i++)
            {
                string slotKey = $"save_slot_{i}";
                if (savedData.TryGetValue(slotKey, out var value))
                {
                    allSlotsData[i] = JsonUtility.FromJson<UserData>(value.Value.GetAsString());
                }
            }
            return allSlotsData;
        }
        catch (Exception e) { Debug.LogError($"Error loading user data: {e}"); return new Dictionary<int, UserData>(); }
    }

    public async UniTask<bool> DeleteUserDataAsync(int slotIndex)
    {
        if (!IsLoggedIn)
        {
            Debug.LogWarning("Cannot delete data. User is not logged in.");
            return false;
        }

        try
        {
            string slotKey = $"save_slot_{slotIndex}";

            await CloudSaveService.Instance.Data.Player.DeleteAsync(slotKey,
                new Unity.Services.CloudSave.Models.Data.Player.DeleteOptions());

            Debug.Log($"User data for slot {slotIndex} (key: {slotKey}) deleted successfully.");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error deleting user data for slot {slotIndex}: {e}");
            return false;
        }
    }
    #endregion

    #region REMOTE CONTENT (Addressables + CCD)
    private struct UserAttributes { }
    private struct AppAttributes { }

    public async UniTask CheckForContentUpdateAsync()
    {
        if (!IsInit)
        {
            return;
        }

        RemoteConfigService.Instance.FetchCompleted += OnFetchCompleted;
        await RemoteConfigService.Instance.FetchConfigsAsync(new UserAttributes(), new AppAttributes());
    }

    private void OnFetchCompleted(ConfigResponse response)
    {
        RemoteConfigService.Instance.FetchCompleted -= OnFetchCompleted;

        RemoteContentVersion = RemoteConfigService.Instance.appConfig.GetString("contentVersion", "1.0.0");
        Debug.Log($"Local Content Version: {localContentVersion}, Remote Content Version: {RemoteContentVersion}");

        if (RemoteContentVersion != localContentVersion)
        {
            Debug.Log("New content found! Starting update...");
            _ = UpdateAddressablesCatalogAsync(); // fire-and-forget
        }
        else
        {
            Debug.Log("Content is up to date.");
        }
    }

    private async UniTask UpdateAddressablesCatalogAsync()
    {
        var handle = Addressables.CheckForCatalogUpdates(false);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result.Count > 0)
        {
            Debug.Log($"Found {handle.Result.Count} catalogs to update.");

            var updateHandle = Addressables.UpdateCatalogs(handle.Result, false);
            await updateHandle.Task;

            if (updateHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("Addressables catalog updated successfully.");
                localContentVersion = RemoteContentVersion;
            }
            else
            {
                Debug.LogError("Addressables catalog update failed.");
            }
            Addressables.Release(updateHandle);
        }
        else
        {
            Debug.Log("No new catalogs found to update.");
        }

        Addressables.Release(handle);
    }
    #endregion
}
