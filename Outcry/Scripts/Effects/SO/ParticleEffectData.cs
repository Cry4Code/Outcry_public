using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "ParticleEffectData", menuName = "ScriptableObjects/Effects/ParticleEffectData", order = 5)]
public class ParticleEffectData : BaseEffectData
{
    [field: SerializeField] public float Duration { get; private set; }
    [field: SerializeField] public string path { get; private set; }
#if UNITY_EDITOR
    [Header("Editor Only")] 
    public GameObject particlePrefab;
    private void OnValidate()
    {
        this.effectType = EffectType.Particle;
        if (particlePrefab != null)
        {
            path = AssetDatabase.GetAssetPath(particlePrefab).Replace(AddressablePaths.ROOT, "");
            particlePrefab = null;
        }
    }
#endif
    public override UniTask EffectAsync(EffectOrder order, CancellationToken token, GameObject target = null, Vector3 position = default(Vector3))
    {
        return UniTask.CompletedTask;
    }
}
