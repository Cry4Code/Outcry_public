using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DoubleJumpState : AirSubState
{
    private float elapsedTime;
    private float fallStartTime = 0.1f;
    
    public override void Enter(PlayerController controller)
    {
        if (!controller.Condition.TryUseStamina(controller.Data.doubleJumpStamina))
        {
            if (controller.Move.isGrounded)
            {
                controller.ChangeState<IdleState>();
                return;
            }
            else
            {
                controller.ChangeState<FallState>();
                return;
            }
        }
        base.Enter(controller);
        Debug.Log("[플레이어] !!Double Jump!!");
        controller.SetAnimation(AnimatorHash.PlayerAnimation.DoubleJump, true);
        controller.Move.DoubleJump();
        
    }
    
    public override void HandleInput(PlayerController controller) 
    {
        elapsedTime += Time.deltaTime;
        var moveInput = controller.Inputs.Player.Move.ReadValue<Vector2>();
        
        if(controller.Move.isWallTouched 
           && ((moveInput.x < 0 && controller.Move.lastWallIsLeft) || (moveInput.x > 0 && !controller.Move.lastWallIsLeft)))
        {
            controller.ChangeState<WallHoldState>();
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
        var moveInput = controller.Inputs.Player.Move.ReadValue<Vector2>();
        if(moveInput != null)
            controller.Move.Move();

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
        
        
    }

    public override void Exit(PlayerController controller)
    {
        base.Exit(controller);
    }
}
