using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DownAttackSubState : AirSubState
{
    public override void Enter(PlayerController controller)
    {
        controller.Animator.OnBoolParam(AnimatorHash.PlayerAnimation.SubDownAttack);
    }

    public override void Exit(PlayerController controller)
    {
        controller.Animator.OffBoolParam(AnimatorHash.PlayerAnimation.SubDownAttack);
    }
}
