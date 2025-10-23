using Cysharp.Threading.Tasks;
using UnityEngine;

public class TitleScene : SceneBase
{
    public override void SceneAwake()
    {
        EffectManager.Instance.ToString();
        ResourceManager.Instance.LoadAssetsByLabelAsync<AudioClip>("UISFX").Forget();

        UIManager.Instance.Show<TitleUI>();
    }

    public override void SceneEnter()
    {
        _ = AudioManager.Instance.PlayBGM((int)SoundEnums.EBGM.Title);

        TitleUI titleUI = UIManager.Instance.GetUI<TitleUI>();
    }

    public override void SceneExit()
    {
        UIManager.Instance.ClearUIPool();
        _ = AudioManager.Instance.StopBGM();
    }
}
