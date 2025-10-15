
public class DungeonGate : InteractableObject
{
    public override void Interact()
    {
        base.Interact();

        UIManager.Instance.Show<ChapterUI>();
    }
}
