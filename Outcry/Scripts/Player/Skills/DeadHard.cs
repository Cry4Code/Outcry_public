using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeadHard : SkillBase
{
    // 프레임 쪼개기
    private const float ANIMATION_FRAME_RATE = 20f;
    
    // 십자가 생기는 시간
    private float invincibleStartTime = (1.0f / ANIMATION_FRAME_RATE) * 2f;
    
    // 애니메이션 재생 끝나면 무적처리 해주면 됨
    private bool isBuffed = false;
    
    
    public async override void Enter()
    {
        isBuffed = false;

        base.Enter();

        if (!useSuccessed) return;
        
        await EffectManager.Instance.PlayEffectsByIdAsync(PlayerEffectID.HolySlash , EffectOrder.Player, controller.gameObject,
            Vector3.up * 0.2f
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

            if (animRunningTime >= invincibleStartTime && !isBuffed)
            {
                isBuffed = true;
                controller.Condition.DeadHard(duration);
                
            }
        }
    }

    public override bool ConditionCheck()
    {
        if (!base.ConditionCheck()) return false;
        // 발동 조건 체크 : 지상
        if (!controller.Move.isGrounded)
        {
            Debug.Log("[플레이어] 스킬 Unbreakable는 지상에서만 사용 가능");
            return false;
        }

        return true;
    }

    public async override void Exit()
    {
        base.Exit();
        if (isBuffed)
        {
            
            await EffectManager.Instance.PlayEffectByIdAndTypeAsync(skillId, EffectType.Particle, controller.gameObject,
                Vector3.down * 1f
            );
        }
    }

}
