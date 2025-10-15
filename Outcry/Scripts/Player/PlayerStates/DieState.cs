using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieState : BasePlayerState
{
    public override eTransitionType ChangableStates { get; }

    public override void Enter(PlayerController controller)
    {
        controller.SetAnimation(AnimatorHash.PlayerAnimation.Die, true);
        controller.Inputs.Player.Dodge.Disable();
        controller.Inputs.Player.Move.Disable();
        controller.Inputs.Player.SpecialAttack.Disable();
        controller.Inputs.Player.NormalAttack.Disable();
        controller.Inputs.Player.Jump.Disable();
        controller.Inputs.Player.Parry.Disable();
    }

    public override void HandleInput(PlayerController controller)
    {
        
    }

    public override void LogicUpdate(PlayerController controller)
    {
        
    }

    public override void Exit(PlayerController controller)
    {
        
    }
}
