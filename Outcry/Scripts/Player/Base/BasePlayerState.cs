using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasePlayerState
{
    public abstract eTransitionType ChangableStates { get; }
    
    public abstract void Enter(PlayerController controller); // 상태 변화했을 때 돌아감 
    public abstract void HandleInput(PlayerController controller); // 입력에 따른 상태 전환 등을 처리
    public abstract void LogicUpdate(PlayerController controller); // 실제 로직이 돌아가는 부분
    public abstract void Exit(PlayerController controller); // 상태에서 나갈 때 돌아감
    
    public bool CanChangeTo(eTransitionType next)
    {
        return ChangableStates.HasFlag(next);
    }

    public void TryChangeState(eTransitionType next, PlayerController controller)
    {
        if (ChangableStates.HasFlag(next))
        {
            System.Type type = controller.stringStateTypes[next.ToString()];
            controller.ChangeState(type);
        }
    }
}
