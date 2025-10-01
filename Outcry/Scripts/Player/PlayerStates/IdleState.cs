using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class IdleState : GroundSubState
{
    public override void Enter(PlayerController controller)
    {
        // 애니메이션 설정 
        // player.SetAnimation("Idle");
        base.Enter(controller);
        
        controller.Move.Stop();
        controller.Move.ChangeGravity(false);
        controller.Condition.canStaminaRecovery.Value = true;
        controller.Attack.ClearAttackCount();
        controller.Animator.ClearTrigger();
        controller.Animator.ClearInt();
        controller.Animator.ClearBool();
        
        controller.Animator.OnBoolParam(AnimatorHash.PlayerAnimation.Idle);
        controller.Inputs.Player.Move.Enable();
    }

    public override async void HandleInput(PlayerController controller)
    {
        AnimatorStateInfo curAnimInfo = controller.Animator.animator.GetCurrentAnimatorStateInfo(0);
        if (curAnimInfo.IsName("Idle"))
        {
            controller.isLookLocked = false;
        }
        else
        {
            controller.Move.Stop();
            return;
        }
        
        var input = controller.Inputs.Player.Move.ReadValue<Vector2>();

        if (input.y < 0 && controller.Inputs.Player.Jump.triggered && controller.Move.isGrounded )
        {
            Debug.Log("[플레이어] 아래 점프 입력됨");
            RaycastHit2D hit =
                Physics2D.Raycast((Vector2)controller.transform.position, Vector2.down,  controller.halfPlayerHeight + 1f, LayerMask.GetMask("Ground"));
            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Platform"))
                {
                    Debug.Log("[플레이어] 아래 점프 입력된 후 플랫폼 발견됨");
                    await TurnOffPlatformCollider(hit.collider);
                    controller.ChangeState<FallState>();
                }
            }
        }
        
        if (controller.Inputs.Player.NormalAttack.triggered)
        {
            controller.isLookLocked = true;
            controller.ChangeState<NormalAttackState>();
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

        if (controller.Inputs.Player.Parry.triggered)
        {
            controller.ChangeState<StartParryState>();
            return;
        }

        if (controller.Inputs.Player.Potion.triggered && controller.Condition.potionCount > 0)
        {
            controller.ChangeState<PotionState>();
            return;
        }
        
        
        if (controller.Inputs.Player.Jump.triggered 
            && controller.Move.isGrounded 
            && !controller.Move.isGroundJump 
            && !controller.Move.isWallTouched)
        {
            // Debug.Log("Jump Key Input");
            controller.ChangeState<JumpState>();
            return;
        }
        if (input.x != 0)
        {
            controller.Move.ForceLook(input.x < 0);
            controller.isLookLocked = true;
            controller.ChangeState<MoveState>();
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
        

        if (controller.Move.isDodged)
        {
            controller.Move.Stop();
            controller.Move.isDodged = false;
        }
        
        if (controller.Move.rb.velocity.y < 0) controller.ChangeState<FallState>();
    }

    public override void Exit(PlayerController controller)
    {
        base.Exit(controller);
        controller.Animator.OffBoolParam(AnimatorHash.PlayerAnimation.Idle);
    }
    async Task TurnOffPlatformCollider(Collider2D collider)
    {
        collider.enabled = false;
        await Task.Delay(500);
        collider.enabled = true;
    }
    
}