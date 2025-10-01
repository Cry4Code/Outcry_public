using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState : GroundSubState
{
    private float lastSFXTime = 0;
    private float SFXThresholdTime = 0.3f;
    
    
    public override void Enter(PlayerController controller)
    {
        base.Enter(controller);
        controller.Animator.OnBoolParam(AnimatorHash.PlayerAnimation.Move);
        controller.Condition.canStaminaRecovery.Value = true;
        lastSFXTime = 0;

    }

    public override void HandleInput(PlayerController controller)
    {
        var input = controller.Inputs.Player.Move.ReadValue<Vector2>();
        if (controller.Inputs.Player.Jump.triggered
            && controller.Move.isGrounded
            && !controller.Move.isGroundJump)
        {
            controller.ChangeState<JumpState>();
            return;
        }
        if (input.x == 0)
        {
            controller.ChangeState<IdleState>();
            return;
        }
        // else if (player.Inputs.Player.Dodge.triggered) player.ChangeState(new DodgeState());

        // 땅에 안닿아있고, 벽에 닿았고, 좌우 입력이 벽의 방향과 같을 때 벽 짚기로 변경
        if (!controller.Move.isGrounded
            && controller.Move.isWallTouched
            && ((input.x < 0 && controller.Move.lastWallIsLeft) || (input.x > 0 && !controller.Move.lastWallIsLeft) ))
        {
            controller.ChangeState<WallHoldState>();
            return;
        }
        
        if (controller.Inputs.Player.NormalAttack.triggered)
        {
            controller.isLookLocked = true;
            controller.ChangeState<NormalAttackState>();
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
        if (controller.Inputs.Player.Potion.triggered && controller.Condition.potionCount > 0)
        {
            controller.ChangeState<PotionState>();
            return;
        }
        if (controller.Inputs.Player.AdditionalAttack.triggered)
        {
            controller.ChangeState<AdditionalAttackState>();
            return;
        }

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
