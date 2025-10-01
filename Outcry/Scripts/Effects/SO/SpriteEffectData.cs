using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "SpriteEffectData", menuName = "ScriptableObjects/Effects/SpriteEffectData", order = 6)]
public class SpriteEffectData : BaseEffectData
{
    [field: SerializeField] public float Duration { get; private set; }
    [field: SerializeField] public string path { get; private set; }
#if UNITY_EDITOR
    [Header("Editor Only")]
    public GameObject SpritePrefab;
    private void OnValidate()
    {
        this.effectType = EffectType.Sprite;
        if (SpritePrefab != null)
        {
            path = AssetDatabase.GetAssetPath(SpritePrefab).Replace(AddressablePaths.ROOT, "");
            SpritePrefab = null;
        }
    }
#endif
    public override async UniTask EffectAsync(EffectOrder order, CancellationToken token, GameObject target = null, Vector3 position = default(Vector3))
    {
        Debug.LogWarning($"[이펙트: UniTask (ID : {effectId} TYPE: {effectType})] EffectAsync operation try entered.");
        if (target == null)
            target = EffectManager.Instance.gameObject;

        GameObject effectInstance = null; //finally에서 참조하기 위해 미리 선언
        
        Vector3 adjustedPosition = position;
        Vector3 scale = target.transform.localScale;
        adjustedPosition.x *= Mathf.Sign(scale.x);
        adjustedPosition.y *= Mathf.Sign(scale.y);
        adjustedPosition.z *= Mathf.Sign(scale.z);
        
        try
        {
            effectInstance = await ObjectPoolManager.Instance.GetObjectAsync(path, target.transform, adjustedPosition);
            await UniTask.Delay((int)(Duration * 1000), cancellationToken: token);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning($"[이펙트: UniTask (ID : {effectId})] EffectAsync operation cancelled");
        }
        catch (Exception e)
        {
            Debug.LogError($"[이펙트: UniTask (ID : {effectId})] EffectAsync operation failed: {e}");
        }
        finally
        {
            if (effectInstance != null)
            {
                ObjectPoolManager.Instance.ReleaseObject(path, effectInstance);
                Debug.Log($"이펙트: UniTask (ID : {effectId}) EffectAsync operation completed and object {path} released.");
            }
        }
    }
}
