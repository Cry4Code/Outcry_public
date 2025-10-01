using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NormalJumpAttackState : NormalJumpAttackSubState
{
    private float startStateTime;
    private float startAttackTime = 0.01f;
    private float jumpAnimationLength;

    private float animRunningTime = 0f;
    /*private float inAirTime = 0.1f;*/
    
    public override void Enter(PlayerController controller)
    {
        base.Enter(controller);
        startStateTime = Time.time;
        controller.Animator.ClearBool();
        controller.Attack.HasJumpAttack = true;
        controller.Attack.SetDamage(controller.Data.jumpAttackDamage);
        controller.Hitbox.AttackState = AttackState.NormalJumpAttack;
        controller.Animator.SetTriggerAnimation(AnimatorHash.PlayerAnimation.NormalAttack);
        controller.Inputs.Player.Move.Disable();
        controller.Move.rb.gravityScale = 0;
        animRunningTime = 0f;
        jumpAnimationLength = 
            controller.Animator.animator.runtimeAnimatorController
            .animationClips.First(c => c.name == "NormalJumpAttack").length;
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
        /*player.PlayerMove.rb.velocity = new Vector2(player.PlayerMove.rb.velocity.x, 0);*/
        
        controller.Move.rb.velocity = Vector2.zero;
        animRunningTime += Time.deltaTime;
        
        if (Time.time - startStateTime > startAttackTime)
        {
            AnimatorStateInfo curAnimInfo = controller.Animator.animator.GetCurrentAnimatorStateInfo(0);

            if (curAnimInfo.IsName("NormalJumpAttack"))
            { 
                float animTime = curAnimInfo.normalizedTime;

                if (animTime >= 1.0f)
                {
                    controller.ChangeState<IdleState>();
                    return;
                }
            }

            if (animRunningTime >= jumpAnimationLength)
            {
                controller.ChangeState<IdleState>();
                return;
            }
                
        }
    }

    public override void Exit(PlayerController controller)
    {
        base.Exit(controller);
        controller.Move.rb.gravityScale = 1;
    }
}
