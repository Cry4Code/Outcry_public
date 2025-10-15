using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalJumpAttackSubState : AirSubState
{
    public override void Enter(PlayerController controller)
    {
        controller.Animator.OnBoolParam(AnimatorHash.PlayerAnimation.SubNormalJumpAttack);
    }

    public override void LogicUpdate(PlayerController controller)
    {
        
    }

    public override void Exit(PlayerController controller)
    {
        controller.Animator.OffBoolParam(AnimatorHash.PlayerAnimation.SubNormalJumpAttack);
    }
}
