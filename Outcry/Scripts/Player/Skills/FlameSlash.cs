using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class FlameSlash : SkillBase
{
    // 프레임 쪼개기
    private const float ANIMATION_FRAME_RATE = 20f;
    // 앞으로 날라가는 기준 시간
    private const float RUN_FRONT_TIME = (1.0f / ANIMATION_FRAME_RATE) * 8f;

    private float[] attackTimes = 
    {
        (1.0f / ANIMATION_FRAME_RATE) * 8f,
        (1.0f / ANIMATION_FRAME_RATE) * 16f,
        (1.0f / ANIMATION_FRAME_RATE) * 22f,
    };

    private int attackTimesIndex = 0;
    
    // 날라가기 관련
    private bool isMoved = false;
    private Vector2 attackDirection;
    private float runDistance = 2f;
    private Vector2 startPos;
    private Vector2 targetPos;
  
    
    public override void Enter()
    {
        useSuccessed = false;
        // 발동 조건 체크 : 지상
        if (!controller.Move.isGrounded)
        {
            Debug.Log("[플레이어] 스킬 WindSlash는 지상에서만 사용 가능");
            controller.ChangeState<FallState>();
            return;
        }
        // 쿨타임 체크
        if (Time.time - lastUsedTime < cooldown)
        {
            Debug.Log("[플레이어] 스킬 WindSlash는 쿨타임 중");
            controller.ChangeState<IdleState>();
            return;
        }
        // 발동 조건 체크 : 스태미나
        if (!controller.Condition.TryUseStamina(needStamina))
        {
            Debug.Log("[플레이어] 스킬 WindSlash 사용을 위한 스태미나 부족");
            controller.ChangeState<IdleState>();
            return;
        }
        
        
        
        Debug.Log("[플레이어] 스킬 WindSlash 사용!");
        useSuccessed = true;
        attackTimesIndex = 0;
        controller.isLookLocked = false;
        controller.Move.ForceLook(controller.transform.localScale.x < 0);
        controller.isLookLocked = true;
        controller.Move.rb.velocity = Vector2.zero;
        controller.Condition.isCharge = false;
        controller.Condition.isSuperArmor = true;
        /*controller.Animator.ClearTrigger();
        controller.Animator.ClearInt();
        
        controller.Animator.ClearBool();*/
        animRunningTime = 0f;
        startStateTime = Time.time;
        controller.Attack.SetDamageList(damages);
        controller.Animator.SetIntAniamtion(AnimatorHash.PlayerAnimation.AdditionalAttackID, skillId);
        controller.Animator.SetTriggerAnimation(AnimatorHash.PlayerAnimation.AdditionalAttack);
        controller.PlayerInputDisable();
        

        attackDirection = controller.transform.localScale.x < 0 ? Vector2.left : Vector2.right;
        startPos = controller.transform.position;
        targetPos = startPos + (attackDirection * runDistance);
        isMoved = false;
    }

    public override void LogicUpdate()
    {
        animRunningTime += Time.deltaTime;

        if (animRunningTime >= RUN_FRONT_TIME && !isMoved)
        {
            isMoved = true;
            Vector2 direction = (targetPos - startPos).normalized;
            
            RaycastHit2D hit = Physics2D.Raycast(startPos, direction, runDistance, controller.Move.groundMask);

            if (hit.collider != null)
            {
                controller.Move.rb.MovePosition(hit.point - direction * 0.01f);
            }
            else
            {
                controller.Move.rb.MovePosition(targetPos);
            }
            
        }


        if (attackTimesIndex < attackTimes.Length)
        {
            if (animRunningTime >= attackTimes[attackTimesIndex])
            {
                attackTimesIndex++;
                EffectManager.Instance.PlayEffectByIdAndTypeAsync(skillId * 10, EffectType.Sound,
                    controller.gameObject).Forget();
                EffectManager.Instance.PlayEffectsByIdAsync(skillId, EffectOrder.Player, controller.gameObject,
                    new Vector3(2, 0.2f)).Forget();
            }    
        }
        
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

}
