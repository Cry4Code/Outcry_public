using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "New Tutorial Data", menuName = "Tutorial/New Tutorial Data")]
public class TutorialDataSO : ScriptableObject
{
    public List<TutorialPageData> pages;
}

[Serializable]
public class TutorialPageData
{
    public AssetReferenceSprite PageImageRef;
    [TextArea(3, 10)]
    public string Description;
    [TextArea(3, 10)]
    public string Description_Ko;
}
