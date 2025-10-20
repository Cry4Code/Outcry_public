
public class SkillShop : InteractableObject
{
    public override void Interact()
    {
        base.Interact();

        UIManager.Instance.Show<StoreUI>();
    }
}
