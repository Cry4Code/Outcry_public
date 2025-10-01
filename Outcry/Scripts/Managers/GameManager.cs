using StageEnums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

// 게임 전체의 상태를 나타내는 열거형
public enum EGameState
{
    Initializing, // 초기화 중
    MainMenu,     // 메인 메뉴 (로그인 전)
    Lobby,        // 로비 (캐릭터/보스 선택)
    InGame,       // 게임 플레이 중 (보스 전투)
    LoadingScene  // 씬 로딩 중
}

public class GameManager : Singleton<GameManager>, IStageDataProvider
{
    public EGameState CurrentGameState { get; private set; }
    public UserData CurrentUserData { get; private set; }
    public SceneLoadPackage NextLoadPackage { get; private set; }

    private StageData currentStageData;
    private EnemyData currentEnemyData;

    private int currentSlotIndex = -1; // 현재 플레이 중인 슬롯 번호 저장

    public event Action<int, UserData> OnUserDataSaved;

    protected override void Awake()
    {
        base.Awake();

        InitializeCoreSystems();
    }

    private void Start()
    {
        // StageManager가 보스 처치 시 발생시키는 이벤트를 구독
        StageManager.OnStageCleared += HandleStageCleared;
    }

    private void OnDisable()
    {
        StageManager.OnStageCleared -= HandleStageCleared;
    }

    // 게임 시작 시 단 한번만 실행되어야 하는 초기화 로직
    private void InitializeCoreSystems()
    {
        CurrentGameState = EGameState.Initializing;
        // Application.targetFrameRate = 60;

        // TODO: ResourceManager, AudioManager 등 다른 핵심 시스템 초기화 호출
        DataTableManager.Instance.LoadCollectionData<StageDataTable>();
        DataTableManager.Instance.LoadCollectionData<SoundDataTable>();
        DataTableManager.Instance.LoadCollectionData<EnemyDataTable>();

        Debug.Log("GameManager Initialized.");
    }

    public StageData GetStageData()
    {
        return currentStageData;
    }

    public EnemyData GetEnemyData(int id)
    {
        if (currentEnemyData != null && currentEnemyData.ID == id)
        {
            return currentEnemyData;
        }

        return DataTableManager.Instance.GetCollectionDataById<EnemyData>(id);
    }

    // 여러 몬스터 데이터를 가져오는 새로운 메서드
    public List<EnemyData> GetCurrentStageEnemyData()
    {
        if (currentStageData == null)
        {
            return new List<EnemyData>();
        }

        var enemyDataList = new List<EnemyData>();
        foreach (var id in currentStageData.Monster_ids)
        {
            var data = DataTableManager.Instance.GetCollectionDataById<EnemyData>(id);
            if (data != null)
            {
                enemyDataList.Add(data);
            }
        }
        return enemyDataList;
    }

    #region 유저 데이터 관리
    /// <summary>
    /// 슬롯 UI에서 빈 슬롯 선택 시 호출
    /// </summary>
    public void PrepareNewGame(int slotIndex)
    {
        currentSlotIndex = slotIndex;
        Debug.Log($"{slotIndex}번 슬롯에서 새 게임을 시작합니다.");

        UIManager.Instance.Show<NicknameUI>();
    }

    public void CreateNewGame(string nickname)
    {
        CurrentUserData = new UserData(nickname);

        // 새 데이터를 바로 Firestore에 저장
        SaveGame();

        StartStage((int)EStageType.Tutorial);
    }

    /// <summary>
    /// 슬롯 UI에서 저장할 슬롯 선택 시 호출
    /// </summary>
    public void SaveGameToSlot(int slotIndex)
    {
        currentSlotIndex = slotIndex;
        SaveGame();
    }

    // 현재 유저 데이터를 저장하는 기능
    public void SaveGame()
    {
        _ = SaveGameAsync();
    }

    public async Task<bool> SaveGameAsync()
    {
        if (CurrentUserData == null || currentSlotIndex == -1)
        {
            Debug.LogWarning("Cannot save game. No user data or slot selected.");
            return false;
        }

        Debug.Log($"게임을 {currentSlotIndex}번 슬롯에 저장합니다...");
        bool success = await FirebaseManager.Instance.SaveUserDataAsync(currentSlotIndex, CurrentUserData);
        if (success)
        {
            Debug.Log("Save successful.");
        }
        else
        {
            Debug.LogError("Save failed.");
        }

        return success;
    }

    public void LoadGame(int slotIndex, UserData data)
    {
        CurrentUserData = data;
        currentSlotIndex = slotIndex;

        if(CurrentUserData.IsTutorialCleared)
        {
            GoToLobby();
        }
        else
        {
            StartStage((int)EStageType.Tutorial);
        }
    }

    private IEnumerator SaveGameCoroutine()
    {
        var saveTask = SaveGameAsync();
        // Task가 완료될 때까지 코루틴에서 대기
        yield return new WaitUntil(() => saveTask.IsCompleted);

        if (saveTask.IsFaulted)
        {
            Debug.LogError("로딩 중 저장 실패!");
            // TODO: 저장 실패 시 예외 처리(재시도 UI 표시)?
        }
    }
    #endregion

    #region 스테이지 관리
    // 로비(거점) 씬으로 이동
    public void GoToLobby()
    {
        CurrentGameState = EGameState.LoadingScene;

        // 로비 이동을 위한 간단한 명세서 생성(미리 로드할 리소스 없음)
        var package = new SceneLoadPackage(ESceneType.LobbyScene);

        // 유저 데이터 저장
        if (CurrentUserData != null && currentSlotIndex != -1)
        {
            package.PreLoadingTasks.Add(new LoadingTask
            {
                Description = "Saving player data...", // 로딩 UI에 표시될 텍스트
                Coroutine = SaveGameCoroutine           // 실행할 코루틴 메서드 연결
            });
        }

        // 생성된 명세서 저장
        NextLoadPackage = package;

        // LoadingScene 로드
        SceneLoadManager.Instance.LoadScene(ESceneType.LoadingScene);
    }

    /// <summary>
    /// 로비에서 보스 선택 시 호출.
    /// stageId를 StageData로 변환하여 전투 씬 로드
    /// </summary>
    public void StartStage(int stageId)
    {
        currentStageData = DataTableManager.Instance.GetCollectionDataById<StageData>(stageId);
        if (currentStageData == null)
        {
            Debug.LogError($"ID: {stageId}에 해당하는 StageData를 찾을 수 없습니다. 로비로 돌아갑니다.");
            GoToLobby();
            return;
        }

        CurrentGameState = EGameState.LoadingScene;

        // 스테이지 시작 이벤트 로깅
        FirebaseManager.Instance.LogStageStart(currentStageData.Stage_name);

        // 스테이지 시작을 위한 데이터 설정
        var package = new SceneLoadPackage(ESceneType.StageScene);
        package.AdditiveSceneNames.Add("StageManagers");
        
        if (!string.IsNullOrEmpty(currentStageData.Map_path))
        {
            package.ResourceAddressesToLoad.Add(currentStageData.Map_path);
        }

        foreach (var monsterId in currentStageData.Monster_ids)
        {
            var enemyData = GetEnemyData(monsterId);
            if (enemyData != null && !string.IsNullOrEmpty(enemyData.Enemy_path))
            {
                package.ResourceAddressesToLoad.Add(enemyData.Enemy_path);
            }
            else
            {
                Debug.LogWarning($"ID: {monsterId}에 해당하는 EnemyData 또는 리소스 경로가 없습니다.");
            }
        }

        package.ResourceAddressesToLoad.Add(Paths.Prefabs.Player);

        // 스테이지 데이터 저장
        NextLoadPackage = package;

        // 로딩 씬으로 이동
        SceneLoadManager.Instance.LoadScene(ESceneType.LoadingScene);
    }

    // StageManager가 보스 처치 이벤트를 발생시키면 실행
    private void HandleStageCleared(StageData clearedStage)
    {
        if (CurrentUserData == null)
        {
            return;
        }

        Debug.Log($"스테이지 ID: {clearedStage.ID} 클리어! 유저 데이터 업데이트 및 저장.");

        // 스테이지에 포함된 모든 몬스터를 클리어 목록에 추가
        foreach (var bossId in clearedStage.Monster_ids)
        {
            // TODO: 클리어 보스 아이디 중복 방지 처리가 필요할까?
            if (!CurrentUserData.ClearedBossIds.Contains(bossId))
            {
                CurrentUserData.ClearedBossIds.Add(bossId);
            }
        }

        // 튜토리얼 클리어 처리
        if (clearedStage.ID == (int)EStageType.Tutorial && !CurrentUserData.IsTutorialCleared)
        {
            CurrentUserData.IsTutorialCleared = true;
        }

        // 변경된 데이터 저장
        SaveGame();
    }
    #endregion
}
