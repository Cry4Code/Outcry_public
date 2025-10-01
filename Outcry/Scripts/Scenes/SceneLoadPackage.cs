using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LoadingScene이 수행해야 할 작업의 명세를 담는 데이터 클래스
/// </summary>
public class SceneLoadPackage
{
    public ESceneType MainSceneType { get; private set; }
    public List<string> AdditiveSceneNames { get; private set; } = new List<string>();
    public List<string> ResourceAddressesToLoad { get; private set; } = new List<string>();

    // 로딩 선행 작업 목록
    public List<LoadingTask> PreLoadingTasks { get; private set; } = new List<LoadingTask>();

    public List<AsyncOperation> SceneLoadOperations { get; set; }

    public SceneLoadPackage(ESceneType mainSceneType)
    {
        MainSceneType = mainSceneType;
    }
}