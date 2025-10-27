using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallState : AirSubState
{
    public override eTransitionType ChangableStates =>
        eTransitionType.DoubleJumpState | eTransitionType.DownAttackState | eTransitionType.NormalJumpAttackState |
        eTransitionType.SpecialAttackState | eTransitionType.DodgeState | eTransitionType.StartParryState |
        eTransitionType.AdditionalAttackState;
    
    public override void Enter(PlayerController controller)
    {
        base.Enter(controller);
        /*controller.isLookLocked = true;*/ 
        controller.Condition.canStaminaRecovery.Value = true;
        controller.Animator.SetBoolAnimation(AnimatorHash.PlayerAnimation.Fall);
        controller.Move.rb.gravityScale = 3f;
    }

    public override void Exit(PlayerController controller)
    {
        base.Exit(controller);
        controller.isLookLocked = false;
        controller.Move.rb.gravityScale = 1f;
        
        /*controller.isLookLocked = false; */
    }

    public override void HandleInput(PlayerController controller)
    {
        
        if (moveInput.x != 0)
        {
            /*controller.Move.ForceLook(moveInput.x < 0);*/
            controller.Move.Move();
        }
        else if (controller.Move.rb.velocity.x != 0)
        {
            controller.Move.Stop();
        }
        
        base.HandleInput(controller);

    }

    public override void LogicUpdate(PlayerController controller)
    {
        if (controller.Move.isGrounded)
        {
            controller.ChangeState<IdleState>();
            return;
        }
    }
    
    
}
