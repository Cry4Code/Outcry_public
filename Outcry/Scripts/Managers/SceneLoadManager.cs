using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadManager : Singleton<SceneLoadManager>
{
    private Dictionary<ESceneType, SceneBase> scenes;
    private SceneBase currentScene;
    private SceneBase prevScene;
    private string prevSceneName;

    private const string managerSceneName = "InitManagers";

    protected override void Awake()
    {
        base.Awake();

        scenes = new Dictionary<ESceneType, SceneBase>
        {
            { ESceneType.TitleScene, new TitleScene() },
            { ESceneType.LoadingScene, new LoadingScene() },
            { ESceneType.InGameScene, new InGameScene() },
        };
    }

    public async UniTaskVoid LoadInitManager()
    {
        await SceneManager.LoadSceneAsync(managerSceneName, LoadSceneMode.Additive);
        await SceneManager.UnloadSceneAsync(managerSceneName);

        Debug.Log("<color=yellow>Manager Scene Load Complete.</color>");
    }

    public void LoadScene(ESceneType sceneType)
    {
        StartCoroutine(LoadSceneRoutine(sceneType));
    }

    private IEnumerator LoadSceneRoutine(ESceneType sceneType)
    {
        // Fade Out 실행 및 완료까지 대기
        yield return FadeManager.Instance.FadeOut();

        // 현재 씬이 InGameScene일 경우에만 씬 전환 직전 모든 카메라 및 이펙트 효과들 강제 중지
        if (currentScene is InGameScene)
        {
            if (EffectManager.Instance != null)
            {
                EffectManager.Instance.StopAllEffects();
            }
            if (ObjectPoolManager.Instance != null)
            {
                ObjectPoolManager.Instance.ClearAllPools();
            }

            if (CameraManager.Instance != null)
            {
                CameraManager.Instance.StopAllCameraCoroutine();
            }            
        }

        if (!scenes.ContainsKey(sceneType))
        {
            Debug.LogWarning($"{sceneType.ToString()} 씬이 존재하지 않습니다.");
            // Fade In을 다시 실행하여 화면을 돌려줌
            yield return FadeManager.Instance.FadeIn();
            yield break; // 코루틴 종료
        }

        if (currentScene != null && currentScene.SceneName == sceneType.ToString())
        {
            yield return FadeManager.Instance.FadeIn();
            yield break;
        }

        if (currentScene != null)
        {
            currentScene.SceneExit();
        }

        currentScene = null;

        // 씬 비동기 로드
        var op = SceneManager.LoadSceneAsync(sceneType.ToString());
        while (!op.isDone)
        {
            yield return null;
        }

        // SceneBase 초기화
        currentScene = scenes[sceneType];
        currentScene.SceneAwake();
        currentScene.SceneEnter();

        // Fade In 실행 및 완료까지 대기
        yield return FadeManager.Instance.FadeIn();
    }

    /// <summary>
    /// 로딩 이후 씬 활성화
    /// </summary>
    public async UniTask ActivateLoadedScenes(SceneLoadPackage package)
    {
        // 이전 씬 정리
        currentScene?.SceneExit();
        prevSceneName = SceneManager.GetActiveScene().name;

        // 메인 씬 활성화
        package.SceneLoadOperations[0].allowSceneActivation = true;
        await package.SceneLoadOperations[0];

        Scene newActiveScene = SceneManager.GetSceneByName(package.MainSceneType.ToString());
        SceneManager.SetActiveScene(newActiveScene);

        // 나머지 Additive 씬들도 모두 활성화
        for (int i = 1; i < package.SceneLoadOperations.Count; i++)
        {
            package.SceneLoadOperations[i].allowSceneActivation = true;
        }

        // SceneBase 논리 상태 전환
        if (scenes.TryGetValue(package.MainSceneType, out SceneBase newScene))
        {
            currentScene = newScene;
            currentScene.SceneAwake();
            currentScene.SceneEnter();
        }

        // 이전 씬(LoadingScene) 언로드
        if (!string.IsNullOrEmpty(prevSceneName) && SceneManager.GetSceneByName(prevSceneName).isLoaded)
        {
            await SceneManager.UnloadSceneAsync(prevSceneName);
        }

        Debug.Log($"<color=cyan>SceneLoadManager: '{package.MainSceneType}'으로 씬 전환 완료.</color>");

        await FadeManager.Instance.FadeIn().ToUniTask();
    }

    public async UniTaskVoid LoadAdditiveScene(string sceneName)
    {
        await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        Debug.Log($"<color=yellow>Additive Scene '{sceneName}' Load Complete.</color>");
    }
}
