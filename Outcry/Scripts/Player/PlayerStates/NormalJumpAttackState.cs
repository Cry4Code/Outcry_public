using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class NormalJumpAttackState : NormalJumpAttackSubState
{
    public override eTransitionType ChangableStates =>
        eTransitionType.SpecialAttackState | eTransitionType.DodgeState | eTransitionType.StartParryState |
        eTransitionType.AdditionalAttackState;
    
    // 애니메이션 클립 초당 프레임 수
    private const float ANIMATION_FRAME_RATE = 20f;

    private float[] attackSoundTime = new[]
    {
        (1f / ANIMATION_FRAME_RATE) * 2f,
        (1f / ANIMATION_FRAME_RATE) * 6f,
    };

    private int attackSoundIndex = 0;
    
    
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
        attackSoundIndex = 0;
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
            
            if (attackSoundIndex < attackSoundTime.Length)
            {
                if (animRunningTime >= attackSoundTime[attackSoundIndex])
                {
                    attackSoundIndex++;
                    EffectManager.Instance.PlayEffectByIdAndTypeAsync(PlayerEffectID.NormalAttackSound, EffectType.Sound,
                        controller.gameObject).Forget();
                }    
            }

            if (curAnimInfo.IsName("NormalJumpAttack"))
            { 
                
                float animTime = curAnimInfo.normalizedTime;

                if (animTime >= 1.0f)
                {
                    controller.ChangeState<FallState>();
                    return;
                }
            }

            if (animRunningTime >= jumpAnimationLength)
            {
                controller.ChangeState<FallState>();
                return;
            }
                
        }
    }

    public override void Exit(PlayerController controller)
    {
        base.Exit(controller);
        controller.Move.rb.gravityScale = 1;
        int stageId = StageManager.Instance.CurrentStageData.Stage_id;
        if (stageId != StageID.Village)
        {
            UGSManager.Instance.LogDoAction(stageId, PlayerEffectID.JumpAttack);
        }
    }
}
