using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SuperCrash : SkillBase
{
    // SuperCrash -> WhileSuperCrash -> EndSuperCrash
    // SuperCrash 진행할 동안만 공중에서 멈춰있고 나머지 재생해주면 됨

    private bool isEnded = false;
    private float endAnimationLength = -1;
    
    public override void Enter()
    {
        useSuccessed = false;
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
        useSuccessed = true;
        // 시점 고정
        controller.isLookLocked = false;
        isEnded = false;
        controller.Move.ForceLook(controller.transform.localScale.x < 0);
        controller.isLookLocked = true;
        controller.Condition.isCharge = false;
        controller.Condition.isSuperArmor = true;
        if (endAnimationLength < 0)
        {
            endAnimationLength = controller.Animator.animator.runtimeAnimatorController.animationClips
                .First(c => c.name == "EndSuperCrash").length;
        }
        
        animRunningTime = 0f;
        controller.Attack.SetDamageList(damages);
        controller.Animator.SetIntAniamtion(AnimatorHash.PlayerAnimation.AdditionalAttackID, skillId);
        controller.Animator.SetTriggerAnimation(AnimatorHash.PlayerAnimation.AdditionalAttack);
        controller.PlayerInputDisable();
        controller.Move.rb.gravityScale = 10f;
    }

    public override void LogicUpdate()
    {
        if (controller.Move.isGrounded)
        {
            animRunningTime += Time.deltaTime;
            if (!isEnded)
            {
                isEnded = true;
                controller.Animator.OnBoolParam(AnimatorHash.PlayerAnimation.SubGround);
            }
            AnimatorStateInfo curAnimInfo = controller.Animator.animator.GetCurrentAnimatorStateInfo(0);

            if (curAnimInfo.IsName("EndSuperCrash"))
            { 
                float animTime = curAnimInfo.normalizedTime;

                if (animTime >= 1.0f)
                {
                    controller.ChangeState<IdleState>();
                    return;
                }
            }
            if (animRunningTime >= endAnimationLength)
            {
                controller.ChangeState<IdleState>();
                return;
            }
        }
    }

}
