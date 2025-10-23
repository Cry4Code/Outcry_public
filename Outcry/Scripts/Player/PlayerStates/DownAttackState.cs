using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DownAttackState : DownAttackSubState
{
    public override eTransitionType ChangableStates => eTransitionType.None;
    private bool isLeft = false;
    
    public override void Enter(PlayerController controller)
    {
        base.Enter(controller);
        controller.Animator.ClearBool();
        controller.isLookLocked = true; 
        isLeft = CursorManager.Instance.IsLeftThan(controller.transform);
        controller.Move.ForceLook(isLeft);
        controller.Condition.canStaminaRecovery.Value = false;
        controller.Hitbox.AttackState = AttackState.DownAttack;
        controller.Animator.SetTriggerAnimation(AnimatorHash.PlayerAnimation.DownAttack);
        controller.Inputs.Player.Move.Disable();
        controller.Move.rb.gravityScale = 8f;
    }

    public override void HandleInput(PlayerController controller)
    {
        base.HandleInput(controller);
        controller.Move.ForceLook(isLeft);
    }
    
    public override void LogicUpdate(PlayerController controller)
    {
        if (controller.Move.isGrounded)
        {
            EffectManager.Instance.PlayEffectByIdAndTypeAsync(PlayerEffectID.NormalAttackSound, EffectType.Sound,
                controller.gameObject).Forget();
            controller.ChangeState<IdleState>();
            return;
        }
    }

    public override void Exit(PlayerController controller)
    {
        base.Exit(controller);
        controller.Move.rb.gravityScale = 1f;
        int stageId = StageManager.Instance.CurrentStageData.Stage_id;
        if (stageId != StageID.Village)
        {
            UGSManager.Instance.LogDoAction(stageId, PlayerEffectID.JumpDownAttack);
        }
    }
}
