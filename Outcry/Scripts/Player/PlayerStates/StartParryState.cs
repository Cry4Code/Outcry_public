using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StartParryState : BasePlayerState
{
    private float startStateTime;
    private float startAttackTime = 0.01f;
    private float t;
    private float parryTime;

    public override eTransitionType ChangableStates { get; }

    public async override void Enter(PlayerController controller)
    {
        if (!controller.Condition.TryUseStamina(controller.Data.parryStamina))
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
        await EffectManager.Instance.PlayEffectByIdAndTypeAsync(PlayerEffectID.StartParrying, EffectType.Sound, controller.gameObject);
        controller.isLookLocked = false;
        controller.Move.ForceLook(CursorManager.Instance.mousePosition.x - controller.transform.position.x < 0);
        controller.Move.rb.velocity = Vector2.zero;
        controller.Animator.ClearTrigger();
        controller.Animator.ClearInt();
        controller.Animator.ClearBool();
        controller.Inputs.Player.Move.Disable();
        controller.Animator.SetTriggerAnimation(AnimatorHash.PlayerAnimation.StartParry);
        parryTime = controller.Animator.animator.runtimeAnimatorController
            .animationClips.First(c => c.name == "StartParry").length;
        
        
        controller.isLookLocked = true;
        controller.Attack.isStartParry = true;
        t = 0;
    }

    public override void HandleInput(PlayerController controller)
    {
        
    }

    public override void LogicUpdate(PlayerController controller)
    {
        t += Time.deltaTime;
        
        if (controller.Attack.successParry)
        {
            controller.ChangeState<SuccessParryState>();
            return;
        }
        
        if (Time.time - startStateTime > startAttackTime)
        {
            AnimatorStateInfo curAnimInfo = controller.Animator.animator.GetCurrentAnimatorStateInfo(0);

            if (curAnimInfo.IsName("StartParry"))
            { 
                float animTime = curAnimInfo.normalizedTime;
                
                if (animTime >= 1.0f)
                {
                    if (controller.Move.isGrounded) controller.ChangeState<IdleState>();
                    else controller.ChangeState<FallState>();
                    return;
                }
                
                if (t >= parryTime)
                {
                    if (controller.Move.isGrounded) controller.ChangeState<IdleState>();
                    else controller.ChangeState<FallState>();
                    return;
                }
            }
        }
    }

    public override void Exit(PlayerController controller)
    {
        if(!controller.Attack.successParry) controller.Condition.NoMoreInvincible();
        controller.Attack.isStartParry = false;
    }
}
