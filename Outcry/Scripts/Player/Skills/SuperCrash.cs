using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SuperCrash : SkillBase
{
    // SuperCrash -> WhileSuperCrash -> EndSuperCrash
    // SuperCrash 진행할 동안만 공중에서 멈춰있고 나머지 재생해주면 됨
    
    // 시간 쪼개기 
    private float startAttackTime = 0.01f;
    private float animRunningTime = 0f;
    private float inAirAnimationLength;
    
    // 쿨타임
    private float lastUsedTime = float.MinValue;
    
    public override void Enter(PlayerController controller)
    {
        // 발동 조건 체크 : 공중
        if (controller.Move.isGrounded)
        {
            Debug.Log("[플레이어] 스킬 SuperCrash는 공중에서만 사용 가능");
            controller.ChangeState<IdleState>();
            return;
        }
        // 쿨타임 체크
        if (Time.time - lastUsedTime < cooldown)
        {
            Debug.Log("[플레이어] 스킬 SuperCrash는 쿨타임 중");
            controller.ChangeState<FallState>();
            return;
        }
        // 발동 조건 체크 : 스태미나
        if (!controller.Condition.TryUseStamina(needStamina))
        {
            Debug.Log("[플레이어] 스킬 SuperCrash 사용을 위한 스태미나 부족");
            controller.ChangeState<FallState>();
            return;
        }
        Debug.Log("[플레이어] 스킬 SuperCrash 사용!");
        
        // 시점 고정
        controller.isLookLocked = false;
        controller.Move.ForceLook(controller.transform.localScale.x < 0);
        controller.isLookLocked = true;
        controller.Condition.isCharge = true;
        
        animRunningTime = 0f;
        inAirAnimationLength = 
            controller.Animator.animator.runtimeAnimatorController
                .animationClips.First(c => c.name == "SuperCrash").length;
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
        animRunningTime += Time.deltaTime;
        if (animRunningTime < inAirAnimationLength)
        {
            controller.Move.rb.velocity = Vector2.zero;
            return;
        }
        else
        {
            controller.Move.rb.gravityScale = 10f;
            if (controller.Move.isGrounded)
            {
                controller.ChangeState<IdleState>();
                return;
            }
        }
    }

    public override void Exit(PlayerController controller)
    {
        Debug.Log("[플레이어] 스킬 SuperCrash 종료");
        controller.Move.rb.gravityScale = 1f;
        controller.PlayerInputEnable();
        lastUsedTime = Time.time;
        controller.Condition.isCharge = false;
    }
}
