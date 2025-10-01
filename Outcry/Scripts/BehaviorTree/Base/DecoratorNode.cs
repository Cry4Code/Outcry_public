using System;
using UnityEngine;

[Serializable]
public abstract class DecoratorNode : Node
{ 
    [SerializeReference] protected Node child;

    public virtual void SetChild(Node child)
    {
        this.child = child;
    }

    public override void Reset()
    {
        child?.Reset();
    }
}
