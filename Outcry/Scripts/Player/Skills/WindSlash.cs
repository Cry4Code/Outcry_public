using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class WindSlash : SkillBase
{
    // 프레임 쪼개기
    private const float ANIMATION_FRAME_RATE = 20f;
    // 앞으로 날라가는 기준 시간
    private const float RUN_FRONT_TIME = (1.0f / ANIMATION_FRAME_RATE) * 8f;
    
    // 날라가기 관련
    private bool isMoved = false;
    private Vector2 attackDirection;
    private float runDistance = 2f;
    private Vector2 startPos;
    private Vector2 targetPos;

    // 시간 쪼개기 
    private float startStateTime;
    private float startAttackTime = 0.01f;
    private float animRunningTime = 0f;
    private float attackAnimationLength;
    
    // 쿨타임
    private float lastUsedTime = float.MinValue;


    
    
    public override void Enter(PlayerController controller)
    {
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
        controller.isLookLocked = false;
        controller.Move.ForceLook(controller.transform.localScale.x < 0);
        controller.isLookLocked = true;
        controller.Move.rb.velocity = Vector2.zero;
        controller.Condition.isCharge = true;
        /*controller.Animator.ClearTrigger();
        controller.Animator.ClearInt();
        
        controller.Animator.ClearBool();*/
        animRunningTime = 0f;
        startStateTime = Time.time;
        attackAnimationLength = 
            controller.Animator.animator.runtimeAnimatorController
                .animationClips.First(c => c.name == "WindSlash").length;
        controller.Attack.SetDamageList(damages);
        controller.Animator.SetIntAniamtion(AnimatorHash.PlayerAnimation.AdditionalAttackID, skillId);
        controller.Animator.SetTriggerAnimation(AnimatorHash.PlayerAnimation.AdditionalAttack);
        controller.PlayerInputDisable();
        

        attackDirection = controller.transform.localScale.x < 0 ? Vector2.left : Vector2.right;
        startPos = controller.transform.position;
        targetPos = startPos + (attackDirection * runDistance);
        isMoved = false;
    }

    public override void HandleInput(PlayerController controller)
    {

    }

    public override void LogicUpdate(PlayerController controller)
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

            if (animRunningTime >= attackAnimationLength)
            {
                if (controller.Move.isGrounded) controller.ChangeState<IdleState>();
                else controller.ChangeState<FallState>();
                return;
            }
                
        }
    }

    public override void Exit(PlayerController controller)
    {
        Debug.Log("[플레이어] 스킬 WindSlash 종료");
        controller.PlayerInputEnable();
        lastUsedTime = Time.time;
        controller.Condition.isCharge = false;
    }
}
