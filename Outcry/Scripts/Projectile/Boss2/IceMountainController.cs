using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceMountainController : ProjectileBase
{
    public override void Init(int damage, bool isCountable = true)
    {
        base.Init(damage, isCountable);
        EffectManager.Instance.PlayEffectByIdAndTypeAsync(103406, EffectType.Sound).Forget();
        RequestRelease(callback: () =>
        {
            EffectManager.Instance.PlayEffectByIdAndTypeAsync(103406, EffectType.Sound).Forget();
        });
    }

    protected override void OnPrepareRelease()
    {
        if (animator) animator.SetBool(AnimatorHash.ProjectileParameter.Triggered, false);
    }
}
