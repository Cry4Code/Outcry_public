using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpeedSting : SkillBase
{
    public override void Enter()
    {
        base.Enter();        
        if (!useSuccessed) return;
        
        startStateTime = Time.time;
        controller.Attack.SetDamageList(damages);
    }


    public override void LogicUpdate()
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

            if (animRunningTime >= animationLength)
            {
                if (controller.Move.isGrounded) controller.ChangeState<IdleState>();
                else controller.ChangeState<FallState>();
                return;
            }
        }
    }

    public override bool ConditionCheck()
    {
        if (!base.ConditionCheck()) return false;
        // 발동 조건 체크 : 지상
        if (!controller.Move.isGrounded)
        {
            return false;
        }

        return true;
    }
}
