using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DodgeState : BasePlayerState
{
    
    private float startStateTime;
    private float startAttackTime = 0.01f;
    private float animRunningTime = 0f;
    private float dodgePower = 20f;
    private float dodgeAnimationLength;
    private Vector2 dodgeDirection;
    private bool isDodged;

    public override eTransitionType ChangableStates { get; }

    public override void Enter(PlayerController controller)
    {
        isDodged = false;
        if (!controller.Condition.TryUseStamina(controller.Data.dodgeStamina))
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
        isDodged = true;
        var moveInputs = controller.Inputs.Player.Move.ReadValue<Vector2>();
        controller.isLookLocked = true;
        if (moveInputs.x != 0)
        {
            // 입력이 있을 때 그 쪽 보게
            dodgeDirection = (moveInputs.x < 0 ? Vector2.left : Vector2.right) * dodgePower;
            controller.Move.ForceLook(moveInputs.x < 0);
        }
        else
        {
            // 입력이 없으면 그냥 보던 쪽으로 가게
            dodgeDirection = (controller.transform.localScale.x < 0 ? Vector2.left : Vector2.right) * dodgePower;
            controller.Move.ForceLook(controller.transform.localScale.x < 0);
        }
        controller.Move.isDodged = true;
        controller.Animator.ClearTrigger();
        controller.Animator.ClearInt();
        controller.Animator.ClearBool();
        
        dodgeAnimationLength = 
            controller.Animator.animator.runtimeAnimatorController
                .animationClips.First(c => c.name == "Dodge").length;
        CameraManager.Instance.ShakeCamera(0.1f, 0.5f, 1f, EffectOrder.Player);
        Debug.Log($"[Dodge] DodgeDirection: {dodgeDirection}");
        /*controller.Move.rb.velocity = Vector2.zero;*/
        /*controller.Move.rb.AddForce(dodgeDirection, ForceMode2D.Impulse);*/
        
        EffectManager.Instance.PlayEffectByIdAndTypeAsync(PlayerEffectID.Dodge, EffectType.Sound, controller.gameObject).Forget();
        controller.Animator.SetTriggerAnimation(AnimatorHash.PlayerAnimation.Dodge);
        controller.Condition.SetInvincible(controller.Data.dodgeInvincibleTime);
        startStateTime = Time.time;
        animRunningTime = 0f;
        controller.Inputs.Player.Move.Disable();
    }

    public override void HandleInput(PlayerController controller)
    {
        
    }

    public override void LogicUpdate(PlayerController controller)
    {
        animRunningTime += Time.deltaTime;
        
        if (Time.time - startStateTime > startAttackTime)
        {
            AnimatorStateInfo curAnimInfo = controller.Animator.animator.GetCurrentAnimatorStateInfo(0);

            if (curAnimInfo.IsName("Dodge"))
            { 
                controller.Move.rb.velocity = dodgeDirection;
                float animTime = curAnimInfo.normalizedTime;

                if (animTime >= 1.0f)
                {
                    if (controller.Move.isGrounded) controller.ChangeState<IdleState>();
                    else controller.ChangeState<FallState>();
                    return;
                }
            }

            if (animRunningTime >= dodgeAnimationLength)
            {
                if (controller.Move.isGrounded) controller.ChangeState<IdleState>();
                else controller.ChangeState<FallState>();
                return;
            }
                
        }
    }

    public override void Exit(PlayerController controller)
    {
        controller.Inputs.Player.Move.Enable();
        int stageId = StageManager.Instance.CurrentStageData.Stage_id;
        if (isDodged &&  stageId != StageID.Village)
        {
            UGSManager.Instance.LogDoAction(stageId, PlayerEffectID.Dodge);
        }
    }
}
