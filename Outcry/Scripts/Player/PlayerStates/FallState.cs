using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallState : AirSubState
{
    public override void Enter(PlayerController controller)
    {
        base.Enter(controller);
        controller.isLookLocked = true; 
        controller.Condition.canStaminaRecovery.Value = true;
        controller.Animator.SetBoolAnimation(AnimatorHash.PlayerAnimation.Fall);
        controller.Move.rb.gravityScale = 4f;
    }

    public override void Exit(PlayerController controller)
    {
        base.Exit(controller);
        controller.Move.rb.gravityScale = 1f;
        controller.Attack.HasJumpAttack = false;
        controller.isLookLocked = false; 
    }

    public override void HandleInput(PlayerController controller)
    {
        var moveInputs = controller.Inputs.Player.Move.ReadValue<Vector2>();

        if (controller.Move.isWallTouched
            && ((controller.Move.lastWallIsLeft && moveInputs.x < 0) || (!controller.Move.lastWallIsLeft && moveInputs.x > 0)))
        {

            controller.ChangeState<WallHoldState>();
            return;
        }


        if (controller.Inputs.Player.Jump.triggered)
        {
            if (!controller.Move.isDoubleJump)
            {
                controller.ChangeState<DoubleJumpState>();
                return;
            }
        }
        
        if (controller.Inputs.Player.NormalAttack.triggered && moveInputs.y < 0)
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

        // 공중에서 이동 가능
        var input = controller.Inputs.Player.Move.ReadValue<Vector2>();
        if (input != null)
        {
            if (input.x != 0)
            {
                controller.Move.ForceLook(input.x < 0);
            }
            controller.Move.Move();
        }

        if (controller.Move.isGrounded)
        {
            controller.ChangeState<IdleState>();
            return;
        }
        

        
    }
}
