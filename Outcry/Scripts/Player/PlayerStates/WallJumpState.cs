using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WallJumpState : AirSubState
{
    private float wallJumpStartTime;
    private float wallHoldAbleTime = 0.5f;
    private float startFallTime = 0.2f;
    
    private float animRunningTime = 0f;
    private float wallJumpAnimationLength;
    private float wallJumpSpeed = 5f;
    
    /*private Vector2 wallJumpDirection;
    private Vector2 targetPos;
    private Vector2 newPos;
    private Vector2 curPos;*/

    private Vector2 startPos;
    private Vector2 nextPos;
    private float wallJumpDirection;
    private float wallJumpMoveX = 8f;
    private float wallJumpMoveY = 3f;
    
    private float t;

    public override void Enter(PlayerController controller)
    {
        if (!controller.Condition.TryUseStamina(controller.Data.wallJumpStamina))
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
        base.Enter(controller);
        // 벽점할 때에는 벽 반대방향 봐야됨
        controller.Move.ForceLook(!controller.Move.lastWallIsLeft);
        controller.isLookLocked = true;
        // 벽점했으니까 강제로 벽 터치 취소
        controller.Animator.ClearBool(); // WallHold 끄려고
        controller.Move.rb.velocity = Vector2.zero;
        controller.Move.isWallTouched = false;
        controller.Animator.SetTriggerAnimation(AnimatorHash.PlayerAnimation.WallJump);
        controller.Move.isWallJumped = true;
        
        wallJumpStartTime = Time.time;
        
        animRunningTime = 0f;
        wallJumpAnimationLength =
            controller.Animator.animator.runtimeAnimatorController
                .animationClips.First(c => c.name == "StartWallJump").length
            + controller.Animator.animator.runtimeAnimatorController
                .animationClips.First(c => c.name == "WhileWallJump").length * 2f;
        
        /*wallJumpDirection = new Vector2((controller.Move.lastWallIsLeft ?  1.5f : -1.5f),1f).normalized;
        
        targetPos = startPos + (wallJumpDirection * 5f);
        // 시도하려는 것 : 포물선 운동을 강제로 만들기
        controller.Move.wallJumpStartY = startPos.y;*/

        startPos = controller.transform.position;
        wallJumpDirection = controller.Move.lastWallIsLeft ? 1 : -1;
    }

    public override void HandleInput(PlayerController controller) 
    {

        var moveInputs = controller.Inputs.Player.Move.ReadValue<Vector2>();

        if (controller.Inputs.Player.Jump.triggered && !controller.Move.isDoubleJump)
        {
            controller.ChangeState<DoubleJumpState>();
            return;
        }

        if(Time.time - wallJumpStartTime > wallHoldAbleTime && controller.Move.isWallTouched)
        {
            controller.ChangeState<WallHoldState>();
            return;
        }
        else
        {
            controller.Move.isWallTouched = false;
        }
        
        if (controller.Inputs.Player.NormalAttack.triggered && moveInputs.y < 0)
        {
            controller.isLookLocked = true;
            controller.ChangeState<DownAttackState>();
            return;
        }
        
        if (controller.Inputs.Player.NormalAttack.triggered && !controller.Attack.HasJumpAttack)
        {
            controller.isLookLocked = true;
            controller.ChangeState<NormalJumpAttackState>();
            return;
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
        if (controller.Inputs.Player.AdditionalAttack.triggered)
        {
            controller.ChangeState<AdditionalAttackState>();
            return;
        }
        
        

    }

    public override void LogicUpdate(PlayerController controller)
    {
        animRunningTime += Time.deltaTime;
        
        t = animRunningTime / wallJumpAnimationLength;
        
        float x = startPos.x + controller.Move.jumpXCurve.Evaluate(t) * wallJumpDirection * wallJumpMoveX;
        float y = startPos.y + controller.Move.jumpYCurve.Evaluate(t) * wallJumpMoveY;

        nextPos = new Vector2(x, y);

        Vector2 direction = (nextPos - (Vector2)controller.transform.position).normalized;
        float distance = Vector2.Distance(controller.transform.position, nextPos);
        
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

        controller.transform.position = nextPos;

        if (t >= 1f)
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

        /*
         // 개 똥 버전
        newPos = Vector2.Lerp(startPos, targetPos, t);
        
        curPos = controller.transform.position;
        
        
        // 현재 위치에서 이동할 위치만큼 선 하나 그어서, 그게 벽에 닿으면 벽 끝에까지만 가고 상태 바뀌게함
        Vector2 direction = (newPos - curPos).normalized;
        float distance = Vector2.Distance(curPos, newPos);
        
        RaycastHit2D hit =
            Physics2D.Raycast(controller.transform.position, direction, distance, controller.Move.groundMask);
        
        if (hit.collider != null)
        {
            controller.Move.rb.MovePosition(hit.point - direction * 0.01f);
            if (controller.Move.isGrounded) controller.ChangeState<IdleState>();
            else controller.ChangeState<FallState>();
            return;
        }
        
        
        controller.Move.rb.MovePosition(newPos);

        
        if (Vector2.Distance(newPos, targetPos) < 0.01f)
        {
            if (controller.Move.isGrounded)
            {
                controller.ChangeState<IdleState>();
                return;
            }
            controller.ChangeState<WallJumpFallState>();
            
        }  */
        
        if (controller.Move.isGrounded)
        {
            controller.ChangeState<IdleState>();
            return;
        }
    }

    public override void Exit(PlayerController controller) 
    {
        base.Exit(controller);
        controller.isLookLocked = false;
    }
}
