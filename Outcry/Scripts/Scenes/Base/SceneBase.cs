using UnityEngine;

/// <summary>
/// 씬 이름과 씬 베이스를 상속받는 클래스의 이름을 같게 해주세요.
/// </summary>
public abstract class SceneBase
{
    public string SceneName { get; protected set; }

    public abstract void SceneAwake();
    public abstract void SceneEnter();
    public abstract void SceneExit();
}
