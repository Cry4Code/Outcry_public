using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    public void PlayEffect(int effectId)
    { 
        EffectManager.Instance.PlayEffectsByIdAsync(effectId, EffectOrder.Monster, gameObject).Forget();
    }

    public void PlayEffectSound(int effectId)
    {
        EffectManager.Instance.PlayEffectByIdAndTypeAsync(effectId, EffectType.Sound, gameObject).Forget();
    }
}
