using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "CameraShakeEffectData", menuName = "ScriptableObjects/Effects/CameraShakeEffectData", order = 1)]
public class CameraShakeEffectData : BaseEffectData
{
    [field: SerializeField] public float Duration { get; private set; }
    [field: SerializeField] public float Magnitude { get; private set; }
    [field: SerializeField] public float Frequency { get; private set; }
#if UNITY_EDITOR
    private void OnValidate()
    {
        this.effectType = EffectType.Camera;
    }
#endif
    public override async UniTask EffectAsync(EffectOrder order, CancellationToken token, GameObject target = null, Vector3 position = default(Vector3))
    {
        try
        {
            Debug.LogWarning($"[이펙트: UniTask (ID : {effectId} TYPE: {effectType})] EffectAsync operation try entered.");
            CameraManager.Instance.ShakeCamera(Duration, Magnitude, Frequency, EffectOrder.SpecialEffect);
            await UniTask.Delay((int)(Duration * 1000), cancellationToken: token);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning($"[이펙트: UniTask (ID : {effectId})] EffectAsync operation cancelled");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[이펙트: UniTask (ID : {effectId})] EffectAsync operation failed: {e.Message}");
        }
        finally
        {
            //카메라 흔들기 종료
            CameraManager.Instance.StopCameraShake();
            Debug.LogWarning($"[이펙트: UniTask (ID : {effectId})] EffectAsync operation finished.");
        }
    }
}
