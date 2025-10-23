using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedProjectileController : ProjectileBase
{
    public override void Init(int damage, bool isCountable = true)
    {
        base.Init(damage, isCountable);
        RequestRelease();
    }

    protected override void OnPrepareRelease()
    {
        // if (animator) animator.SetBool(AnimatorHash.ProjectileParameter.Triggered, false);
    }
}