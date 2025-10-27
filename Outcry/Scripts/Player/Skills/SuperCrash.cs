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
        base.Enter();        
        isEnded = false;
        if (endAnimationLength < 0)
        {
            endAnimationLength = controller.Animator.animator.runtimeAnimatorController.animationClips
                .First(c => c.name == "EndSuperCrash").length;
        }
        
        controller.Attack.SetDamageList(damages);
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
            
            controller.Move.rb.velocity = Vector2.zero;
            
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

    public override bool ConditionCheck()
    {
        // 발동 조건 체크 : 공중
        if (controller.Move.isGrounded)
        {
            return false;
        }
        // 쿨타임 체크
        if (Time.time - lastUsedTime < cooldown)
        {
            return false;
        }
        // 발동 조건 체크 : 스태미나
        if (!controller.Condition.TryUseStamina(needStamina))
        {
            return false;
        }

        return true;
    }
}
