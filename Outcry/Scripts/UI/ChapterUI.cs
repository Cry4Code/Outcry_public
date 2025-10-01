using StageEnums;
using UnityEngine;
using UnityEngine.UI;

public class ChapterUI : UIBase
{
    [SerializeField] private Button goblinKingStageBtn;
    [SerializeField] private Button exitBtn;

    private void Start()
    {
        goblinKingStageBtn.onClick.AddListener(OnGoblinKingStageClicked);
        exitBtn.onClick.AddListener(OnExitButtonClicked);
    }

    private void OnGoblinKingStageClicked()
    {
        GameManager.Instance.StartStage((int)EStageType.RuinsOfTheFallenKing);
    }

    private void OnExitButtonClicked()
    {
        UIManager.Instance.Hide<ChapterUI>();
    }
}
