using System.Threading.Tasks;
using UnityEngine;

public class TitleScene : SceneBase
{
    public override void SceneAwake()
    {
        UIManager.Instance.Show<TitleUI>();
    }

    public override async void SceneEnter()
    {
        _ = AudioManager.Instance.PlayBGM((int)SoundEnums.EBGM.Title);

        TitleUI titleUI = UIManager.Instance.GetUI<TitleUI>();

        // 시작하자마자 모든 버튼을 비활성화합니다.
        titleUI.SetButtonsInteractable(false);

        // FirebaseManager의 초기화가 완료될 때까지 기다립니다.
        while (!FirebaseManager.Instance.IsInit())
        {
            await Task.Yield();
        }

        Debug.Log("Firebase 초기화 완료.");

        // 초기화 및 최소 대기 시간이 끝나면 버튼을 활성화하여 유저 입력 받음
        Debug.Log("UI 버튼을 활성화합니다. 유저의 입력을 기다립니다.");
        titleUI.SetButtonsInteractable(true);
    }

    public override void SceneExit()
    {
        UIManager.Instance.ClearUIPool();
        _ = AudioManager.Instance.StopBGM();
    }
}
