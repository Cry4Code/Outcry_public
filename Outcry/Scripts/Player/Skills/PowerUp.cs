using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PowerUp : SkillBase
{
    // 애니메이션 재생 끝나면 공격력 증가해주면 됨
    
    // 시간 쪼개기 
    private float startStateTime;
    private float startAttackTime = 0.01f;
    private float animRunningTime = 0f;
    private float buffAnimationTime;

    
    // 쿨타임
    private float lastUsedTime = float.MinValue;
    
    public override void Enter(PlayerController controller)
    {
        // 발동 조건 체크 : 지상
        if (!controller.Move.isGrounded)
        {
            Debug.Log("[플레이어] 스킬 PowerUp는 지상에서만 사용 가능");
            controller.ChangeState<FallState>();
            return;
        }
        // 쿨타임 체크
        if (Time.time - lastUsedTime < cooldown)
        {
            Debug.Log("[플레이어] 스킬 PowerUp는 쿨타임 중");
            controller.ChangeState<FallState>();
            return;
        }
        // 발동 조건 체크 : 스태미나
        if (!controller.Condition.TryUseStamina(needStamina))
        {
            Debug.Log("[플레이어] 스킬 PowerUp 사용을 위한 스태미나 부족");
            controller.ChangeState<IdleState>();
            return;
        }
        Debug.Log("[플레이어] 스킬 PowerUp 사용!");
        
        // 시점 고정
        controller.isLookLocked = false;
        controller.Move.ForceLook(controller.transform.localScale.x < 0);
        controller.isLookLocked = true;
        controller.Condition.isCharge = true;
        
        animRunningTime = 0f;
        buffAnimationTime = 
            controller.Animator.animator.runtimeAnimatorController
                .animationClips.First(c => c.name == "PowerUp").length;
        controller.Animator.SetIntAniamtion(AnimatorHash.PlayerAnimation.AdditionalAttackID, skillId);
        controller.Animator.SetTriggerAnimation(AnimatorHash.PlayerAnimation.AdditionalAttack);
        controller.PlayerInputDisable();
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

            if (curAnimInfo.IsTag("AdditionalAttack"))
            { 
                float animTime = curAnimInfo.normalizedTime;

                if (animTime >= 1.0f)
                {
                    controller.Attack.BuffDamage(buffValue, duration);
                    if (controller.Move.isGrounded) controller.ChangeState<IdleState>();
                    else controller.ChangeState<FallState>();
                    return;
                }
            }

            if (animRunningTime >= buffAnimationTime)
            {
                controller.Attack.BuffDamage(buffValue, duration);
                if (controller.Move.isGrounded) controller.ChangeState<IdleState>();
                else controller.ChangeState<FallState>();
                return;
            }
                
        }
    }

    public override void Exit(PlayerController controller)
    {
        Debug.Log("[플레이어] 스킬 PowerUp 종료");
        controller.PlayerInputEnable();
        lastUsedTime = Time.time;
        controller.Condition.isCharge = false;
    }
}
