using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HolySlash : SkillBase
{
    // 프레임 쪼개기
    private const float ANIMATION_FRAME_RATE = 20f;
    // 십자가 생기는 시간
    private float crossAnimationTime = (1.0f / ANIMATION_FRAME_RATE) * 0f;

    private bool isAnimationPlayed = false;
    
    
  public async override void Enter()
  { 
        base.Enter();
        if (!useSuccessed) return;
        
        isAnimationPlayed = false;
       
        startStateTime = Time.time;
        controller.Attack.SetDamageList(damages);
        
        await EffectManager.Instance.PlayEffectsByIdAsync(skillId, EffectOrder.Player, controller.gameObject,
            Vector3.right * 0.5f + Vector3.up * 0.2f
            );
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
