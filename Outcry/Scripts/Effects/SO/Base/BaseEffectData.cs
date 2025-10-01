using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class BaseEffectData : ScriptableObject
{
    [field: SerializeField] public int effectId { get; protected set; }
    [field: SerializeField] public EffectType effectType { get; protected set; }

    // CancellationToken 지원하는 UniTask 버전
    public abstract UniTask EffectAsync(EffectOrder order, CancellationToken token, GameObject target = null, Vector3 position = default(Vector3));
    
    //todo. UniTask 구현하면 하단부 지우기.
    // public abstract IEnumerator EffectCoroutine(EffectOrder order, GameObject target = null);
}
