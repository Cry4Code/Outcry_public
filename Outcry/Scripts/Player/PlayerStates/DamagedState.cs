using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DamagedState : BasePlayerState
{
    
    private float startStateTime;
    private float startAttackTime = 0.01f;
    private float canInputTime = 0.3f;
    private float t;
    private float damagedTime;
    private bool isKeyEnabled = false;

    private Vector2 moveInput;

    public override eTransitionType ChangableStates { get; }

    public override void Enter(PlayerController controller)
    {
        controller.Move.rb.velocity = Vector2.zero;
        controller.Condition.canStaminaRecovery.Value = true;
        controller.Animator.SetTriggerAnimation(AnimatorHash.PlayerAnimation.Damaged);
        controller.isLookLocked = true;
        controller.Inputs.Player.Move.Disable();
        controller.Inputs.Player.Jump.Disable();
        controller.Inputs.Player.NormalAttack.Disable();
        isKeyEnabled = false;
        startStateTime = Time.time;
        
        damagedTime = controller.Animator.animator.runtimeAnimatorController
            .animationClips.First(c => c.name == "Damaged").length;
        t = 0;
    }

    public override void HandleInput(PlayerController controller)
    {
        if (Time.time - startStateTime > canInputTime)
        {
            if (!isKeyEnabled)
            {
                controller.isLookLocked = false;
                controller.Inputs.Player.Move.Enable();
                controller.Inputs.Player.Jump.Enable();
                controller.Inputs.Player.NormalAttack.Enable();
                isKeyEnabled = true;
            }
            
            moveInput = controller.Inputs.Player.Move.ReadValue<Vector2>();
            
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
            if (controller.Move.isGrounded) controller.ChangeState<IdleState>();
            else controller.ChangeState<FallState>();
            return;
        }  
        
        if (t >= damagedTime)
        {
            if (controller.Move.isGrounded) controller.ChangeState<IdleState>();
            else controller.ChangeState<FallState>();
            return;
        }
            
        
    }

    public override void Exit(PlayerController controller)
    {
        controller.isLookLocked = false;
        controller.Inputs.Player.Move.Enable();
        controller.Inputs.Player.Jump.Enable();
        controller.Inputs.Player.NormalAttack.Enable();
    }
}
