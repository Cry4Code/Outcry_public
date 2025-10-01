using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "ScreenEffectData", menuName = "ScriptableObjects/Effects/ScreenEffectData", order = 2)]
public class ScreenEffectData : BaseEffectData
{
    [field: SerializeField] public float Duration { get; private set; }
    [field: SerializeField] public string path { get; private set; }
#if UNITY_EDITOR
    [Header("Editor Only")]
    public GameObject ImageUIPrefab;
    private void OnValidate()
    {
        this.effectType = EffectType.ScreenUI;
        if (ImageUIPrefab != null)
        {
            path = AssetDatabase.GetAssetPath(ImageUIPrefab).Replace(AddressablePaths.ROOT, "");
            ImageUIPrefab = null;
        }
    }
#endif


    /// <summary>
    /// 
    /// </summary>
    /// <param name="order"></param>
    /// <param name="token"></param>
    /// <param name="target">기본 값은 EffectManager.Instance.EffectCanvas.transform 입니다.</param>
    public override async UniTask EffectAsync(EffectOrder order, CancellationToken token, GameObject target = null, Vector3 position = default(Vector3))
    {
        if (target == null)
        {
            target = EffectManager.Instance.EffectCanvas.gameObject;
        }

        GameObject effectInstance = null; //finally에서 참조하기 위해 미리 선언
        
        try
        {
            Debug.LogWarning($"[이펙트: UniTask (ID : {effectId} TYPE: {effectType})] EffectAsync operation try entered.");
            
            //획득 대기. 
             effectInstance = await ObjectPoolManager.Instance.GetObjectAsync(path, target.transform, position);
            //Duration 만큼 대기
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
            Debug.Log($"이펙트: UniTask (ID : {effectId}) EffectAsync operation finally entered.");
            if (effectInstance != null)
            {
                ObjectPoolManager.Instance.ReleaseObject(path, effectInstance);
                Debug.Log($"이펙트: UniTask (ID : {effectId}) EffectAsync operation completed and object {path} released.");
            }
        }
        
    }
}
