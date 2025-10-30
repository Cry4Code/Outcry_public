using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

public class LoadingManager : Singleton<LoadingManager>
{
    [SerializeField] private LoadingUI loadingUI;
    
    public void StartLoadingProcess()
    {
        LoadAndTransitionRoutine().Forget();
    }

    private async UniTaskVoid LoadAndTransitionRoutine()
    {
        var package = GameManager.Instance.NextLoadPackage;
        if (package == null)
        {
            Debug.LogError("로드할 패키지가 없습니다!");
            return;
        }

        // 리소스와 씬 데이터를 비동기적으로 로드
        await LoadTasksProcess(package, (progress, text) =>
        {
            loadingUI?.UpdateProgress(progress, text);
        });

        // 모든 로딩이 끝나면 SceneLoadManager에게 최종 씬 활성화 요청
        await SceneLoadManager.Instance.ActivateLoadedScenes(package);
    }

    /// <summary>
    /// 리소스와 씬 데이터를 메모리에 비동기적으로 로드하는 메인 코루틴
    /// </summary>
    public async UniTask LoadTasksProcess(SceneLoadPackage package, Action<float, string> onProgress)
    {
        float currentProgress = 0f;

        // 선행 작업(Pre-loading Tasks) 먼저 실행
        if (package.PreLoadingTasks != null && package.PreLoadingTasks.Count > 0)
        {
            foreach (var task in package.PreLoadingTasks)
            {
                onProgress?.Invoke(currentProgress, task.Description);
                // task에 연결된 코루틴을 실행하고 끝날 때까지 대기
                await task.Coroutine().ToUniTask(this);
            }
        }
        currentProgress = 0.1f; // 선행 작업이 끝난 후 진행도 초기화


        // 현재 선택된 로케일(언어) 정보
        Locale currentLanguage = LocalizationSettings.SelectedLocale;
        
        
        // 리소스 로딩
        // onProgress?.Invoke(currentProgress, "Preparing resources...");
        onProgress?.Invoke(currentProgress, LocalizationUtility.GetLocalizedValueByKey(LocalizationStrings.Loading.PREPARINGRESOURCES));
        await ResourceManager.Instance.LoadAllAssetsCoroutine(package.ResourceAddressesToLoad).ToUniTask(this);
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

        currentProgress = 0.4f;
        // onProgress?.Invoke(currentProgress, "Loading...");
        onProgress?.Invoke(currentProgress, LocalizationUtility.GetLocalizedValueByKey(LocalizationStrings.Loading.LOADING));

        // 씬 데이터 로딩 (활성화는 하지 않음)
        package.SceneLoadOperations = new List<AsyncOperation>();
        float totalSceneCount = 1 + (package.AdditiveSceneNames?.Count ?? 0);
        float progressPerScene = 0.6f / totalSceneCount; // 씬 로딩에 할당된 전체 진행률(60%)을 씬 개수만큼 나눔

        // 메인 씬 먼저 로드하고 기다리기
        var mainSceneOp = SceneManager.LoadSceneAsync(package.MainSceneType.ToString(), LoadSceneMode.Additive);
        mainSceneOp.allowSceneActivation = false;
        package.SceneLoadOperations.Add(mainSceneOp);

        while (mainSceneOp.progress < 0.9f)
        {
            // 현재까지 로드된 씬들의 진행률을 합산하여 UI 업데이트
            float sceneProgress = mainSceneOp.progress / 0.9f * progressPerScene;
            // onProgress?.Invoke(0.4f + sceneProgress, "Loading scene data...");
            onProgress?.Invoke(0.4f + sceneProgress, LocalizationUtility.GetLocalizedValueByKey(LocalizationStrings.Loading.SCENEDATA));
            await UniTask.Yield();
        }
        Debug.Log($"<color=lime>메인 씬 '{package.MainSceneType}' 로딩 완료.</color>");

        // Additive 씬들을 순차적으로 로드하고 기다리기
        if (package.AdditiveSceneNames != null)
        {
            for (int i = 0; i < package.AdditiveSceneNames.Count; i++)
            {
                var sceneName = package.AdditiveSceneNames[i];
                var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                op.allowSceneActivation = false;
                package.SceneLoadOperations.Add(op);

                while (op.progress < 0.9f)
                {
                    // 현재까지 로드된 모든 씬의 진행률을 합산하여 UI 업데이트
                    float totalCompletedProgress = (i + 1) * progressPerScene; // 이미 완료된 씬들의 진행률
                    float currentOpProgress = op.progress / 0.9f * progressPerScene; // 현재 로딩 중인 씬의 진행률
                    // onProgress?.Invoke(0.4f + totalCompletedProgress + currentOpProgress, "Loading scene data...");
                    onProgress?.Invoke(0.4f + totalCompletedProgress + currentOpProgress, LocalizationUtility.GetLocalizedValueByKey(LocalizationStrings.Loading.SCENEDATA));
                    await UniTask.Yield();
                }
                Debug.Log($"<color=lime>추가 씬 '{sceneName}' 로딩 완료.</color>");
            }
        }

        // onProgress?.Invoke(1f, "Loading complete!");
        onProgress?.Invoke(1f, LocalizationUtility.GetLocalizedValueByKey(LocalizationStrings.Loading.COMPLETE));
        await UniTask.Delay(TimeSpan.FromSeconds(0.2f)); // "Loading complete" 메시지가 잠시 보이도록 딜레이
        Debug.Log("<color=orange>LoadingManager: 모든 리소스 및 씬 데이터 로딩 완료.</color>");
    }
}
