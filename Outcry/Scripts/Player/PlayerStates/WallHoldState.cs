using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallHoldState : AirSubState
{
    public override void Enter(PlayerController controller)
    {
        base.Enter(controller);
        controller.Move.ForceLook(!controller.Move.lastWallIsLeft);
        controller.Attack.ClearAttackCount();
        controller.isLookLocked = true;
        controller.Move.rb.velocity = Vector2.zero;
    }

    public override void Exit(PlayerController controller)
    {
        base.Exit(controller);
        controller.isLookLocked = false;
        controller.Condition.canStaminaRecovery.Value = true;
        // player.PlayerAnimator.ClearBool();
    }

    public override void HandleInput(PlayerController controller)
    {
        var moveInput = controller.Inputs.Player.Move.ReadValue<Vector2>();
        if (!controller.Move.isWallTouched)
        {
            controller.ChangeState<FallState>();
            return;
        }

        // 벽이 있는 방향으로 입력이 들어왔을 때
        if (((moveInput.x < 0 && controller.Move.lastWallIsLeft) 
            || moveInput.x > 0 && !controller.Move.lastWallIsLeft) )
        {
            // 점프 키가 눌림 and 벽점 가능함
            if(controller.Inputs.Player.Jump.triggered && controller.Move.CanWallJump())
            {
                // Debug.Log("벽점으로");
                controller.ChangeState<WallJumpState>();
                return;
            }
            if (controller.Move.isWallTouched)
            {
                // Debug.Log("중력 감소");
                controller.Move.ChangeGravity(true);
                return;
            }
        }

        if (moveInput.x == 0)
        {
            controller.ChangeState<FallState>();
            return;
        }
        if (controller.Move.isGrounded)
        {
            controller.ChangeState<IdleState>();
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
        if (controller.Inputs.Player.AdditionalAttack.triggered)
        {
            controller.ChangeState<AdditionalAttackState>();
            return;
        }
        
    }

    public override void LogicUpdate(PlayerController controller)
    {
        controller.Move.ApplyWallSlideClamp();
        controller.Move.ForceLook(!controller.Move.lastWallIsLeft);
        
        if (controller.Move.rb.velocity.y < 0)
        {
            controller.Animator.SetBoolAnimation(AnimatorHash.PlayerAnimation.WallHold);
        }
        
        if (controller.Move.keyboardLeft != controller.Move.lastWallIsLeft)
        {
            controller.Move.Move();
            return;
        }
    }
}
