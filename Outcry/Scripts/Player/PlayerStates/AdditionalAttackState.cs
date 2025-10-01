using UnityEngine;

public class AdditionalAttackState : IPlayerState
{
    public void Enter(PlayerController controller)
    {
        Debug.Log("[플레이어] 추가 스킬 입력됨");
        controller.Skill.CurrentSkill.Enter(controller);
    }

    public void HandleInput(PlayerController controller)
    {
        controller.Skill.CurrentSkill.HandleInput(controller);
    }

    public void LogicUpdate(PlayerController controller)
    {
        controller.Skill.CurrentSkill.LogicUpdate(controller);
    }

    public void Exit(PlayerController controller)
    {
        controller.Skill.CurrentSkill.Exit(controller);
    }
}
