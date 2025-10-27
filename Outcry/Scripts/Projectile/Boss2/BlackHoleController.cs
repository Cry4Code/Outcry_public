using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class BlackHoleController : ProjectileBase
{
    public override void Init(int damage, bool isCountable = true)
    {
        base.Init(damage, isCountable);
        EffectManager.Instance.PlayEffectByIdAndTypeAsync(Stage2BossEffectID.BlackHole * 10 + 1, EffectType.Sound, gameObject).Forget();
        RequestRelease();
    }

    protected override void OnPrepareRelease()
    {
        if (animator) animator.SetBool(AnimatorHash.ProjectileParameter.Triggered, false);
    }
}
