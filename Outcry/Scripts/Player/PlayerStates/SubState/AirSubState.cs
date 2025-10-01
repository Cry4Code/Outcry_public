using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirSubState : IPlayerState
{
    public virtual void Enter(PlayerController controller)
    {
        controller.Animator.OnBoolParam(AnimatorHash.PlayerAnimation.SubAir);
    }

    public virtual void HandleInput(PlayerController controller)
    {
        
    }

    public virtual void LogicUpdate(PlayerController controller)
    {
        
    }

    public virtual void Exit(PlayerController controller)
    {
        controller.Animator.OffBoolParam(AnimatorHash.PlayerAnimation.SubAir);
    }
}
