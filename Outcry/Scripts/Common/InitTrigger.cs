using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitTrigger : MonoBehaviour
{
    public ESceneType startScene;

    private void Awake()
    {
        // 매니저 씬 로드
        SceneLoadManager.Instance.LoadInitManager();
    }

    private void Start()
    {
        // TEST: 시작 씬을 로비 씬으로 설정
        SceneLoadManager.Instance.LoadScene(startScene);
    }
}
