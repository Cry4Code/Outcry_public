using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DamagedState : IPlayerState
{
    
    private float startStateTime;
    private float startAttackTime = 0.01f;
    private float canInputTime = 0.1f;
    private float t;
    private float damagedTime;
    
    public void Enter(PlayerController controller)
    {
        controller.Move.rb.velocity = Vector2.zero;
        controller.Condition.canStaminaRecovery.Value = true;
        controller.Animator.SetTriggerAnimation(AnimatorHash.PlayerAnimation.Damaged);
        controller.Inputs.Player.Move.Disable();
        controller.Inputs.Player.Jump.Disable();
        controller.Inputs.Player.NormalAttack.Disable();
        controller.Inputs.Player.Look.Disable();
        
        damagedTime = controller.Animator.animator.runtimeAnimatorController
            .animationClips.First(c => c.name == "Damaged").length;
        t = 0;
    }

    public void HandleInput(PlayerController controller)
    {
        if (Time.time - startStateTime > canInputTime)
        {
            controller.Inputs.Player.Move.Enable();
            controller.Inputs.Player.Jump.Enable();
            controller.Inputs.Player.NormalAttack.Enable();
            controller.Inputs.Player.Look.Enable();
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

    public void LogicUpdate(PlayerController controller)
    {
        AnimatorStateInfo curAnimInfo = controller.Animator.animator.GetCurrentAnimatorStateInfo(0);
        if (!curAnimInfo.IsName("Damaged"))
        {
            controller.Animator.SetTriggerAnimation(AnimatorHash.PlayerAnimation.Damaged);
            return;
        }
         
        float animTime = curAnimInfo.normalizedTime;

        t += Time.deltaTime;
        
        if (animTime >= 1.0f)
        {
            controller.ChangeState<IdleState>();
            return;
        }  
        
        if (t >= damagedTime)
        {
            if (controller.Move.isGrounded) controller.ChangeState<IdleState>();
            else controller.ChangeState<FallState>();
            return;
        }
            
        
    }

    public void Exit(PlayerController controller)
    {
        controller.Inputs.Player.Move.Enable();
        controller.Inputs.Player.Jump.Enable();
        controller.Inputs.Player.NormalAttack.Enable();
        controller.Inputs.Player.Look.Enable();
    }
}
