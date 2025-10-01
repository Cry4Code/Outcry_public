using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum EffectType
{
    Camera,
    Background, //Color 정도로 바꿔야하나..?
    Sound,
    ScreenUI,
    Particle,
    Sprite,
    None
}

[CreateAssetMenu(fileName = "EffectDatabase", menuName = "ScriptableObjects/EffectDatabase", order = 1)]
public class EffectDatabase : ScriptableObject
{
    [SerializeField] private List<BaseEffectData> effectDataList = new ();
    private Dictionary<int, Dictionary<EffectType, BaseEffectData>> effectMap = new ();

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        effectMap.Clear();
        foreach (var effectData in effectDataList)
        {
            if (!effectMap.ContainsKey(effectData.effectId))
            {
                effectMap[effectData.effectId] = new Dictionary<EffectType, BaseEffectData>();
            }
            effectMap[effectData.effectId][effectData.effectType] = effectData;
        }
    }

    public Dictionary<EffectType,BaseEffectData> GetEffectsById(int effectId)
    {
        if (effectMap.TryGetValue(effectId, out var effects))
        {
            return effects;
        }
        return null;
    }
    
    public BaseEffectData GetEffectByIdAndType(int effectId, EffectType effectType)
    {
        if (effectMap.TryGetValue(effectId, out var effects) && effects.TryGetValue(effectType, out var effect))
        {
            return effect;
        }
        return null;
    }
    
#if UNITY_EDITOR
    
    [Header("Editor Only")]
    public List<BaseEffectData> fillteredEffects = new ();
    public int filterEffectId = -1;
    public bool filterEffectsByType = false;
    public EffectType filterEffectType = EffectType.Camera;
    
    private void OnValidate()
    {
        Initialize();
        if (filterEffectId >= 0)
        {
            fillteredEffects.Clear();
            foreach (var effectData in effectDataList)
            {
                if (effectData.effectId == filterEffectId)
                {
                    if (filterEffectsByType)
                    {
                        if (effectData.effectType == filterEffectType)
                        {
                            fillteredEffects.Add(effectData);
                        }
                    }
                    else
                    {
                        fillteredEffects.Add(effectData);
                    }
                }
            }
        }
        else
        {
            fillteredEffects = new List<BaseEffectData>(effectDataList);
        }
    }
    
    
    [ContextMenu("Auto Populate Effects")]
    private void AutoPopulateItems()
    {
        effectDataList.Clear();
        string[] guids = AssetDatabase.FindAssets("t:BaseEffectData", new[] { "Assets/11. ScriptableObjects/Effects" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            BaseEffectData effectData = AssetDatabase.LoadAssetAtPath<BaseEffectData>(path);
            if (effectData != null && !effectDataList.Contains(effectData))
            {
                effectDataList.Add(effectData);
            }
        }
        EditorUtility.SetDirty(this);
        Initialize();
        Debug.Log($"Auto populated {effectDataList.Count} effects.");
    }

    // [MenuItem("Tools/EffectDatabase/Auto Populate All")]
    // public static void AutoPopulateAllEffectDatabases()
    // {
    //     string[] guids = AssetDatabase.FindAssets("t:EffectDatabase");
    //     foreach (string guid in guids)
    //     {
    //         string path = AssetDatabase.GUIDToAssetPath(guid);
    //         var db = AssetDatabase.LoadAssetAtPath<EffectDatabase>(path);
    //         if (db != null)
    //         {
    //             db.GetType()
    //                 .GetMethod("AutoPopulateItems", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
    //                 .Invoke(db, null);
    //         }
    //     }
    //     EditorUtility.DisplayDialog("EffectDatabase", "모든 EffectDatabase를 자동 리프레시했습니다.", "OK");
    // }
    
    [ContextMenu("Clear Effects")]
    private void ClearItems()
    {
        effectDataList.Clear();
        effectMap.Clear();
        EditorUtility.SetDirty(this);
        Debug.Log("Cleared all effects from the database.");
    }
#endif
}
