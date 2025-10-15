using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JumpState : AirSubState
{
    public override eTransitionType ChangableStates =>
        eTransitionType.DoubleJumpState | eTransitionType.DownAttackState | eTransitionType.NormalJumpAttackState |
        eTransitionType.SpecialAttackState | eTransitionType.DodgeState | eTransitionType.StartParryState |
        eTransitionType.AdditionalAttackState;
    
    public override void Enter(PlayerController controller)
    {
        base.Enter(controller);
        controller.Condition.canStaminaRecovery.Value = true;
        controller.Animator.SetTriggerAnimation(AnimatorHash.PlayerAnimation.Jump);
        /*controller.isLookLocked = true; */
        controller.isLookLocked = false; 
        
        if (controller.Move.isWallTouched)
        {
            controller.Move.PlaceJump();
        }
        else
        {
            controller.Move.Jump();
        }
        
        /*if (moveInput.x != 0)
        {
            controller.Move.ForceLook(moveInput.x < 0);
            controller.Move.Move();
        }*/
        
        if (!controller.Move.isGroundJump) controller.Move.isGroundJump = true;
    }
    
    public override void HandleInput(PlayerController controller)
    {
        base.HandleInput(controller);
        
        if (moveInput.x != 0)
        {
            /*controller.Move.ForceLook(moveInput.x < 0);*/
            controller.Move.Move();
        }
        else if (controller.Move.rb.velocity.x != 0)
        {
            controller.Move.Stop();
        }
    }

    public override void LogicUpdate(PlayerController controller)
    {
        if (!controller.Move.isGroundJump)
        {
            controller.Move.isGroundJump = true;
            return;
        }
        if (controller.Move.rb.velocity.y < 1f)
        {
            controller.ChangeState<FallState>();
            return;
        }
    }

    public override void Exit(PlayerController controller)
    {
        base.Exit(controller);
        controller.isLookLocked = false;
    }
}
