using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundSubState : IPlayerState
{
    public virtual void Enter(PlayerController controller)
    {
        controller.Animator.OnBoolParam(AnimatorHash.PlayerAnimation.SubGround);
    }

    public virtual void HandleInput(PlayerController controller)
    {
        
    }

    public virtual void LogicUpdate(PlayerController controller)
    {
        
    }

    public virtual void Exit(PlayerController controller)
    {
        controller.Animator.OffBoolParam(AnimatorHash.PlayerAnimation.SubGround);
    }
}
