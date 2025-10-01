using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SoundEnums;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "SoundEffectData", menuName = "ScriptableObjects/Effects/SoundEffectData", order = 3)]
public class SoundEffectData : BaseEffectData
{
//   [field: SerializeField] public float Duration { get; private set; } = 0f;
    [field: SerializeField] public float Volume { get; private set; } = 1f;
    [field: SerializeField] public string path { get; private set; }
#if UNITY_EDITOR
    [Header("Editor Only")]
    public AudioClip audioClip;
    private void OnValidate()
    {
        this.effectType = EffectType.Sound;
        if (audioClip != null)
        {
            path = AssetDatabase.GetAssetPath(audioClip).Replace(AddressablePaths.ROOT, "");
            audioClip = null;
        }
    }
#endif

    public override async UniTask EffectAsync(EffectOrder order, CancellationToken token, GameObject target = null, Vector3 position = default(Vector3))
    {
        // AudioManater에서 사용 가능한 함수   
        // public int PlaySFX(string address, float volume)
        // public int PlaySFX(string address, float volume, float pitch, Vector3 position)
        int instanceId = -1;
        
        try
        {
            if (target == null)
                instanceId = await AudioManager.Instance.PlaySFXAsync(path, Volume);
            else
                instanceId = await AudioManager.Instance.PlaySFXAsync(path, Volume, 1, target.transform.position);

            var length = AudioManager.Instance.GetSfxLength(instanceId);

            // Debug.LogWarning($"[이펙트: UniTask (ID : {effectId})] SFX length: {length}, instanceId: {instanceId}");
            await UniTask.Delay((int)length * 1000, cancellationToken: token);
        }
        catch (OperationCanceledException)
        {
            // Debug.LogWarning($"[이펙트: UniTask (ID : {effectId})] EffectAsync operation cancelled");
            AudioManager.Instance.StopSFX(instanceId);
            
            // Debug.LogWarning($"[이펙트: UniTask (ID : {effectId})] EffectAsync operation cancelled - StopSFX called");
        }
        catch (Exception e)
        {
            // Debug.LogError($"[이펙트: UniTask (ID : {effectId})] EffectAsync operation failed: {e}");
        }
        finally
        {
            // Debug.Log($"이펙트: UniTask (ID : {effectId}) EffectAsync operation finally entered.");
        }
    }

    // public override IEnumerator EffectCoroutine(EffectOrder order, GameObject target = null)
    // {
    //     if(target == null)
    //         AudioManager.Instance.PlaySFX(path, Volume);
    //     else
    //         AudioManager.Instance.PlaySFX(path, Volume, 1, target.transform.position);
    //         
    //     return null;
    // }
}
