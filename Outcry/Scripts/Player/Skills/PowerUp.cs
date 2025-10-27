using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PowerUp : SkillBase
{
    // 애니메이션 재생 끝나면 공격력 증가해주면 됨
    private bool isBuffed = false;
    
    public async override void Enter()
    {
        isBuffed = false;

        base.Enter();
        
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
                    isBuffed = true;
                    controller.Attack.BuffDamage(buffValue, duration);
                    if (controller.Move.isGrounded) controller.ChangeState<IdleState>();
                    else controller.ChangeState<FallState>();
                    return;
                }
            }
                
        }
    }

    public override bool ConditionCheck()
    {
        // 발동 조건 체크 : 지상
        if (!controller.Move.isGrounded)
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

    public async override void Exit()
    {
        base.Exit();
        if (isBuffed)
        {
            await EffectManager.Instance.PlayEffectByIdAndTypeAsync(skillId, EffectType.Particle, controller.gameObject,
                Vector3.down * 1.2f
            );
        }
    }
}
