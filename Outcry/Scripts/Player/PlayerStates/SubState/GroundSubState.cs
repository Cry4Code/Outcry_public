using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundSubState : BasePlayerState
{
    public override eTransitionType ChangableStates { get; }
    protected Vector2 input;

    public override void Enter(PlayerController controller)
    {
        controller.Animator.OnBoolParam(AnimatorHash.PlayerAnimation.SubGround);
    }

    public override void HandleInput(PlayerController controller)
    {
        input = controller.Inputs.Player.Move.ReadValue<Vector2>();
        
        if (input.x != 0)
        {
            if (CanChangeTo(eTransitionType.MoveState))
            {
                controller.isLookLocked = true;
                TryChangeState(eTransitionType.MoveState, controller);
                return;    
            }
        }
        else
        {
            if (CanChangeTo(eTransitionType.IdleState))
                TryChangeState(eTransitionType.IdleState, controller);
        }
        
        if (controller.Inputs.Player.Jump.triggered 
            && controller.Move.isGrounded 
            && !controller.Move.isGroundJump)
        {
            // Debug.Log("Jump Key Input");
            TryChangeState(eTransitionType.JumpState, controller);
            return;
        }
        
        if (controller.Inputs.Player.NormalAttack.triggered)
        {
            if (CanChangeTo(eTransitionType.NormalAttackState))
            {
                controller.isLookLocked = false;
                TryChangeState(eTransitionType.NormalAttackState, controller);
                return;
            }
        }
        if (controller.Inputs.Player.Potion.triggered && controller.Condition.potionCount.Value > 0)
        {
            TryChangeState(eTransitionType.PotionState, controller);
            return;
        }
        if (controller.Skill.CurrentSkill != null && controller.Inputs.Player.AdditionalAttack.triggered)
        {
            TryChangeState(eTransitionType.AdditionalAttackState, controller);
            return;
        }
        
        if (controller.Inputs.Player.SpecialAttack.triggered)
        {
            if (CanChangeTo(eTransitionType.SpecialAttackState))
            {
                controller.isLookLocked = false;
                TryChangeState(eTransitionType.SpecialAttackState, controller);
                return;
            }
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
    }

    public override void LogicUpdate(PlayerController controller)
    {
        
    }

    public override void Exit(PlayerController controller)
    {
        controller.Animator.OffBoolParam(AnimatorHash.PlayerAnimation.SubGround);
    }
}
