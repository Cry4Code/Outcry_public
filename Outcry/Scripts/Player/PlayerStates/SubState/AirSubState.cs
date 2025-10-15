using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirSubState : BasePlayerState
{
    protected Vector2 moveInput;
    
    public override eTransitionType ChangableStates { get; }

    public override void Enter(PlayerController controller)
    {
        controller.Animator.OnBoolParam(AnimatorHash.PlayerAnimation.SubAir);
    }

    public override void HandleInput(PlayerController controller)
    {
        moveInput = controller.Inputs.Player.Move.ReadValue<Vector2>();
        
        if (controller.Inputs.Player.Jump.triggered)
        {
            if(!controller.Move.isDoubleJump)
            {
                controller.ChangeState<DoubleJumpState>();
                return;
            }
        }
        
        if (controller.Inputs.Player.NormalAttack.triggered && moveInput.y < 0)
        {
            controller.isLookLocked = true;
            controller.ChangeState<DownAttackState>();
            return;
        }
        if (controller.Inputs.Player.NormalAttack.triggered && !controller.Attack.HasJumpAttack)
        {
            controller.isLookLocked = true;
            controller.ChangeState<NormalJumpAttackState>();
            return;
        }
        
        if (controller.Inputs.Player.SpecialAttack.triggered)
        {
            controller.isLookLocked = false;
            controller.ChangeState<SpecialAttackState>();
            return;
        }
        if (controller.Inputs.Player.Dodge.triggered)
        {
            controller.ChangeState<DodgeState>();
            return;
        }
        if (controller.Inputs.Player.Parry.triggered)
        {
            controller.ChangeState<StartParryState>();
            return;
        }
        if (controller.Inputs.Player.AdditionalAttack.triggered)
        {
            controller.ChangeState<AdditionalAttackState>();
            return;
        }
    }

    public override void LogicUpdate(PlayerController controller)
    {
        
    }

    public override void Exit(PlayerController controller)
    {
        controller.Animator.OffBoolParam(AnimatorHash.PlayerAnimation.SubAir);
    }
}
