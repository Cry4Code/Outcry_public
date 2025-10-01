using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : UIBase
{
    [SerializeField] private Button optionBtn;
    [SerializeField] private Button skillSelectBtn;
    [SerializeField] private Button chapterBtn;
    [SerializeField] private Button storeBtn;
    [SerializeField] private Button saveLoadBtn;

    private void Awake()
    {
        optionBtn.onClick.AddListener(OnClickOption);
        skillSelectBtn.onClick.AddListener(OnClickSkillSelect);
        chapterBtn.onClick.AddListener(OnClickChapter);
        storeBtn.onClick.AddListener(OnClickStore);
        saveLoadBtn.onClick.AddListener(OnClickSaveLoad);
    }

    private void OnClickOption()
    {
        Debug.Log("Option Clicked");
        UIManager.Instance.Show<OptionUI>();
    }

    private void OnClickSkillSelect()
    {
        Debug.Log("Skill Select Clicked");
        // TODO: SkillSelectUI 구현 후 연결
    }

    private void OnClickChapter()
    {
        Debug.Log("Chapter Clicked");
        UIManager.Instance.Show<ChapterUI>();
    }

    private void OnClickStore()
    {
        Debug.Log("Store Clicked");
        UIManager.Instance.Show<StoreUI>();
    }

    private void OnClickSaveLoad()
    {
        Debug.Log("Save/Load Clicked");
        
        SaveLoadManager.Instance.OpenUI(ESlotUIType.Save);
    }
}
