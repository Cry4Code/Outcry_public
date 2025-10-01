using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Analytics;
using Firebase.Extensions; // ContinueWithOnMainThread를 위해 필수
using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using FirebaseEnums;

public class FirebaseManager : Singleton<FirebaseManager>
{
    // 파이어베이스가 제공하는 모든 기능에 접근 가능하도록 하는 클래스
    private FirebaseApp app;

    // 파이어베이스 서비스
    private FirebaseAuth auth;
    private FirebaseUser user;
    private FirebaseFirestore db;

    // 서비스 초기화 플래그
    private bool bIsAuthInit = false;
    private bool bIsAnalyticsInit = false;
    private bool bIsFirestoreInit = false;

    // 현재 로그인된 유저가 있는지 확인하는 프로퍼티
    public bool IsLoggedIn => auth != null && auth.CurrentUser != null;
    public bool IsAnonymousUser => user != null && user.IsAnonymous;
    public string CurrentUserUID => user?.UserId;

    protected override void Awake()
    {
        base.Awake();

        StartCoroutine(InitFirebaseServiceCo());
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (auth != null)
        {
            auth.StateChanged -= OnAuthStateChanged;
            auth = null;
        }
    }

    /// Firebase 서비스 초기화 확인
    public bool IsInit()
    {
        return bIsAuthInit && bIsAnalyticsInit && bIsFirestoreInit;
    }

    // Firebase 서비스 초기화를 진행하는 코루틴
    private IEnumerator InitFirebaseServiceCo()
    {
        // Firebase 종속성 확인 및 수정
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                Debug.Log("FirebaseApp initialization success.");

                // FirebaseApp 인스턴스 초기화
                app = FirebaseApp.DefaultInstance;

                // 파이어베이스 서비스 초기화
                InitAuth();
                InitAnalytics();
                InitFirestore();
            }
            else
            {
                Debug.LogError($"FirebaseApp initialization failed. DependencyStatus:{dependencyStatus}");
            }
        });

        float elapsedTime = 0f;
        const float INIT_TIMEOUT = 10.0f; // 초기화 타임아웃 10초로 설정

        // IsInit()가 true가 되거나, 타임아웃 시간이 될 때까지 대기
        while (!IsInit() && elapsedTime < INIT_TIMEOUT)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 루프 종료 후 최종 결과 확인
        if (IsInit())
        {
            Debug.Log($"{GetType()} initialization success.");
        }
        else
        {
            Debug.LogError($"{GetType()} initialization failed within the timeout period.");
        }
    }

    #region AUTH
    /// <summary>
    /// Firebase Auth 서비스 초기화
    /// </summary>
    private void InitAuth()
    {
        auth = FirebaseAuth.DefaultInstance;

        // TODO: 임시로 기존에 로그인된 사용자가 있다면 즉시 로그아웃 -> 회의 후 결정(자동 로그인 유지 vs 매번 새로 로그인)
        //if (auth.CurrentUser != null)
        //{
        //    auth.SignOut();
        //    Debug.Log("An existing user was signed out during initialization.");
        //}

        // 사용자의 로그인 상태가 바뀔 때마다(로그인, 로그아웃) OnAuthStateChanged 함수를 호출하도록 등록
        auth.StateChanged += OnAuthStateChanged;
        bIsAuthInit = true;
        Debug.Log("Firebase Auth initialized.");
    }

    /// <summary>
    /// 로그인 상태 변경 시 호출되는 이벤트 핸들러
    /// </summary>
    private void OnAuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = (auth.CurrentUser != null);
            if (signedIn)
            {
                user = auth.CurrentUser;
                Debug.Log($"Firebase User Signed In: {user.UserId}");

                // 사용자의 로그인 유형에 따라 꼬리표를 붙임
                if (user.IsAnonymous)
                {
                    FirebaseAnalytics.SetUserProperty("login_type", "anonymous");
                }
                else
                {
                    FirebaseAnalytics.SetUserProperty("login_type", "email");
                }

                // TODO: 로그인 성공 후 필요한 작업 (예: 유저 데이터 로드)을 여기서 시작?
            }
            else
            {
                Debug.Log("Firebase User Signed Out.");
                user = null;
            }
        }
    }

    /// <summary>
    /// 익명으로 Firebase에 로그인합니다. (게스트 로그인)
    /// </summary>
    /// <returns>로그인 성공 여부 Task</returns>
    public async Task<bool> SignInAnonymouslyAsync()
    {
        if (!IsInit() || user != null)
        {
            Debug.LogWarning("Firebase not initialized or user already signed in.");
            return false;
        }

        try
        {
            // 익명 로그인 시도
            // AuthResult 객체로 결과를 받음
            AuthResult result = await auth.SignInAnonymouslyAsync();
            // 결과 객체 안의 User 프로퍼티 사용
            FirebaseUser newUser = result.User;
            Debug.Log($"Anonymous sign-in successful. User ID: {newUser.UserId}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Anonymous sign-in failed with exception: {e}");
            return false;
        }
    }

    /// <summary>
    /// 이메일과 비밀번호로 새 계정 생성(회원가입)
    /// </summary>
    public async Task<ESignUpResult> SignUpWithEmailAsync(string email, string password)
    {
        if (!IsInit())
        {
            Debug.LogWarning("Firebase not initialized.");
            return ESignUpResult.UnknownError;
        }

        try
        {
            AuthResult result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            FirebaseUser newUser = result.User;
            Debug.Log($"Email sign-up successful. User ID: {newUser.UserId}");
            return ESignUpResult.Success;
        }
        catch (FirebaseException e)
        {
            // Firebase 관련 에러 처리
            AuthError errorCode = (AuthError)e.ErrorCode;
            switch (errorCode)
            {
                case AuthError.EmailAlreadyInUse:
                    Debug.LogError("Email already in use.");
                    return ESignUpResult.EmailAlreadyInUse;

                case AuthError.WeakPassword:
                    Debug.LogError("Password is too weak.");
                    return ESignUpResult.WeakPassword;

                case AuthError.InvalidEmail:
                    Debug.LogError("Email format is invalid.");
                    return ESignUpResult.InvalidEmail;

                default:
                    Debug.LogError($"Sign-up failed with Firebase exception: {e}");
                    return ESignUpResult.UnknownError;
            }
        }
        catch (Exception e)
        {
            // 기타 에러 처리
            Debug.LogError($"Sign-up failed with exception: {e}");
            return ESignUpResult.UnknownError;
        }
    }

    /// <summary>
    /// 이메일과 비밀번호로 로그인
    /// </summary>
    public async Task<ESignInResult> SignInWithEmailAsync(string email, string password)
    {
        if (!IsInit())
        {
            Debug.LogWarning("Firebase not initialized.");
            return ESignInResult.UnknownError;
        }

        try
        {
            AuthResult result = await auth.SignInWithEmailAndPasswordAsync(email, password);
            FirebaseUser signInuser = result.User;
            Debug.Log($"Email sign-in successful. User ID: {signInuser.UserId}");
            return ESignInResult.Success;
        }
        catch (FirebaseException e)
        {
            AuthError errorCode = (AuthError)e.ErrorCode;
            switch (errorCode)
            {
                case AuthError.WrongPassword:
                    Debug.LogError("Wrong password.");
                    return ESignInResult.WrongPassword;

                case AuthError.UserNotFound:
                    Debug.LogError("User not found.");
                    return ESignInResult.UserNotFound;

                case AuthError.InvalidEmail:
                    Debug.LogError("Email format is invalid.");
                    return ESignInResult.InvalidEmail;

                default:
                    Debug.LogError($"Sign-in failed with Firebase exception: {e}");
                    return ESignInResult.UnknownError;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Sign-in failed with exception: {e}");
            return ESignInResult.UnknownError;
        }
    }

    /// <summary>
    /// 현재 사용자 로그아웃
    /// </summary>
    public void SignOut()
    {
        if (auth.CurrentUser != null)
        {
            Debug.Log("Signing out current user.");

            auth.SignOut();
        }
    }

    /// <summary>
    /// 현재 사용자를 비동기적으로 로그아웃하고 완료될 때까지 기다림
    /// </summary>
    public Task SignOutAsync()
    {
        // 이미 로그아웃 상태이면 즉시 완료된 Task 반환
        if (!IsLoggedIn)
        {
            return Task.CompletedTask;
        }

        // 비동기 작업의 완료를 수동으로 제어하기 위한 TaskCompletionSource 생성
        var tcs = new TaskCompletionSource<bool>();

        // 로그인 상태가 변경될 때 호출될 임시 이벤트 핸들러 정의
        void AuthStateChangedHandler(object sender, System.EventArgs eventArgs)
        {
            // 사용자가 실제로 로그아웃(null)되었는지 확인
            if (auth.CurrentUser == null)
            {
                // 이벤트가 중복 호출되지 않도록 즉시 구독 해제
                auth.StateChanged -= AuthStateChangedHandler;

                // Task를 성공 상태로 만들어 await 구문이 다음으로 진행되게 함
                tcs.SetResult(true);
            }
        }

        // 위에서 정의한 핸들러를 StateChanged 이벤트에 구독
        auth.StateChanged += AuthStateChangedHandler;

        // 기존의 동기 로그아웃 메서드를 호출하여 로그아웃 프로세스 시작
        SignOut();

        // TaskCompletionSource의 Task 반환
        // AuthStateChangedHandler가 tcs.SetResult()를 호출할 때까지 이 Task는 끝나지 않음
        return tcs.Task;
    }

    /// <summary>
    /// 현재 로그인된 익명 계정을 영구적인 이메일/비밀번호 계정으로 연동(업그레이드)
    /// </summary>
    public async Task<ELinkResult> LinkEmailToCurrentUserAsync(string email, string password)
    {
        // 반드시 익명 유저가 로그인한 상태여야 함
        if (user == null || !user.IsAnonymous)
        {
            Debug.LogError("No anonymous user is currently signed in to link.");
            return ELinkResult.NoAnonymousUser;
        }

        try
        {
            // 이메일/비밀번호로 자격 증명(Credential) 생성
            Credential credential = EmailAuthProvider.GetCredential(email, password);

            // 현재 익명 유저에게 새 자격 증명을 연결
            await user.LinkWithCredentialAsync(credential);

            Debug.Log("Anonymous account successfully upgraded to an Email account.");
            return ELinkResult.Success;
        }
        catch (FirebaseException e)
        {
            AuthError errorCode = (AuthError)e.ErrorCode;
            switch (errorCode)
            {
                case AuthError.EmailAlreadyInUse:
                    Debug.LogError("This email is already associated with another account.");
                    return ELinkResult.EmailAlreadyInUse;
                case AuthError.CredentialAlreadyInUse:
                    Debug.LogError("This credential is already linked to another user.");
                    return ELinkResult.CredentialAlreadyInUse;
                case AuthError.WeakPassword:
                    Debug.LogError("Password is too weak.");
                    return ELinkResult.WeakPassword;
                default:
                    Debug.LogError($"Account linking failed with Firebase exception: {e}");
                    return ELinkResult.UnknownError;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Account linking failed with exception: {e}");
            return ELinkResult.UnknownError;
        }
    }
    #endregion

    #region ANALYTICS
    /// <summary>
    /// Firebase Analytics 서비스 초기화
    /// </summary>
    private void InitAnalytics()
    {
        // 애널리틱스 데이터 수집 활성화
        FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
        bIsAnalyticsInit = true; // 초기화 완료 플래그 설정
        Debug.Log("Firebase Analytics initialized.");
    }

    /// <summary>
    /// 스테이지 시작 시 호출
    /// </summary>
    /// <param name="stageName">스테이지 이름 (예: "RuinsOfAFallenKing")</param>
    public void LogStageStart(string stageName)
    {
        if (!IsInit())
        {
            Debug.LogWarning("Firebase is not initialized. LogStageStart event was ignored.");
            return;
        }

        FirebaseAnalytics.LogEvent("stage_start", new Parameter("stage_name", stageName));
        Debug.Log($"Analytics: Stage Start logged - {stageName}");
    }

    /// <summary>
    /// 스테이지 결과(성공/실패) 기록
    /// </summary>
    /// <param name="stageName">스테이지 이름</param>
    /// <param name="isClear">클리어 성공 여부</param>
    /// <param name="playTime">플레이한 시간(초)</param>
    public void LogStageResult(string stageName, bool isClear, float playTime)
    {
        if (!IsInit())
        {
            Debug.LogWarning("Firebase is not initialized. LogStageResult event was ignored.");
            return;
        }

        string result = isClear ? "clear" : "fail";
        Parameter[] stageParams = {
            new Parameter("stage_name", stageName),
            new Parameter("result", result),
            new Parameter("play_time", playTime)
        };

        FirebaseAnalytics.LogEvent("stage_result", stageParams);
        Debug.Log($"Analytics: Stage Result logged - {stageName}, Result: {result}, Time: {playTime}");
    }

    /// <summary>
    /// 스킬 사용 기록
    /// </summary>
    /// <param name="skillName">스킬 이름 (예: "Fireball", "Heal")</param>
    public void LogSkillUsage(string skillName)
    {
        if (!IsInit())
        {
            Debug.LogWarning("Firebase is not initialized. LogSkillUsage event was ignored.");
            return;
        }

        FirebaseAnalytics.LogEvent("skill_use", new Parameter("skill_name", skillName));
        Debug.Log($"Analytics: Skill Usage logged - {skillName}");
    }
    #endregion

    #region FIRESTORE
    private void InitFirestore()
    {
        db = FirebaseFirestore.DefaultInstance;
        bIsFirestoreInit = true;
        Debug.Log("Firebase Firestore initialized.");
    }

    /// <summary>
    /// 특정 슬롯에 유저 데이터 저장
    /// </summary>
    /// <param name="slotIndex">저장할 슬롯 번호 (0, 1, 2)</param>
    /// <param name="data">저장할 UserData 객체</param>
    public async Task<bool> SaveUserDataAsync(int slotIndex, UserData data)
    {
        if (user == null)
        {
            Debug.LogError("User is not logged in. Cannot save data.");
            return false;
        }
        if (slotIndex < 0 || slotIndex > 2)
        {
            Debug.LogError("Invalid slot index.");
            return false;
        }

        // 데이터의 주소 생성: 실제 데이터를 보내기 전에 데이터가 저장될 정확한 위치(주소)를 지정
        // db.Collection("users"): users라는 최상위 컬렉션(폴더)을 선택
        // .Document(user.UserId): 그 안에서 현재 로그인한 유저의 고유 ID로 된 문서(파일)를 특정
        DocumentReference docRef = db.Collection("users").Document(user.UserId);

        // 데이터 직렬화
        // Firestore가 이해하는 범용 데이터 형식인 Dictionary<string, object>로 변환
        var userDataDict = data.ToDictionary();

        // "save_slots" 맵 안에 { "슬롯번호": 데이터 } 형태로 중첩된 Dictionary를 직접 구성
        var updates = new Dictionary<string, object>
        {
            {
                "save_slots", new Dictionary<string, object>
                {
                    { slotIndex.ToString(), userDataDict }
                }
            }
        };

        // 네트워크 통신
        try
        {
            // SetAsync와 MergeAll 옵션을 사용해 다른 슬롯은 건드리지 않고 해당 슬롯만 덮어씀
            // docRef.SetAsync: 앞서 지정한 주소(docRef)에 우리가 만든 데이터 묶음(updates)을 써달라고 요청
            // MergeAll: updates에 명시된 내용만 새로 추가하거나 덮어쓴다
            await docRef.SetAsync(updates, SetOptions.MergeAll);
            Debug.Log($"User data for slot {slotIndex} saved successfully.");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving user data: {e}");
            return false;
        }
    }

    /// <summary>
    /// 현재 로그인된 유저의 모든 슬롯 데이터를 불러옴
    /// </summary>
    /// <returns>슬롯 번호와 UserData를 담은 Dictionary. 데이터가 없으면 비어있는 Dictionary 반환.</returns>
    public async Task<Dictionary<int, UserData>> LoadAllUserDataAsync()
    {
        var allSlotsData = new Dictionary<int, UserData>();
        if (user == null)
        {
            Debug.LogError("User is not logged in. Cannot load data.");
            return allSlotsData;
        }

        // 읽어올 데이터의 정확한 주소 먼저 지정
        DocumentReference docRef = db.Collection("users").Document(user.UserId);
        try
        {
            // GetSnapshotAsync()를 호출하여 해당 주소의 데이터 복사본인 DocumentSnapshot을 비동기로 가져옴
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            if (snapshot.Exists) // 문서 존재 확인
            {
                // save_slots 맵 필드를 가져옴(필드 존재 확인)
                if (snapshot.TryGetValue($"save_slots", out object slotsObject))
                {
                    var slotsDict = slotsObject as Dictionary<string, object>;
                    foreach (var slot in slotsDict)
                    {
                        // 슬롯 데이터가 null이 아닐 경우에만 처리
                        // slot.Value가 우리가 예상하는 Dictionary 형태가 맞는지 한번 더 확인
                        if (slot.Value is Dictionary<string, object> slotDataDict)
                        {
                            // 데이터 역직렬화
                            int slotIndex = int.Parse(slot.Key);
                            allSlotsData[slotIndex] = new UserData(slotDataDict);
                        }
                    }
                }
            }
            else
            {
                Debug.Log("No user data found for this user. A new document will be created upon saving.");
            }

            return allSlotsData;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading user data: {e}");
            return new Dictionary<int, UserData>(); // 에러 발생 시 비어있는 딕셔너리 반환
        }
    }
    #endregion
}