using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PotionState : BasePlayerState
{
    private float startStateTime;
    private float startPotionTime = 0.01f;
    private float animRunningTime = 0f;
    private float potionAnimationLength;
    private bool isGetPotion = false;

    public override eTransitionType ChangableStates { get; }

    public override void Enter(PlayerController controller)
    {
        isGetPotion = false;
        animRunningTime = 0f;
        controller.Move.rb.velocity = Vector2.zero;
        startStateTime = Time.time;
        potionAnimationLength = 
            controller.Animator.animator.runtimeAnimatorController
                .animationClips.First(c => c.name == "Potion").length;
        controller.Animator.ClearBool();
        controller.Condition.getPotion.Value = true; // 포션 먹기 시작
        controller.Animator.SetTriggerAnimation(AnimatorHash.PlayerAnimation.Potion);
        Debug.Log("[플레이어] 체력 회복 시도");
    }
    
    public override void HandleInput(PlayerController controller)
    {
        /*var input = controller.Inputs.Player.Move.ReadValue<Vector2>();
        
        if (controller.Inputs.Player.Jump.triggered 
            && controller.Move.isGrounded 
            && !controller.Move.isGroundJump 
            && !controller.Move.isWallTouched)
        {
            // Debug.Log("Jump Key Input");
            controller.ChangeState<JumpState>();
            return;
        }
        
        if (input.x != 0)
        {
            controller.Move.ForceLook(input.x < 0);
            controller.isLookLocked = true;
            controller.ChangeState<MoveState>();
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
        }*/
    }

    public async override void LogicUpdate(PlayerController controller)
    {
        animRunningTime += Time.deltaTime;
        
        if (Time.time - startStateTime > startPotionTime)
        {
            AnimatorStateInfo curAnimInfo = controller.Animator.animator.GetCurrentAnimatorStateInfo(0);

            if (curAnimInfo.IsName("Potion"))
            { 
                float animTime = curAnimInfo.normalizedTime;

                if (animTime >= 1.0f)
                {
                    await EffectManager.Instance.PlayEffectsByIdAsync(PlayerEffectID.Potion, EffectOrder.Player,
                        controller.gameObject);
                    controller.Condition.potionCount--;
                    controller.Condition.health.Add(controller.Condition.potionHealthRecovery);
                    isGetPotion = true;
                    Debug.Log($"[플레이어] 체력 회복됨 {controller.Condition.health.CurValue()}");
                    controller.ChangeState<IdleState>();
                    return;
                }
            }

            if (animRunningTime >= potionAnimationLength)
            {
                await EffectManager.Instance.PlayEffectsByIdAsync(PlayerEffectID.Potion, EffectOrder.Player,
                    controller.gameObject);
                controller.Condition.potionCount--; 
                controller.Condition.health.Add(controller.Condition.potionHealthRecovery);
                isGetPotion = true;
                Debug.Log($"[플레이어] 체력 회복됨 {controller.Condition.health.CurValue()}");
                controller.ChangeState<IdleState>();
                return;
            }
                
        }
    }
    public override void Exit(PlayerController controller)
    {
        controller.Condition.getPotion.Value = false;
        int stageId = StageManager.Instance.CurrentStageData.Stage_id;
        if (isGetPotion && stageId != StageID.Village)
        {
            UGSManager.Instance.LogDoAction(stageId, PlayerEffectID.Potion);
        }
    }
}
