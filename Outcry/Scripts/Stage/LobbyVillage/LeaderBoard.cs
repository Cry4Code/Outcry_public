public class LeaderBoard : InteractableObject
{
    public override void Interact()
    {
        base.Interact();

#if UNITY_WEBGL  && !UNITY_EDITOR
        var warningPopup = UIManager.Instance.Show<ConfirmUI>();
        warningPopup.Setup(new ConfirmPopupData
        {
            // Title = "Warning",
            // Message = "Leaderboard registration is not supported in the WebGL version.",
            Title = LocalizationUtility.GetLocalizedValueByKey(LocalizationStrings.UI.WARNING),
            Message = LocalizationUtility.ChooseLocalizedString(
                "Leaderboard registration is not supported in the WebGL version.",
                "WebGL 버전에서는 리더보드 등록을 지원하지 않습니다."),
            Type = EConfirmPopupType.OK,
            OnClickOK = () =>
            {
                UIManager.Instance.Show<LeaderboardUI>();
            }
        });
#else
        UIManager.Instance.Show<LeaderboardUI>();
#endif
    }
}
