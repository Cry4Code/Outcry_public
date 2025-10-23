using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class StoneController : ProjectileBase
{
    public override void Init(int damage, bool isCountable = true)
    {
        base.Init(damage, isCountable);
        EffectManager.Instance.PlayEffectByIdAndTypeAsync(1030040, EffectType.Sound).Forget();
        RequestRelease(callback: () =>
        {
            EffectManager.Instance.PlayEffectByIdAndTypeAsync(1030041, EffectType.Sound).Forget();
        });
    }

    protected override void OnPrepareRelease()
    {
        if (animator) animator.SetBool(AnimatorHash.ProjectileParameter.Triggered, false);
    }
}
