using System;
using UnityEngine;

/// <summary>
/// 조건이 참일 떄만 child 실행, 거짓이면 즉시 Failure 반환, 조건 노드와 액션 노드가 필요합니다.
/// resetOnEnter 는 기본 True, 자식 노드를 리셋하지 않고 진행시 false로
/// </summary>
public class WhileTrueDecorator : DecoratorNode
{
    private readonly Node condition;

    private bool wasActive = false;
    private readonly bool resetOnEnter;

    public WhileTrueDecorator(Node condition, Node action, bool resetOnEnter = true)        
    {
        this.condition = condition ?? throw new ArgumentNullException(nameof(condition));
        this.child = action ?? throw new ArgumentNullException(nameof(action));
        this.resetOnEnter = resetOnEnter;

        this.nodeName = $"WhileTrue({condition.nodeName})";
    }

    public override NodeState Tick()
    {
        NodeState c = condition.Tick();

        // 조건이 깨지면 즉시 Failure 반환
        switch (c)
        {
            case NodeState.Success: // 컨디션이 Success면 자식 실행
                if (!wasActive && resetOnEnter)  // Success로 재진입한 첫 프레임에 자식 리셋
                {
                    child.Reset();
                }
                wasActive = true;
                return child.Tick();

            case NodeState.Running: // 컨디션이 평가 중
                Debug.LogWarning($"[{this.nodeName}] 컨디션 평가 중..."); // 컨디션 평가가 너무 오래 진행되는 경우에는 수정을 권장함
                wasActive = false;
                return NodeState.Running;

            case NodeState.Failure: // 컨디션이 Failure면 자식 리셋, Failure 반환
            default:
                if (wasActive)  // 실행이 됬었을 때는 자식 리셋
                {
                    child.Reset();
                }
                wasActive = false;
                return NodeState.Failure;
        }
    }

    public override void Reset()
    {
        wasActive = false;
        condition.Reset();
        base.Reset();   // child.Reset()
    }
}
