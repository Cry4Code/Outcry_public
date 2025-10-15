using UnityEngine;

public class PlayerHouse : InteractableObject
{
    public override void Interact()
    {
        base.Interact();

        UIManager.Instance.Show<SkillSelectUI>();
    }
}
