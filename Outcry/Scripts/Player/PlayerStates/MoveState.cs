using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState : GroundSubState
{
    public override eTransitionType ChangableStates =>
        eTransitionType.JumpState | eTransitionType.IdleState | eTransitionType.NormalAttackState | 
        eTransitionType.SpecialAttackState | eTransitionType.FallState | eTransitionType.DodgeState | 
        eTransitionType.StartParryState | eTransitionType.PotionState | eTransitionType.AdditionalAttackState;
    
    private float lastSFXTime = 0;
    private float SFXThresholdTime = 0.3f;
    
    
    public override void Enter(PlayerController controller)
    {
        base.Enter(controller);
        controller.Animator.OnBoolParam(AnimatorHash.PlayerAnimation.Move);
        controller.Condition.canStaminaRecovery.Value = true;
        controller.isLookLocked = true;
        lastSFXTime = 0;

    }

    public override void HandleInput(PlayerController controller)
    {
        base.HandleInput(controller);
        if (!controller.isLookLocked) controller.isLookLocked = true;
        controller.Move.ForceLook(input.x < 0);
    }

    public override async void LogicUpdate(PlayerController controller)
    {
        if (Time.time - lastSFXTime > SFXThresholdTime)
        {
            await EffectManager.Instance.PlayEffectsByIdAsync(PlayerEffectID.Move, EffectOrder.Player,
                controller.gameObject);
            lastSFXTime = Time.time;
        }
        
        if (controller.Move.rb.velocity.y < 0)
        {
            controller.ChangeState<FallState>();
            return;
        }
        controller.Move.Move();
        
        
    }

    public override void Exit(PlayerController controller) 
    {
        base.Exit(controller);
        controller.Animator.OffBoolParam(AnimatorHash.PlayerAnimation.Move);
    }
}
