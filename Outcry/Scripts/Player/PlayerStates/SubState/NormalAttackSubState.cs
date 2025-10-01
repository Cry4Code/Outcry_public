using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalAttackSubState : GroundSubState
{
    public override void Enter(PlayerController controller)
    {
        base.Enter(controller);
        controller.Animator.OnBoolParam(AnimatorHash.PlayerAnimation.SubNormalAttack);
    }

    public override void HandleInput(PlayerController controller)
    {
        
    }

    public override void LogicUpdate(PlayerController controller)
    {
        
    }

    public override void Exit(PlayerController controller)
    {
        base.Exit(controller);
        controller.Animator.OffBoolParam(AnimatorHash.PlayerAnimation.SubNormalAttack);
    }
}
