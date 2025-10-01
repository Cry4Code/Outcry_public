using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public enum EffectOrder
{
    SpecialEffect = 100,
    Player = 75,
    Monster = 25,
    None = 0
}

public class EffectManager : Singleton<EffectManager>
{
    public EffectDatabase EffectDatabase;
    public Canvas EffectCanvas { get; private set; } //ui 이펙트용 캔버스

    private EffectOrder currentEffectOrder;
    
    private CancellationTokenSource currentCts; // 전체 이펙트 취소용

    // 여러 이펙트 동시 관리
    private Dictionary<(int, EffectType), CancellationTokenSource> effectCtsDict = new();
    private Dictionary<(int, EffectType), BaseEffectData> currentEffectsDict = new();
    
    protected override async void Awake()
    {
        base.Awake();
        
        EffectDatabase = await ResourceManager.Instance.LoadAssetAsync<EffectDatabase>("EffectDatabase", Paths.SO);
        EffectDatabase.Initialize();
        
        if (EffectCanvas == null)
            SetEffectCanvase();
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="effectId"></param>
    /// <param name="order"></param>
    /// <param name="target"></param>
    /// <param name="position"></param>
    public async UniTask PlayEffectsByIdAsync(int effectId, EffectOrder order, GameObject target = null, Vector3 position = default(Vector3))
    {
        Debug.Log($"[이펙트] currentOrder: {currentEffectOrder}, 요청 order: {order}");

        //더 높은 순위인지 확인
        if(order < currentEffectOrder)
        {
            return;
        }

        //기존 이펙트 모두 취소
        foreach (var cts in effectCtsDict.Values)
        {
            cts.Cancel();
        }
        effectCtsDict.Clear();
        currentEffectsDict.Clear();
        
        currentEffectOrder = order; //현재 이펙트 순위 갱신
        
        //id에 해당하는 이펙트들 가져오기
        var effects = EffectDatabase.GetEffectsById(effectId);
        if (effects == null) return;

        foreach (var kvp in effects)
        {
            var effectType = kvp.Key;
            var effect = kvp.Value;
            var key = (effectId, effectType);
            
            var cts = new CancellationTokenSource();
            effectCtsDict[key] = cts;
            currentEffectsDict[key] = effect;

            _ = RunEffectAsync(effect, order, cts, key, target, position);
        }
    }
   
    private async UniTask RunEffectAsync(BaseEffectData effect, EffectOrder order, CancellationTokenSource cts, (int, EffectType) key, GameObject target = null, Vector3 position = default)
    {
        try
        {
            await effect.EffectAsync(order, cts.Token, target, position);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("[이펙트] EffectAsync 취소됨");
        }
        finally
        {
            cts.Dispose();
            effectCtsDict.Remove(key);
            currentEffectsDict.Remove(key);

            if (effectCtsDict.Count == 0)
            {
                currentEffectOrder = EffectOrder.None;
            }
        }
    }

    /// <summary>
    /// 이미 재생 중인 같은 타입의 이펙트가 있다면 해당 타입의 이펙트'만' 취소 후 재생합니다.
    /// 그 외 다른 타입의 이펙트는 유지됩니다.
    /// 우선순위를 따지지 않고 이전 이펙트를 취소 및 새 이펙트를 재생하므로 주의하여 사용하세요.
    /// </summary>
    /// <param name="effectId"></param>
    /// <param name="effectType"></param>
    /// <param name="target"></param>
    /// <param name="position"></param>
    public async UniTask PlayEffectByIdAndTypeAsync(int effectId, EffectType effectType, GameObject target = null, Vector3 position = default(Vector3))
    {
        var effect = EffectDatabase.GetEffectByIdAndType(effectId, effectType);
        if (effect == null) return;

        var key = (effectId, effectType);

        if (effectCtsDict.TryGetValue(key, out var prevCts))
        {
            prevCts.Cancel();
            prevCts.Dispose();
        }
        
        var cts = new CancellationTokenSource();
        effectCtsDict[key] = cts;
        currentEffectsDict[key] = effect;

        await RunEffectAsync(effect, EffectOrder.SpecialEffect, cts, key, target, position);
    }
    
    public void StopEffectByType(EffectType effectType)
    {
        var keysToRemove = effectCtsDict.Keys.Where(k => k.Item2 == effectType).ToList();
        if (keysToRemove.Count == 0)
        {
            return;
        }
        foreach (var key in keysToRemove)
        {
            if (effectCtsDict.TryGetValue(key, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
                effectCtsDict.Remove(key);
                currentEffectsDict.Remove(key);
            }
        }

        if (effectCtsDict.Count == 0)
        {
            currentEffectOrder = EffectOrder.None;
        }
    }

    public void StopAllEffects()
    {
        foreach (var cts in effectCtsDict.Values)
        {
            cts.Cancel();
        }
        effectCtsDict.Clear();
        currentEffectsDict.Clear();
        currentEffectOrder = EffectOrder.None;
    }
    
    private void SetEffectCanvase(string canvasName = "EffectCanvas")
    {
        if (EffectCanvas == null)
        {
            GameObject canvasObj = new GameObject(canvasName);
            EffectCanvas = canvasObj.AddComponent<Canvas>();
            EffectCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
    }

    public bool IsEffectPlaying(int effectId, EffectType type = EffectType.None)
    {
        if (type == EffectType.None)
            return false;
        else
            return currentEffectsDict.ContainsKey((effectId, type));
    }
}