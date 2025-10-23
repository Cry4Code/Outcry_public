using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum ESlotUIType
{
    Load,
    Save
}

// SaveLoadUI에 전달할 데이터 클래스
public class SaveLoadUIData
{
    // 나가기 버튼 클릭 시 실행될 동작을 담는 Action
    public Action OnClickExitAction { get; set; }
}

public class SaveLoadManager : Singleton<SaveLoadManager>
{
    // UI에게 슬롯 데이터 로딩이 완료되었음을 알리는 이벤트
    public event Action<ESlotUIType, Dictionary<int, UserData>> OnSlotsDataUpdated;

    private Dictionary<int, UserData> slotsData;
    private ESlotUIType currentType;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnUserDataSaved += HandleGameDataSaved;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnUserDataSaved -= HandleGameDataSaved;
        }
    }

    public async UniTask LoadAllUserData()
    {
        // Firebase에서 유저 데이터 로드
        slotsData = await UGSManager.Instance.LoadAllUserDataAsync();
    }

    private void HandleGameDataSaved(int slotIndex, UserData savedData)
    {
        // 로컬 슬롯 데이터 갱신
        if (slotsData != null)
        {
            slotsData[slotIndex] = savedData;
        }

        // 현재 Save/Load UI가 활성화되어 있다면 UI 갱신 요청
        // (UI가 꺼져있을 때 불필요한 업데이트를 방지)
        if (UIManager.Instance.IsUIActive<SaveLoadUI>())
        {
            OnSlotsDataUpdated?.Invoke(currentType, slotsData);
        }
    }

    /// <summary>
    /// 저장/로드 UI를 여는 유일한 진입점
    /// </summary>
    public void OpenUI(ESlotUIType type, SaveLoadUIData data = null)
    {
        currentType = type;

        var saveLoadPopup = UIManager.Instance.Show<SaveLoadUI>();

        // 전달받은 데이터가 있다면, Setup 메서드를 호출하여 UI에 전달합니다.
        if (data != null)
        {
            saveLoadPopup.Setup(data);
        }

        // 데이터 로딩 완료되면 이벤트를 발생시켜 UI에게 데이터 전달
        OnSlotsDataUpdated?.Invoke(currentType, slotsData);
    }

    /// <summary>
    /// UI로부터 슬롯 선택 이벤트를 받아서 처리
    /// </summary>
    public void SelectSlot(int slotIndex)
    {
        Debug.Log($"Slot {slotIndex} selected in {currentType} mode by Manager.");

        EffectManager.Instance.ButtonSound();

        switch (currentType)
        {
            case ESlotUIType.Load:
                if (slotsData.TryGetValue(slotIndex, out UserData dataToLoad))
                {
                    GameManager.Instance.LoadGame(slotIndex, dataToLoad);
                }
                else
                {
                    GameManager.Instance.PrepareNewGame(slotIndex);
                }
                break;

            case ESlotUIType.Save:
                GameManager.Instance.SaveGameToSlot(slotIndex);
                break;
        }
    }

    /// <summary>
    /// UI로부터 슬롯 데이터 삭제 이벤트를 받아서 처리
    /// </summary>
    public void DeleteSlotData(int slotIndex)
    {
        Debug.Log($"Deletion requested for slot {slotIndex}.");
        EffectManager.Instance.ButtonSound();

        // 삭제 여부를 확인 팝업
        ConfirmUI popup = UIManager.Instance.Show<ConfirmUI>();
        popup.Setup(new ConfirmPopupData
        {
            Title = "Warning",
            Message = "Are you sure you want to delete this?",
            OkButtonText = "Delete",
            Type = EConfirmPopupType.OK_CANCEL,

            // 삭제 버튼을 눌렀을 때 실행될 로직
            OnClickOK = async () =>
            {
                // UGS 서비스를 통해 클라우드 데이터 삭제를 시도하고 결과 기다림
                bool success = await UGSManager.Instance.DeleteUserDataAsync(slotIndex);

                // 삭제 결과에 따라 후속 처리
                if (success)
                {
                    // 성공: 로컬에 캐싱된 데이터에서도 해당 슬롯 정보 제거
                    if (slotsData != null && slotsData.ContainsKey(slotIndex))
                    {
                        slotsData.Remove(slotIndex);
                    }

                    // 성공: 데이터가 변경되었음을 UI에 알려 화면 갱신
                    OnSlotsDataUpdated?.Invoke(currentType, slotsData);

                    Debug.Log($"Slot {slotIndex} data deleted successfully.");
                }
                else
                {
                    // 실패: 사용자에게 삭제 실패를 알리는 팝업을 띄움
                    Debug.LogError($"Failed to delete data for slot {slotIndex}.");

                    var failPopup = UIManager.Instance.Show<ConfirmUI>();
                    failPopup.Setup(new ConfirmPopupData
                    {
                        Title = "Error",
                        Message = "Failed to delete.\nPlease check your network connection and try again.",
                        Type = EConfirmPopupType.OK // 확인 버튼만 있는 팝업
                    });
                }
            }
        });
    }
}
