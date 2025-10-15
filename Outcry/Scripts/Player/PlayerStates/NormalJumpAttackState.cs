using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NormalJumpAttackState : NormalJumpAttackSubState
{
    public override eTransitionType ChangableStates =>
        eTransitionType.SpecialAttackState | eTransitionType.DodgeState | eTransitionType.StartParryState |
        eTransitionType.AdditionalAttackState;
    
    private float startStateTime;
    private float startAttackTime = 0.01f;
    private float jumpAnimationLength;

    private float animRunningTime = 0f;
    
    private bool isLeft = false;
    
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
        controller.isLookLocked = true;
        animRunningTime = 0f;
        isLeft = CursorManager.Instance.IsLeftThan(controller.transform);
        controller.Move.ForceLook(isLeft);
        jumpAnimationLength = 
            controller.Animator.animator.runtimeAnimatorController
            .animationClips.First(c => c.name == "NormalJumpAttack").length;
    }

    public override void HandleInput(PlayerController controller)
    {
        base.HandleInput(controller);
        controller.Move.ForceLook(isLeft);
    }

    public override void LogicUpdate(PlayerController controller)
    {
        /*player.PlayerMove.rb.velocity = new Vector2(player.PlayerMove.rb.velocity.x, 0);*/
        if (!controller.isLookLocked) controller.isLookLocked = true;
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
