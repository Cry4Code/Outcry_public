using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScrewAttack : SkillBase
{
    // 시간 쪼개기 
    private float startStateTime;
    private float startAttackTime = 0.01f;
    private float animRunningTime = 0f;
    private float attackAnimationLength;

    // 쿨타임
    private float lastUsedTime = float.MinValue;
    
    
    public override void Enter(PlayerController controller)
    {
        // 발동 조건 체크 : 지상
        if (!controller.Move.isGrounded)
        {
            Debug.Log("[플레이어] 스킬 ScrewAttack은 지상에서만 사용 가능");
            controller.ChangeState<FallState>();
            return;
        }
        // 쿨타임 체크
        if (Time.time - lastUsedTime < cooldown)
        {
            Debug.Log("[플레이어] 스킬 ScrewAttack는 쿨타임 중");
            controller.ChangeState<IdleState>();
            return;
        }
        // 발동 조건 체크 : 스태미나
        if (!controller.Condition.TryUseStamina(needStamina))
        {
            Debug.Log("[플레이어] 스킬 ScrewAttack 사용을 위한 스태미나 부족");
            controller.ChangeState<IdleState>();
            return;
        }
        Debug.Log("[플레이어] 스킬 ScrewAttack 사용!");
        controller.isLookLocked = false;
        controller.Move.ForceLook(controller.transform.localScale.x < 0);
        controller.isLookLocked = true;
        controller.Move.rb.velocity = Vector2.zero;
        controller.Condition.isCharge = true;
        
        animRunningTime = 0f;
        startStateTime = Time.time;
        attackAnimationLength = 
            controller.Animator.animator.runtimeAnimatorController
                .animationClips.First(c => c.name == "ScrewAttack").length;
        controller.Attack.SetDamageList(damages);
        controller.Animator.SetIntAniamtion(AnimatorHash.PlayerAnimation.AdditionalAttackID, skillId);
        controller.Animator.SetTriggerAnimation(AnimatorHash.PlayerAnimation.AdditionalAttack);
        controller.PlayerInputDisable();
        
    }

    public override void HandleInput(PlayerController controller)
    {
        
    }

    public override void LogicUpdate(PlayerController controller)
    {
        if (Time.time - startStateTime > startAttackTime)
        {
            AnimatorStateInfo curAnimInfo = controller.Animator.animator.GetCurrentAnimatorStateInfo(0);

            if (curAnimInfo.IsTag("AdditionalAttack"))
            { 
                float animTime = curAnimInfo.normalizedTime;

                if (animTime >= 1.0f)
                {
                    if (controller.Move.isGrounded) controller.ChangeState<IdleState>();
                    else controller.ChangeState<FallState>();
                    return;
                }
            }

            if (animRunningTime >= attackAnimationLength)
            {
                if (controller.Move.isGrounded) controller.ChangeState<IdleState>();
                else controller.ChangeState<FallState>();
                return;
            }
        }
    }

    public override void Exit(PlayerController controller)
    {
        Debug.Log("[플레이어] 스킬 ScrewAttack 종료");
        controller.PlayerInputEnable();
        lastUsedTime = Time.time;
        controller.Condition.isCharge = false;
    }
}
