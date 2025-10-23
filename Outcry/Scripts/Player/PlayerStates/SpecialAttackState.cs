using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SpecialAttackState : BasePlayerState
{
    private float startStateTime;
    private float startAttackTime = 0.01f;
    private float animRunningTime = 0f;
    private float attackAnimationLength;
    private float specialAttackSpeed = 10f;
    private Vector2 specialAttackDirection;
    private float specialAttackDistance = 7f;
    private float justTiming = 0.05f;
    private Vector2 startPos;
    private Vector2 targetPos;
    private Vector2 newPos;
    private Vector2 curPos;
    private float cursorAngle = 0f;

    private Vector2 lastSpeed;
    private bool hasLastSpeed;
    
    private bool isSpecialAttacking = false;


    private float t;
    public override eTransitionType ChangableStates { get; }

    public async override void Enter(PlayerController controller)
    {
        isSpecialAttacking = false;
        if (!controller.Condition.TryUseStamina(controller.Data.specialAttackStamina))
        {
            if (controller.Move.isGrounded)
            {
                controller.ChangeState<IdleState>();
                return;
            }
            else
            {
                controller.ChangeState<FallState>();
                return;
            }
        }
        isSpecialAttacking = true;
        controller.isLookLocked = false;
        controller.Move.ForceLook(CursorManager.Instance.mousePosition.x - controller.transform.position.x < 0);
        controller.Move.rb.velocity = Vector2.zero;
        controller.Animator.ClearTrigger();
        controller.Animator.ClearInt();
        controller.Animator.ClearBool();
        controller.Inputs.Player.Move.Disable();
        controller.Hitbox.AttackState = AttackState.SpecialAttack;
        animRunningTime = 0f;
        attackAnimationLength = 
            controller.Animator.animator.runtimeAnimatorController
                .animationClips.First(c => c.name == "SpecialAttack").length;
        specialAttackDirection = (CursorManager.Instance.mousePosition - controller.transform.position).normalized;
        controller.Attack.SetDamage(controller.Data.specialAttackDamage);
        controller.Attack.isStartJustAttack = true;
        controller.Condition.isSuperArmor = true;
        EffectManager.Instance.PlayEffectByIdAndTypeAsync(PlayerEffectID.SpecialAttack, EffectType.Sound,
            controller.gameObject).Forget();
        controller.Animator.SetTriggerAnimation(AnimatorHash.PlayerAnimation.SpecialAttack);
        
        
        controller.isLookLocked = true;
        hasLastSpeed = false;
        
        // 마우스 바라보는 방향으로 캐릭터 돌리기
        // 1. 마우스를 바라보는 각도 구하기
        cursorAngle = Mathf.Atan2(specialAttackDirection.y, specialAttackDirection.x) *  Mathf.Rad2Deg;
        
        // 2. 그 각도대로 돌리기
        if (specialAttackDirection.x > 0)
        {
            controller.transform.rotation = Quaternion.Euler(0, 0, cursorAngle);
        }
        else
        {
            controller.transform.rotation = Quaternion.Euler(0, 0, -180f+cursorAngle);
        }
        
        startPos = controller.transform.position;
        targetPos = startPos + (specialAttackDirection * specialAttackDistance);
        controller.Attack.justAttackStartPosition = startPos;
    }

    public override void HandleInput(PlayerController controller)
    {
        
    }

    public override void LogicUpdate(PlayerController controller)
    {
        // 멈춤 상태. 별로면 나중에 이부분 빼면 됨
        if (controller.Animator.animator.speed < 1)
        {
            if (!hasLastSpeed)
            {
                hasLastSpeed = true;
                lastSpeed = controller.Move.rb.velocity;
            }
            controller.Move.rb.velocity = Vector2.zero;
            return;
        }
        
        animRunningTime += Time.deltaTime;

        if (animRunningTime >= justTiming)
        {
            controller.Attack.isStartJustAttack = false;
        }

        if (hasLastSpeed)
        {
            controller.Move.rb.velocity = lastSpeed;
            hasLastSpeed = false;
        }
        
        t = animRunningTime / attackAnimationLength;

        newPos = Vector2.MoveTowards(startPos, targetPos, t * specialAttackSpeed);

        curPos = controller.transform.position;
        
        
        // 현재 위치에서 이동할 위치만큼 선 하나 그어서, 그게 벽에 닿으면 벽 끝에까지만 가고 상태 바뀌게함
        Vector2 direction = (newPos - curPos).normalized;
        float distance = Vector2.Distance(curPos, newPos);
        
        RaycastHit2D hit =
            Physics2D.Raycast(controller.transform.position, direction, distance, controller.Move.groundMask);
        
        if (hit.collider != null)
        {
            if (!hit.collider.CompareTag("Platform"))
            {
                controller.Move.rb.MovePosition(hit.point - direction * 0.01f);
                if (controller.Move.isGrounded) controller.ChangeState<IdleState>();
                else controller.ChangeState<FallState>();
                return;    
            }
        }
        
        
        controller.Move.rb.MovePosition(newPos);
        
        if (Vector2.Distance(newPos, targetPos) < 0.01f)
        {
            controller.Move.rb.velocity = Vector2.zero;
            if (controller.Move.isGrounded) controller.ChangeState<IdleState>();
            else controller.ChangeState<FallState>();
            return;
        }
        
        if (Time.time - startStateTime > startAttackTime)
        {
            AnimatorStateInfo curAnimInfo = controller.Animator.animator.GetCurrentAnimatorStateInfo(0);

            if (curAnimInfo.IsName("SpecialAttack"))
            { 
                float animTime = curAnimInfo.normalizedTime;

                if (animTime >= 1.0f)
                {
                    if (controller.Move.isGrounded) controller.ChangeState<IdleState>();
                    else controller.ChangeState<FallState>();
                    return;
                }
            }

            /*
            if (animRunningTime >= attackAnimationLength)
            {
                if (controller.Move.isGrounded) controller.ChangeState<IdleState>();
                else controller.ChangeState<FallState>();
                return;
            }*/
                
        }
    }

    public override void Exit(PlayerController controller)
    {
        controller.isLookLocked = false;
        controller.Inputs.Player.Move.Enable();
        controller.Condition.isSuperArmor = false;
        controller.transform.rotation = Quaternion.Euler(0, 0, 0);
        int stageId = StageManager.Instance.CurrentStageData.Stage_id;
        if (isSpecialAttacking &&  stageId != StageID.Village)
        {
            UGSManager.Instance.LogDoAction(stageId, PlayerEffectID.SpecialAttack);
        }
    }
}
