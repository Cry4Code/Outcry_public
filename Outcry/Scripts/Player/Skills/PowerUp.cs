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
        useSuccessed = false;
        isBuffed = false;
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
        useSuccessed = true;
        // 시점 고정
        controller.PlayerInputDisable();
        controller.Move.rb.velocity = Vector2.zero;
        
        controller.isLookLocked = true;
        controller.Move.ForceLook(controller.transform.localScale.x < 0);
        controller.Condition.isCharge = false;
        controller.Condition.isSuperArmor = true;
        
        animRunningTime = 0f;
        
        controller.Animator.SetIntAniamtion(AnimatorHash.PlayerAnimation.AdditionalAttackID, skillId);
        controller.Animator.SetTriggerAnimation(AnimatorHash.PlayerAnimation.AdditionalAttack);
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

            /*if (animRunningTime >= animationLength)
            {
                controller.Attack.BuffDamage(buffValue, duration);
                await EffectManager.Instance.PlayEffectByIdAndTypeAsync(skillId, EffectType.Particle, controller.gameObject,
                    Vector3.down * 1.2f
                );
                if (controller.Move.isGrounded) controller.ChangeState<IdleState>();
                else controller.ChangeState<FallState>();
                return;
            }*/
                
        }
    }

    public async override void Exit()
    {
        base.Exit();
        if (isBuffed)
        {
            lastUsedTime = Time.time;
            await EffectManager.Instance.PlayEffectByIdAndTypeAsync(skillId, EffectType.Particle, controller.gameObject,
                Vector3.down * 1.2f
            );
        }
    }
}
