using UnityEngine;

public class PlayerHouse : InteractableObject
{
    [SerializeField] private TutorialDataSO playerHouseTutorial;
    private const string FirstInteractionKey = "PlayerHouse_FirstInteraction";

    public override void Interact()
    {
        base.Interact();

        // PlayerPrefs에 저장된 값이 0이면(또는 키가 없으면) 첫 상호작용으로 판단
        if (PlayerPrefs.GetInt(FirstInteractionKey, 0) == 0)
        {
            // 키 값을 1로 설정하여 다음부터 이 코드 실행 안됨
            PlayerPrefs.SetInt(FirstInteractionKey, 1);
            PlayerPrefs.Save(); // 변경사항 즉시 저장

            var tutoPopup = UIManager.Instance.Show<TutorialPopupUI>();
            tutoPopup.Setup(playerHouseTutorial, () =>
            {
                UIManager.Instance.Show<SkillSelectUI>();
            });
        }
        else
        {
            UIManager.Instance.Show<SkillSelectUI>();
        }
    }
}
