public class LeaderBoard : InteractableObject
{
    public override void Interact()
    {
        base.Interact();

        UIManager.Instance.Show<LeaderboardUI>();
    }
}
