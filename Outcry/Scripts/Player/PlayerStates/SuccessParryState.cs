using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuccessParryState : BasePlayerState
{
    private float startStateTime;
    private float startAttackTime = 0.01f;
    private bool isStartSFX = false;
    public override eTransitionType ChangableStates { get; }

    public override void Enter(PlayerController controller)
    {
        controller.isLookLocked = false;
        controller.Move.ForceLook(CursorManager.Instance.mousePosition.x - controller.transform.position.x < 0);
        controller.Move.rb.velocity = Vector2.zero;
        controller.Animator.ClearTrigger();
        controller.Animator.ClearInt();
        controller.Animator.ClearBool();
        controller.Inputs.Player.Move.Disable();
        controller.Attack.SetDamage(controller.Data.parryDamage);
        
        controller.Animator.SetTriggerAnimation(AnimatorHash.PlayerAnimation.SuccessParry);
        controller.Condition.stamina.Add(controller.Data.parryStamina);
        
        isStartSFX = false;
        controller.isLookLocked = true;
    }

    public override void HandleInput(PlayerController controller)
    {
        
    }

    public async override void LogicUpdate(PlayerController controller)
    {
        if (Time.time - startStateTime > startAttackTime)
        {
            AnimatorStateInfo curAnimInfo = controller.Animator.animator.GetCurrentAnimatorStateInfo(0);

            if (curAnimInfo.IsName("SuccessParry"))
            { 
                float animTime = curAnimInfo.normalizedTime;

                if (animTime >= 0.2f && !isStartSFX)
                {
                    EffectManager.Instance.StopEffectByType(EffectType.Sound);
                    await EffectManager.Instance.PlayEffectByIdAndTypeAsync(PlayerEffectID.SuccessParryingSound, EffectType.Sound,
                        controller.gameObject);
                    isStartSFX = true;
                }
                
                if (animTime >= 1.0f)
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
        controller.Attack.successParry = false;
    }
}
