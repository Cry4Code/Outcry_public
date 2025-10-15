using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Unbreakable : SkillBase
{
    // 애니메이션 재생 끝나면 무적처리 해주면 됨
    
    public override void Enter()
    {
        useSuccessed = false;
        // 발동 조건 체크 : 지상
        if (!controller.Move.isGrounded)
        {
            Debug.Log("[플레이어] 스킬 Unbreakable는 지상에서만 사용 가능");
            controller.ChangeState<FallState>();
            return;
        }
        
        // 쿨타임 체크
        if (Time.time - lastUsedTime < cooldown)
        {
            Debug.Log("[플레이어] 스킬 Unbreakable는 쿨타임 중");
            controller.ChangeState<FallState>();
            return;
        }
        Debug.Log("[플레이어] 스킬 Unbreakable 사용!");
        useSuccessed = true;
        // 시점 고정
        controller.isLookLocked = false;
        controller.Move.ForceLook(controller.transform.localScale.x < 0);
        controller.isLookLocked = true;
        controller.Condition.isCharge = true;
        
        animRunningTime = 0f;
        controller.Animator.SetIntAniamtion(AnimatorHash.PlayerAnimation.AdditionalAttackID, skillId);
        controller.Animator.SetTriggerAnimation(AnimatorHash.PlayerAnimation.AdditionalAttack);
        controller.PlayerInputDisable();
    }


    public override void LogicUpdate()
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
                    controller.Condition.SetInvincible(duration);
                    if (controller.Move.isGrounded) controller.ChangeState<IdleState>();
                    else controller.ChangeState<FallState>();
                    return;
                }
            }

            if (animRunningTime >= animationLength)
            {
                controller.Condition.SetInvincible(duration);
                if (controller.Move.isGrounded) controller.ChangeState<IdleState>();
                else controller.ChangeState<FallState>();
                return;
            }
                
        }
    }

}
