using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class IdleState : GroundSubState
{
    public override eTransitionType ChangableStates =>
        eTransitionType.NormalAttackState | eTransitionType.FallState |  eTransitionType.SpecialAttackState | 
        eTransitionType.DodgeState | eTransitionType.StartParryState | eTransitionType.PotionState | 
        eTransitionType.JumpState |  eTransitionType.MoveState | eTransitionType.AdditionalAttackState;
    public override void Enter(PlayerController controller)
    {
        // 애니메이션 설정 
        // player.SetAnimation("Idle");
        base.Enter(controller);
        
        controller.Move.Stop();
        controller.Attack.successParry = false;
        controller.Condition.canStaminaRecovery.Value = true;
        controller.Attack.HasJumpAttack = false;
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
                    TurnOffPlatformCollider(hit.collider).Forget();
                    controller.ChangeState<FallState>();
                }
            }
        }

        if (input.y > 0)
        {
            Debug.Log($"[플레이어] 상호작용 시도");
            controller.Move.TryInteract();
        }
        
        base.HandleInput(controller);
    }
 
    public override void LogicUpdate(PlayerController controller) 
    {
        if (controller.Move.isDodged)
        {
            controller.Move.Stop();
            controller.Move.isDodged = false;
        }

        if (controller.Move.rb.velocity.y < 0)
        {
            controller.ChangeState<FallState>();
            return;
        }
    }

    public override void Exit(PlayerController controller)
    {
        base.Exit(controller);
        controller.Animator.OffBoolParam(AnimatorHash.PlayerAnimation.Idle);
    }
    async UniTaskVoid TurnOffPlatformCollider(Collider2D collider)
    {
        collider.enabled = false;
        await UniTask.Delay(500);
        collider.enabled = true;
    }
    
}