using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JumpState : AirSubState
{
    private float minWallHoldTime = 1f; // 이 초가 지나야 벽 짚기가 가능함
    private float elapsedTime;
    
    public override void Enter(PlayerController controller)
    {
        base.Enter(controller);
        controller.Condition.canStaminaRecovery.Value = true;
        controller.Animator.SetTriggerAnimation(AnimatorHash.PlayerAnimation.Jump);
        controller.isLookLocked = true; 
        elapsedTime = 0f;
        
        if (controller.Move.isWallTouched)
        {
            controller.Move.PlaceJump();
        }
        else
        {
            controller.Move.Jump();
        }
        
        if (!controller.Move.isGroundJump) controller.Move.isGroundJump = true;
    }

    public override void HandleInput(PlayerController controller)
    {
        elapsedTime += Time.deltaTime;
        var moveInput = controller.Inputs.Player.Move.ReadValue<Vector2>();
        
        if (controller.Inputs.Player.Jump.triggered)
        {
            if(!controller.Move.isDoubleJump)
            {
                controller.ChangeState<DoubleJumpState>();
                return;
            }
        }
        if (controller.Move.isWallTouched && elapsedTime >= minWallHoldTime)
        {
            controller.ChangeState<WallHoldState>();
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
        if (!controller.Move.isGroundJump)
        {
            controller.Move.isGroundJump = true;
            return;
        }
        controller.Move.Move();
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
