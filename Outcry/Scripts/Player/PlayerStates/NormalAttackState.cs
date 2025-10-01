using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NormalAttackState : NormalAttackSubState
{
    private float startStateTime;
    private float startAttackTime = 0.001f;
    private float startComboTime = 0.1f;
    private float comboTime = 0.2f; // 콤보타임 지나서 누르면 의미없음.
    private bool isComboInput = false;
    private float animRunningTime;
    
    public override async void Enter(PlayerController controller)
    {
        base.Enter(controller);
        startStateTime = Time.time;
        isComboInput = false;
        controller.Condition.canStaminaRecovery.Value = false;
        // AttackCount = 0 + NormalAttack Trigger On.
        controller.Animator.ClearBool();
        controller.Attack.SetDamage(controller.Data.normalAttackDamage[controller.Attack.AttackCount]);

        controller.Hitbox.AttackState = AttackState.NormalAttack;
        controller.Animator.SetTriggerAnimation(AnimatorHash.PlayerAnimation.NormalAttack);
        if (controller.Attack.AttackCount != controller.Attack.MaxAttackCount)
        {
            await EffectManager.Instance.PlayEffectByIdAndTypeAsync(PlayerEffectID.NormalAttackSound, EffectType.Sound, controller.gameObject);
        }

        else
        {
            await EffectManager.Instance.PlayEffectByIdAndTypeAsync(PlayerEffectID.LastNormalAttackSound, EffectType.Sound, controller.gameObject);
        }
        
        controller.Inputs.Player.Move.Disable();
        animRunningTime = 0f;
    }

    public override void HandleInput(PlayerController controller)
    {
        controller.Move.rb.velocity = Vector2.zero;
        // 키 입력이 필요
        if (Time.time - startStateTime <= comboTime)
        {
            if (controller.Inputs.Player.NormalAttack.triggered)
            {
                isComboInput = true;
                
            }
        }

        if (Time.time - startStateTime > startComboTime)
        {
            if (controller.Inputs.Player.NormalAttack.ReadValue<float>() > 0)
            {
                isComboInput = true;
            }
        }
        
        
        if (controller.Inputs.Player.SpecialAttack.triggered)
        {
            controller.isLookLocked = false;
            controller.ChangeState<SpecialAttackState>();
            return;
        }
        
        if (controller.Inputs.Player.Dodge.triggered)
        {
            controller.ChangeState<DodgeState>();
            return;
        }
        if (controller.Inputs.Player.Parry.triggered)
        {
            controller.ChangeState<StartParryState>();
            return;
        }
    }

    public override void LogicUpdate(PlayerController controller)
    {
        /*Debug.Log("Normal Attack State");*/
        // Normalization Time = 1이면 => 애니메이션이 끝난 상태
        // 애니메이션이 끝날 때까지 입력이 없으면 Idle로 넘어감
        // 입력이 있으면 (0.5초 이내로) 다시 NormalAttackState로 변경
        
        // 현재 진행 중인 애니메이션이 NormalAttack_(현재번호) 일 때

        AnimatorStateInfo curAnimInfo = controller.Animator.animator.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo nextAnimInfo = controller.Animator.animator.GetNextAnimatorStateInfo(0);

        if (!curAnimInfo.IsTag("NormalAttack")) return; // NormalAttack 애니메이션 상태 들어갔는지 확인하는 용도
        
        animRunningTime += Time.deltaTime;
        
        if (Time.time - startStateTime > startAttackTime)
        {
            if (curAnimInfo.IsTag("NormalAttack"))
            {

                float normalizedFullTime = curAnimInfo.normalizedTime;
                
                if (normalizedFullTime >= 1.0f)
                {
                    // 애니메이션 끝
                    if (isComboInput)
                    {
                        if (controller.Attack.AttackCount >= controller.Attack.MaxAttackCount)
                        {
                            controller.Animator.ClearInt();
                            controller.ChangeState<IdleState>();
                            return;
                        }
                        controller.Attack.AttackCount++;
                        controller.Animator.SetIntAniamtion(AnimatorHash.PlayerAnimation.NormalAttackCount, controller.Attack.AttackCount);
                        controller.ChangeState<NormalAttackState>();
                    }
                    else
                    {
                        controller.Animator.ClearInt();
                        controller.ChangeState<IdleState>();
                    }
                }
                
                
                /*if (animRunningTime >= attackAnimationLength)
                {
                    // 애니메이션 끝
                    if (isComboInput)
                    {
                        controller.Attack.AttackCount++;
                        controller.Animator.SetIntAniamtion(AnimatorHash.PlayerAnimation.NormalAttackCount, controller.Attack.AttackCount);
                        controller.ChangeState<NormalAttackState>();
                    }
                    else
                    {
                        controller.Animator.ClearInt();
                        controller.ChangeState<IdleState>();
                    }
                }*/
            }
            else if (nextAnimInfo.IsTag("NormalAttack"))
            {
                return;
            }
            else
            {
                controller.Animator.ClearInt();
                controller.ChangeState<IdleState>();
            }
        }
    }

    public override void Exit(PlayerController controller)
    {
        base.Exit(controller);
        isComboInput = false;
        controller.Condition.canStaminaRecovery.Value = true;
    }
}
