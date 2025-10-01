using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DownAttackState : DownAttackSubState
{
    public override void Enter(PlayerController controller)
    {
        base.Enter(controller);
        controller.Animator.ClearBool();
        controller.isLookLocked = true; 
        controller.Condition.canStaminaRecovery.Value = false;
        controller.Hitbox.AttackState = AttackState.DownAttack;
        controller.Animator.SetTriggerAnimation(AnimatorHash.PlayerAnimation.DownAttack);
        controller.Inputs.Player.Move.Disable();
        controller.Move.rb.gravityScale = 8f;
    }

    public override void HandleInput(PlayerController controller)
    {
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
    }

    public override void LogicUpdate(PlayerController controller)
    {
        if (controller.Move.isGrounded)
        {
            controller.ChangeState<IdleState>();
            return;
        }
    }

    public override void Exit(PlayerController controller)
    {
        base.Exit(controller);
        controller.Move.rb.gravityScale = 1f;
    }
}
