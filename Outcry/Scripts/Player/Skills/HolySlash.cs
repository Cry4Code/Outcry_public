using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HolySlash : SkillBase
{
  public override void Enter()
    {
        useSuccessed = false;
        // 발동 조건 체크 : 지상
        if (!controller.Move.isGrounded)
        {
            Debug.Log("[플레이어] 스킬 HolySlash는 지상에서만 사용 가능");
            controller.ChangeState<FallState>();
            return;
        }
        // 쿨타임 체크
        if (Time.time - lastUsedTime < cooldown)
        {
            Debug.Log("[플레이어] 스킬 HolySlash는 쿨타임 중");
            controller.ChangeState<IdleState>();
            return;
        }
        // 발동 조건 체크 : 스태미나
        if (!controller.Condition.TryUseStamina(needStamina))
        {
            Debug.Log("[플레이어] 스킬 HolySlash 사용을 위한 스태미나 부족");
            controller.ChangeState<IdleState>();
            return;
        }
        Debug.Log("[플레이어] 스킬 HolySlash 사용!");
        useSuccessed = true;
        controller.isLookLocked = false;
        controller.Move.ForceLook(controller.transform.localScale.x < 0);
        controller.isLookLocked = true;
        controller.Move.rb.velocity = Vector2.zero;
        controller.Condition.isCharge = true;
        
        animRunningTime = 0f;
        startStateTime = Time.time;
        controller.Attack.SetDamageList(damages);
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
                Debug.Log($"[HolySlash] NormalizedTime = {animTime}");
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
}
