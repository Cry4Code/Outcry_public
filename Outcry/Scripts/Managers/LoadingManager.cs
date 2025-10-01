using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : Singleton<LoadingManager>
{
    [SerializeField] private LoadingUI loadingUI;

    public void StartLoadingProcess()
    {
        StartCoroutine(LoadAndTransitionRoutine());
    }

    private IEnumerator LoadAndTransitionRoutine()
    {
        var package = GameManager.Instance.NextLoadPackage;
        if (package == null)
        {
            Debug.LogError("로드할 패키지가 없습니다!");
            yield break;
        }

        // 리소스와 씬 데이터를 비동기적으로 로드
        yield return StartCoroutine(LoadTasksProcess(package, (progress, text) =>
        {
            loadingUI?.UpdateProgress(progress, text);
        }));

        // 모든 로딩이 끝나면 SceneLoadManager에게 최종 씬 활성화 요청
        yield return StartCoroutine(SceneLoadManager.Instance.ActivateLoadedScenes(package));
    }

    /// <summary>
    /// 리소스와 씬 데이터를 메모리에 비동기적으로 로드하는 메인 코루틴
    /// </summary>
    public IEnumerator LoadTasksProcess(SceneLoadPackage package, Action<float, string> onProgress)
    {
        float currentProgress = 0f;

        // 선행 작업(Pre-loading Tasks) 먼저 실행
        if (package.PreLoadingTasks != null && package.PreLoadingTasks.Count > 0)
        {
            foreach (var task in package.PreLoadingTasks)
            {
                onProgress?.Invoke(currentProgress, task.Description);
                // task에 연결된 코루틴을 실행하고 끝날 때까지 대기
                yield return StartCoroutine(task.Coroutine());
            }
        }
        currentProgress = 0.1f; // 선행 작업이 끝난 후 진행도 초기화

        // 리소스 로딩
        onProgress?.Invoke(currentProgress, "Preparing resources...");
        yield return StartCoroutine(ResourceManager.Instance.LoadAllAssetsCoroutine(package.ResourceAddressesToLoad));
        yield return new WaitForSeconds(0.5f); // 로딩이 너무 빠를 경우를 대비한 최소 딜레이

        currentProgress = 0.4f;
        onProgress?.Invoke(currentProgress, "Loading...");

        // 씬 데이터 로딩 (활성화는 하지 않음)
        package.SceneLoadOperations = new List<AsyncOperation>();

        // 메인 씬 로드
        var mainSceneOp = SceneManager.LoadSceneAsync(package.MainSceneType.ToString(), LoadSceneMode.Additive);
        mainSceneOp.allowSceneActivation = false;
        package.SceneLoadOperations.Add(mainSceneOp);

        // 추가 씬 로드(매니저용)
        if (package.AdditiveSceneNames != null)
        {
            foreach (var sceneName in package.AdditiveSceneNames)
            {
                var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                op.allowSceneActivation = false;
                package.SceneLoadOperations.Add(op);
            }
        }

        // 모든 씬 데이터가 90% 로드될 때까지 대기(allowSceneActivation = false 상태에서는 0.9에서 멈춤)
        float totalProgress = 0f;
        while (totalProgress < package.SceneLoadOperations.Count * 0.9f)
        {
            totalProgress = 0f;
            foreach (var op in package.SceneLoadOperations)
            {
                totalProgress += op.progress;
            }

            // 전체 진행도 계산: 리소스 로딩(40%) + 씬 로딩(60%) = 100%
            float progressValue = 0.4f + (totalProgress / package.SceneLoadOperations.Count * 0.6f);
            onProgress?.Invoke(progressValue, "Loading...");

            yield return null;
        }

        onProgress?.Invoke(1f, "Loading complete!");
        yield return new WaitForSeconds(0.2f); // "Loading complete" 메시지가 잠시 보이도록 딜레이
        Debug.Log("<color=orange>LoadingManager: 모든 리소스 및 씬 데이터 로딩 완료.</color>");
    }
}
