using UnityEngine;

public class AdditionalAttackState : BasePlayerState
{
    public override eTransitionType ChangableStates { get; }

    public override void Enter(PlayerController controller)
    {
        Debug.Log("[플레이어] 추가 스킬 입력됨");
        if (controller.Skill.CurrentSkill == null)
        {
            if (controller.Move.rb.velocity.y != 0f)
            {
                controller.ChangeState<FallState>();
                return;
            }
            else
            {
                controller.ChangeState<IdleState>();
                return;
            }
        }
        controller.Skill.CurrentSkill?.Enter();
    }

    public override void HandleInput(PlayerController controller)
    {
        
    }

    public override void LogicUpdate(PlayerController controller)
    {
        controller.Skill.CurrentSkill?.LogicUpdate();
    }

    public override void Exit(PlayerController controller)
    {
        controller.Skill.CurrentSkill?.Exit();
    }
}
