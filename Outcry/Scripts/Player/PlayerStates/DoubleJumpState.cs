using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DoubleJumpState : AirSubState
{
    public override eTransitionType ChangableStates =>
        eTransitionType.DownAttackState | eTransitionType.NormalJumpAttackState |
        eTransitionType.SpecialAttackState | eTransitionType.DodgeState | eTransitionType.StartParryState |
        eTransitionType.AdditionalAttackState;
    
    private float elapsedTime;
    private float fallStartTime = 0.1f;
    
    public override void Enter(PlayerController controller)
    {
        base.Enter(controller);
        Debug.Log("[플레이어] !!Double Jump!!");
        controller.Condition.canStaminaRecovery.Value = true;
        controller.SetAnimation(AnimatorHash.PlayerAnimation.DoubleJump, true);
        controller.Move.DoubleJump();
        
    }
    
    public override void HandleInput(PlayerController controller) 
    {
        base.HandleInput(controller);
        if(moveInput.x != 0)
            controller.Move.Move();
        
    }

    public override void LogicUpdate(PlayerController controller)
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime > fallStartTime)
        {
            if (controller.Move.rb.velocity.y < 0)
            {
                controller.ChangeState<FallState>();
                return;
            }
        }
        if (controller.Move.isGrounded)
        {
            controller.ChangeState<IdleState>();
            return;
        }
        
    }

    public override void Exit(PlayerController controller)
    {
        base.Exit(controller);
        controller.isLookLocked = false;
    }
}
