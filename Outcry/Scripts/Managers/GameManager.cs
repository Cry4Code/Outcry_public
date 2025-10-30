using Cysharp.Threading.Tasks;
using StageEnums;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization.Settings;

// 게임 전체의 상태를 나타내는 열거형
public enum EGameState
{
    Initializing, // 초기화 중
    MainMenu,     // 메인 메뉴 (로그인 전)
    Lobby,        // 로비 (캐릭터/보스 선택)
    Battle,       // 게임 플레이 중 (보스 전투)
    LoadingScene  // 씬 로딩 중
}

public class GameManager : Singleton<GameManager>, IStageDataProvider
{
    public EGameState CurrentGameState { get; set; }
    public UserData CurrentUserData { get; private set; }
    public SceneLoadPackage NextLoadPackage { get; private set; }
    public bool HasEnteredDungeon { get; set; } = false;

    private StageData currentStageData;
    private EnemyData currentEnemyData;

    private int currentSlotIndex = -1; // 현재 플레이 중인 슬롯 번호 저장

    public event Action<int, UserData> OnUserDataSaved;
    // <int soulId, int newCount>
    public event Action<int, int> OnSoulCountChanged;

    // 이전 리소스 목록 임시 저장
    private List<string> assetsToUnloadFromPreviousScene = new List<string>();

    private Dictionary<int, Sprite> spriteDict = new Dictionary<int, Sprite>();

    protected override void Awake()
    {
        base.Awake();

        InitializeCoreSystems();
    }

    private async void Start()
    {
        // StageManager가 보스 처치 시 발생시키는 이벤트를 구독
        StageManager.OnStageCleared += HandleStageCleared;

        await LocalizationSettings.InitializationOperation.Task.AsUniTask();
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

        DataTableManager.Instance.LoadCollectionData<StageDataTable>();
        DataTableManager.Instance.LoadCollectionData<SoundDataTable>();
        DataTableManager.Instance.LoadCollectionData<EnemyDataTable>();
        DataTableManager.Instance.LoadCollectionData<AchievementsDataTable>();

        LoadAllSpritesAsync().Forget();

        Debug.Log("GameManager Initialized.");

        CurrentGameState = EGameState.MainMenu;
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

    public async UniTask StartNewGameAsaGuest()
    {
        await CreateNewGame("Guest");
    }

    public async UniTask CreateNewGame(string nickname)
    {
        CurrentUserData = new UserData(nickname);

        // TODO: 유저 테스트용으로 모든 보스 소울 획득
        Debug.Log("<color=yellow>[TEST] 모든 보스 소울을 획득합니다.</color>");
        var allStageData = DataTableManager.Instance.CollectionData[typeof(StageData)] as Dictionary<int, IData>;
        if (allStageData != null)
        {
            foreach (StageData stage in allStageData.Values)
            {
                // 튜토리얼과 마을을 제외한 모든 스테이지의 보스 소울을 2개씩 획득
                if (stage.ID != (int)EStageType.Tutorial && stage.ID != (int)EStageType.Village)
                {
                    GainSouls(stage.Boss_Soul, 2);
                }
            }
        }

        bool nameUpdateSuccess = await UGSManager.Instance.UpdatePlayerDisplayNameAsync(nickname);
        if (nameUpdateSuccess)
        {
            // 이름 업데이트 성공 후 UGS로부터 태그가 포함된 최종 이름을 가져옴
            CurrentUserData.UniquePlayerName = await UGSManager.Instance.GetPlayerDisplayNameAsync();
            Debug.Log($"고유 이름 저장 완료: {CurrentUserData.UniquePlayerName}");
        }
        else
        {
            // 실패 시에는 최소한 입력한 닉네임이라도 사용
            CurrentUserData.UniquePlayerName = nickname;
            Debug.LogError("UGS 이름 업데이트 실패. HUD에 임시 닉네임 표시.");
        }

        // 새 데이터를 바로 UGS에 저장
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

    public async UniTask<bool> SaveGameAsync()
    {
        if (CurrentUserData == null || currentSlotIndex == -1)
        {
            Debug.LogWarning("Cannot save game. No user data or slot selected.");
            return false;
        }

        Debug.Log($"게임을 {currentSlotIndex}번 슬롯에 저장합니다...");
        bool success = await UGSManager.Instance.SaveUserDataAsync(currentSlotIndex, CurrentUserData);
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

        if (CurrentUserData.IsTutorialCleared)
        {
            GoToLobby();
        }
        else
        {
            StartStage((int)EStageType.Tutorial);
        }
    }
    #endregion

    #region 스테이지 관리
    // 로비(거점) 씬으로 이동
    public void GoToLobby()
    {
        StartStage((int)EStageType.Village);
    }

    /// <summary>
    /// 이전 스테이지에서 사용했던 리소스들을 안전하게 언로드하는 코루틴
    /// 이 코루틴은 LoadingScene이 활성화된 후에 실행된다.
    /// </summary>
    private IEnumerator UnloadPreviousStageAssetsCoroutine(List<string> assetsToUnload)
    {
        // 이전 SceneLoadManager가 언로드하므로 여기서는 씬 언로드가 필요 없다.
        // 오직 에셋만 언로드

        // 이전 스테이지의 로드 패키지에서 언로드할 리소스 목록을 가져옴
        // 단, NextLoadPackage는 이미 새로운 로비 패키지로 교체되었으므로
        // 이전 리소스 목록을 다른 곳에 임시 저장해야 한다.(StartStage에서 처리)
        if (assetsToUnload != null && assetsToUnload.Count > 0)
        {
            yield return ResourceManager.Instance.UnloadAllAssetsCoroutine(assetsToUnload);
            assetsToUnload.Clear(); // 정리 후 비워줌
        }
        yield break;
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

        // 스테이지 시작 시 로비, 튜토리얼이 아닌 다른 스테이지(던전)에 입장하는 것이라면 플래그 true 설정
        if (stageId != (int)EStageType.Village && stageId != (int)EStageType.Tutorial)
        {
            HasEnteredDungeon = true;
            Debug.Log("던전에 입장했습니다. HasEnteredDungeon 플래그를 true로 설정합니다.");
        }

        CurrentGameState = EGameState.LoadingScene;
        Time.timeScale = 1f; // 일시정지 상태였다면 풀어줌

        // 스테이지 시작 이벤트 로깅 (마을 아닐 때만)
        if(currentStageData.Stage_id != StageID.Village)
            UGSManager.Instance.LogStageStart(currentStageData.Stage_id);

        // 스테이지 시작을 위한 데이터 설정
        var package = new SceneLoadPackage(ESceneType.InGameScene);
        //package.AdditiveSceneNames.Add("StageManagers");

        // 가장 먼저 이전 리소스 언로드 작업 PreLoadingTask에 추가
        // 이전 스테이지에서 로드한 리소스가 있을 경우에만 언로드 작업을 추가합니다.
        List<string> assetsForUnloading;

        if (assetsToUnloadFromPreviousScene != null)
        {
            assetsForUnloading = new List<string>(assetsToUnloadFromPreviousScene);
        }
        else
        {
            assetsForUnloading = new List<string>();
        }

        if (assetsForUnloading.Count > 0)
        {
            package.PreLoadingTasks.Add(new LoadingTask
            {
                // Description = "Cleaning up previous stage...",
                Description = LocalizationUtility.GetLocalizedValueByKey(LocalizationStrings.Loading.CLEANINGUP),
                Coroutine = () => UnloadPreviousStageAssetsCoroutine(assetsForUnloading)
            });
        }

        // 현재 유저 데이터 저장 작업 추가(로비로 돌아갈 때만 저장)
        if (stageId == (int)EStageType.Village && CurrentUserData != null && currentSlotIndex != -1)
        {
            package.PreLoadingTasks.Add(new LoadingTask
            {
                // Description = "Saving game data...",
                Description = LocalizationUtility.GetLocalizedValueByKey(LocalizationStrings.Loading.SAVING),
                Coroutine = () => SaveGameAsync().ToCoroutine()
            });
        }

        // PlayerSFX 효과음 프리 로드
        package.PreLoadingTasks.Add(new LoadingTask
        {
            // Description = "Loading sound effects...", // 로딩 UI에 표시될 텍스트
            Description = LocalizationUtility.GetLocalizedValueByKey(LocalizationStrings.Loading.SOUNDS),
            Coroutine = () => LoadPlayerSFXAsync().ToCoroutine() // 실행할 코루틴 메서드 연결
        });

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

        package.ResourceAddressesToLoad.Add(Paths.Prefabs.Cursor);
        package.ResourceAddressesToLoad.Add(Paths.Prefabs.Player);

        // 새로운 스테이지를 시작하기 전에 다음에 언로드해야 할 리소스 목록 저장
        assetsToUnloadFromPreviousScene = new List<string>(package.ResourceAddressesToLoad);

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

        // 보스 킬 업적(튜토리얼이 아닐 때만 보스 킬 인정)
        if (clearedStage.ID != (int)EStageType.Tutorial)
        {
            AchievementManager.Instance.ReportEvent(EMissionType.BossKillAchieve);
        }

        // NoHit 업적 확인
        var player = PlayerManager.Instance.player;
        if (player != null && !player.Condition.WasHitThisStage)
        {
            AchievementManager.Instance.ReportEvent(EMissionType.NoHit, clearedStage.Stage_id);
        }

        // 튜토리얼 클리어 처리
        if (clearedStage.ID == (int)EStageType.Tutorial && !CurrentUserData.IsTutorialCleared)
        {
            CurrentUserData.IsTutorialCleared = true;

            AchievementManager.Instance.ReportEvent(EMissionType.CleardTutorial);
        }

        // 스테이지에 포함된 모든 몬스터를 클리어 목록에 추가
        foreach (var bossId in clearedStage.Monster_ids)
        {
            // TODO: 클리어 보스 아이디 중복 방지 처리가 필요할까?
            if (!CurrentUserData.ClearedBossIds.Contains(bossId))
            {
                CurrentUserData.ClearedBossIds.Add(bossId);
            }
        }

        // TODO: 스테이지 클리어 보상 지급(소울)
        // (보상 중복 보유 가능 여부에 따라 달라질 예정)
        GainSouls(clearedStage.Boss_Soul, 1);

        // 변경된 데이터 저장
        SaveGame();
    }
    #endregion

    #region 게임 플레이 관련
    /// <summary>
    /// 플레이어의 인벤토리에 소울을 추가하거나 개수를 늘립니다.
    /// </summary>
    /// <param name="soulId">추가할 소울의 고유 ID</param>
    /// <param name="amount">추가할 개수</param>
    public void GainSouls(int soulId, int amount)
    {
        if (CurrentUserData == null)
        {
            return;
        }

        if (soulId <= 0)
        {
            return;
        }

        // FindIndex는 조건에 맞는 항목의 인덱스를 찾고 없으면 -1 반환
        int index = CurrentUserData.AcquiredSouls.FindIndex(s => s.SoulId == soulId);

        if (index != -1) // 인벤토리에 이미 해당 소울이 존재할 경우
        {
            // struct는 값 타입(Value Type)이므로, 리스트에서 직접 수정하려면
            // 복사본을 만들고 값을 변경한 뒤, 다시 리스트의 해당 위치에 덮어써야 합니다.
            UserSoulData existingSoul = CurrentUserData.AcquiredSouls[index];
            existingSoul.Count += amount;
            CurrentUserData.AcquiredSouls[index] = existingSoul;

            Debug.Log($"소울 갱신: ID {soulId}, 총 개수: {existingSoul.Count}");
            OnSoulCountChanged?.Invoke(soulId, existingSoul.Count);
        }
        else // 새로 획득한 소울일 경우
        {
            var newSoul = new UserSoulData { SoulId = soulId, Count = amount };
            CurrentUserData.AcquiredSouls.Add(newSoul);
            Debug.Log($"새로운 소울 획득: ID {soulId}, 개수: {amount}");
            OnSoulCountChanged?.Invoke(soulId, newSoul.Count);
        }

        AchievementManager.Instance.ReportEvent(EMissionType.GetSoul);
    }

    /// <summary>
    /// 지정된 양의 소울을 소모하려고 시도
    /// </summary>
    /// <param name="soulId">소모할 소울의 ID</param>
    /// <param name="amount">소모할 개수</param>
    /// <returns>소모에 성공하면 true, 소울이 부족하면 false 반환</returns>
    public bool TrySpendSouls(int soulId, int amount)
    {
        if (CurrentUserData == null) return false;

        // 소모하려는 소울이 인벤토리에 있는지 확인
        int index = CurrentUserData.AcquiredSouls.FindIndex(s => s.SoulId == soulId);

        if (index == -1) // 해당 소울을 가지고 있지 않은 경우
        {
            Debug.LogWarning($"소울 소모 실패: ID {soulId} 소울을 가지고 있지 않습니다.");
            return false;
        }

        // 소울 개수가 충분한지 확인
        if (CurrentUserData.AcquiredSouls[index].Count < amount)
        {
            Debug.LogWarning($"소울 소모 실패: ID {soulId} 소울이 부족합니다. (필요: {amount}, 보유: {CurrentUserData.AcquiredSouls[index].Count})");
            return false;
        }

        // 소울 개수가 충분하면 개수 차감
        UserSoulData soulToSpend = CurrentUserData.AcquiredSouls[index];
        soulToSpend.Count -= amount;
        CurrentUserData.AcquiredSouls[index] = soulToSpend;

        Debug.Log($"소울 소모 성공: ID {soulId}, {amount}개 사용. 남은 개수: {soulToSpend.Count}");
        OnSoulCountChanged?.Invoke(soulId, soulToSpend.Count);

        // 소모 성공
        return true;
    }

    /// <summary>
    /// 플레이어 데이터에 스킬을 추가하려고 시도
    /// </summary>
    /// <param name="skillId">추가할 스킬의 ID</param>
    public void GainSkill(int skillId)
    {
        // 유저데이터가 없는 경우(로그인/초기화 안됨) 그냥 종료
        if (CurrentUserData == null)
        {
            return;
        }

        // AcquiredSkillIds 리스트가 null이면 새로 생성
        if (CurrentUserData.AcquiredSkillIds == null)
        {
            Debug.LogError("[GameManager.GainSkill] AcquiredSkillIds is null. Initializing new list.");
            return;
        }

        // 이미 같은 스킬을 가지고 있으면 중복 추가를 막고 종료
        if (CurrentUserData.AcquiredSkillIds.Contains(skillId))
        {
            return;
        }

        //리스트에 skillId 기록
        CurrentUserData.AcquiredSkillIds.Add(skillId);

        AchievementManager.Instance.ReportEvent(EMissionType.GetSkill);

        SaveGame();
    }

    /// <summary>
    /// (임시) 현재 장착된 스킬 ID. 
    /// </summary>
    private int equippedSkillId = 0;

    /// <summary>
    /// UI 등에서 장착 성공 시 알림을 받고 싶을 때 구독
    /// </summary>
    public event Action<int> OnSkillEquipped;

    /// <summary>
    /// 스킬 장착 시도: 유저가 해당 스킬을 보유하고 있어야 함
    /// </summary>
    /// <param name="skillId">장착할 스킬 ID</param>
    /// <returns>장착 성공 여부</returns>
    public bool TryEquipSkill(int skillId)
    {
        if (CurrentUserData == null)
        {
            Debug.LogWarning("[GameManager.TryEquipSkill] CurrentUserData is null.");
            return false;
        }

        // 보유 스킬 목록 검사
        if (CurrentUserData.AcquiredSkillIds == null ||
            !CurrentUserData.AcquiredSkillIds.Contains(skillId))
        {
            Debug.LogWarning($"[GameManager.TryEquipSkill] 보유하지 않은 스킬은 장착할 수 없습니다. (skillId: {skillId})");
            return false;
        }

        // 이미 같은 스킬 장착 중이면 조용히 성공 반환 (중복 이벤트 방지)
        if (equippedSkillId == skillId)
        {
            return true;
        }

        // 한 개만 장착 가능 → 단순히 교체
        equippedSkillId = skillId;
        CurrentUserData.SelectSkillId = skillId;
        Debug.Log($"[GameManager.TryEquipSkill]유저데이터에 스킬 장착 성공: {skillId}");
        PlayerManager.Instance.player.Skill.SetSkill(skillId);
        Debug.Log($"[GameManager.TryEquipSkill]플레이어에 스킬 장착 성공: {skillId}");

        OnSkillEquipped?.Invoke(skillId);

        EventBus.Publish("SkillEquipped", skillId);

        SaveGame();

        return true;
    }
    #endregion

    #region Sprites 관리
    /// <summary>
    /// ResourceManager를 통해 Sprites 레이블을 가진 모든 스프라이트를 비동기 로드
    /// </summary>
    private async UniTask LoadAllSpritesAsync()
    {
        // ResourceManager에게 Sprites 레이블을 가진 모든 Sprite 에셋을 로드해달라고 요청
        var loadedSprites = await ResourceManager.Instance.LoadAssetsByLabelAsync<Sprite>("Sprites");

        if (loadedSprites == null || loadedSprites.Count == 0)
        {
            Debug.LogWarning("[GameManager] 'SoulSprites' 레이블로 로드된 스프라이트가 없습니다.");
            return;
        }

        // 로드된 스프라이트들 순회
        foreach (var sprite in loadedSprites)
        {
            // 스프라이트의 이름(Addressable 주소)을 ID로 변환
            if (int.TryParse(sprite.name, out int id))
            {
                // 조회용 Dictionary에 ID와 스프라이트 추가
                if (!spriteDict.ContainsKey(id))
                {
                    spriteDict.Add(id, sprite);
                }
            }
            else
            {
                Debug.LogWarning($"[GameManager] 스프라이트 이름({sprite.name})을 ID로 변환할 수 없습니다.");
            }
        }

        Debug.Log($"[GameManager] {spriteDict.Count}개의 스프라이트 준비 완료.");
    }

    /// <summary>
    /// ID로 미리 로드된 스프라이트 즉시 반환
    /// </summary>
    public Sprite GetSprite(int id)
    {
        if (spriteDict.TryGetValue(id, out Sprite sprite))
        {
            return sprite;
        }

        Debug.LogWarning($"[DataManager] ID: {id}에 해당하는 스프라이트를 찾을 수 없습니다. 미리 로드되었는지 확인해주세요.");
        return null;
    }
    #endregion

    /// <summary>
    /// PlayerSFX 레이블을 가진 모든 오디오 클립을 로드하는 코루틴
    /// </summary>
    private async UniTask LoadPlayerSFXAsync()
    {
        try
        {
            var loadedClips = await ResourceManager.Instance.LoadAssetsByLabelAsync<AudioClip>("PlayerSFX");
            Debug.Log($"플레이어 효과음 {loadedClips.Count}개 로딩 완료.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"플레이어 효과음(PlayerSFX) 로딩에 실패했습니다! 에러: {ex.Message}");
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
