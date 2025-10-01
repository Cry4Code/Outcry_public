using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum ESlotUIType
{
    Load,
    Save
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

    public async Task LoadAllUserData()
    {
        // Firebase에서 유저 데이터 로드
        slotsData = await FirebaseManager.Instance.LoadAllUserDataAsync();
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
    public void OpenUI(ESlotUIType type)
    {
        currentType = type;

        UIManager.Instance.Show<SaveLoadUI>();

        // 데이터 로딩 완료되면 이벤트를 발생시켜 UI에게 데이터 전달
        OnSlotsDataUpdated?.Invoke(currentType, slotsData);
    }

    /// <summary>
    /// UI로부터 슬롯 선택 이벤트를 받아서 처리
    /// </summary>
    public void SelectSlot(int slotIndex)
    {
        Debug.Log($"Slot {slotIndex} selected in {currentType} mode by Manager.");

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
}
