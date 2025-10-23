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
                TryChangeState(eTransitionType.DoubleJumpState, controller);
                return;
            }
        }
        
        if (controller.Inputs.Player.NormalAttack.triggered && moveInput.y < 0)
        {
            controller.isLookLocked = true;
            TryChangeState(eTransitionType.DownAttackState, controller);
            return;
        }
        if (controller.Inputs.Player.NormalAttack.triggered && !controller.Attack.HasJumpAttack)
        {
            controller.isLookLocked = true;
            TryChangeState(eTransitionType.NormalJumpAttackState, controller);
            return;
        }
        
        if (controller.Inputs.Player.SpecialAttack.triggered)
        {
            controller.isLookLocked = false;
            TryChangeState(eTransitionType.SpecialAttackState, controller);
            return;
        }
        if (controller.Inputs.Player.Dodge.triggered)
        {
            TryChangeState(eTransitionType.DodgeState, controller);
            return;
        }
        if (controller.Inputs.Player.Parry.triggered)
        {
            TryChangeState(eTransitionType.StartParryState, controller);
            return;
        }
        if (controller.Skill.CurrentSkill != null && controller.Inputs.Player.AdditionalAttack.triggered)
        {
            TryChangeState(eTransitionType.AdditionalAttackState, controller);
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
