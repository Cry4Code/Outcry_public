using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    public static int screenWidth = 1920;
    public static int screenHeight = 1080;
    public Stack<UIPopup> PopupStack => popupStack;

    // 씬 전환 마다 uis 클리어
    private Dictionary<string, UIBase> uis = new Dictionary<string, UIBase>();

    // 팝업 UI를 관리하기 위한 스택 추가
    private Stack<UIPopup> popupStack = new Stack<UIPopup>();
    private int baseSortingOrder = 10; // 팝업이 일반 UI(HUD 등)와 겹치지 않도록 기본 순서값 설정

    protected override void Awake()
    {
        base.Awake();

        InitializeEventSystem();
    }

    /// <summary>
    /// 씬에 EventSystem이 있는지 확인하고 없으면 새로 생성하는 메서드
    /// </summary>
    private void InitializeEventSystem()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            Debug.Log("씬에 EventSystem이 없어 새로 생성합니다.");
            // EventSystem이 없다면 새로 게임 오브젝트를 만듦
            GameObject eventSystemGO = new GameObject("EventSystem");
            // EventSystem 컴포넌트와 입력을 위한 StandaloneInputModule 컴포넌트 추가
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();

            // UIManager가 씬 전환 시 파괴되지 않으므로 EventSystem도 자식으로 만들어 함께 유지
            eventSystemGO.transform.SetParent(this.transform);
        }
    }

    public T Show<T>() where T : UIBase
    {
        string uiName = typeof(T).Name;
        string path = Path.Combine(Paths.Prefabs.UI, uiName);

        uis.TryGetValue(uiName, out UIBase ui);
        
        // uis에 ui가 없으면 새로 로드
        if (ui == null)
        {
            ui = Load<T>();
            uis.Add(uiName, ui);
        }

        ui.Open();

        // UI가 팝업 타입인지 확인
        if (ui is UIPopup popup)
        {
            // 스택에 팝업을 추가하고 SortingOrder를 재설정
            popupStack.Push(popup);
            RefreshPopupOrder();
        }

        return (T)ui;
    }

    public void Hide<T>()
    {
        string uiName = typeof(T).Name;

        uis.TryGetValue(uiName, out UIBase ui);

        // uis에 ui가 없으면, 경고
        if (ui == null)
        {
            Debug.LogWarning($"{uiName} 이 없습니다.");
            return;
        }

        // 닫으려는 UI가 스택의 최상단 팝업인지 확인
        if (popupStack.Count > 0 && ui == popupStack.Peek())
        {
            popupStack.Pop();
            RefreshPopupOrder();
        }

        ui.Close();
    }

    /// <summary>
    /// 현재 열려있는 모든 팝업 UI 닫음
    /// </summary>
    public void CloseAllPopups()
    {
        while (popupStack.Count > 0)
        {
            // 스택에서 하나씩 꺼내서 닫기
            UIPopup popup = popupStack.Pop();
            popup.Close();
        }
    }

    public void CloseAllPopupsWithoutSound()
    {
        while (popupStack.Count > 0)
        {
            // 스택에서 하나씩 꺼내서 닫기
            UIPopup popup = popupStack.Pop();
            popup.CloseWithoutSound();
        }
    }

    public void CloseAllPopupsAndResumeGame()
    {
        StartCoroutine(CloseAllPopupsAndResumeRoutine());
    }

    private IEnumerator CloseAllPopupsAndResumeRoutine()
    {
        // 모든 팝업 UI 닫음
        CloseAllPopups();

        // 현재 프레임의 렌더링이 끝날 때까지 기다림
        yield return new WaitForEndOfFrame();

        // 커서 활성화하고 플레이어 입력 받음
        CursorManager.Instance.SetInGame(true);
        if (PlayerManager.Instance != null && PlayerManager.Instance.player != null)
        {
            PlayerManager.Instance.player.PlayerInputEnable();
        }
    }

    public void ClosePopupAndResumeGame<T>() where T : UIPopup
    {
        StartCoroutine(ClosePopupAndResumeRoutine<T>());
    }

    private IEnumerator ClosePopupAndResumeRoutine<T>() where T : UIPopup
    {
        // 지정된 타입 UI 닫음
        Hide<T>();

        // 현재 프레임의 렌더링이 모두 끝날 때까지 기다림
        // 이 대기 시간 동안 Input 시스템이 실제 마우스 위치를 갱신할 시간 확보
        yield return new WaitForEndOfFrame();

        // 안전하게 커서 활성화하고 플레이어 입력을 받음
        CursorManager.Instance.SetInGame(true);
        if (PlayerManager.Instance != null && PlayerManager.Instance.player != null)
        {
            PlayerManager.Instance.player.PlayerInputEnable();
        }
    }

    /// <summary>
    /// 팝업 스택을 기반으로 캔버스의 SortingOrder 재정렬
    /// </summary>
    private void RefreshPopupOrder()
    {
        int currentOrder = baseSortingOrder;
        // 스택의 가장 아래부터 순회하기 위해 임시 배열 사용
        UIPopup[] popups = popupStack.ToArray();
        for (int i = popups.Length - 1; i >= 0; i--)
        {
            popups[i].canvas.sortingOrder = currentOrder++;
        }
    }

    public T GetUI<T>() where T : UIBase
    {
        string uiName = typeof(T).Name;

        uis.TryGetValue(uiName, out UIBase ui);

        if (ui == null)
        {
            Debug.LogWarning($"{uiName} 이 없습니다.");
            return default;
        }

        return (T)ui;
    }

    public bool IsUIActive<T>() where T : UIBase
    {
        string uiName = typeof(T).Name;

        if (uis.TryGetValue(uiName, out UIBase ui))
        {
            // UI가 존재하고 그 게임 오브젝트가 활성화 상태인지 확인
            return ui != null && ui.gameObject.activeInHierarchy;
        }

        return false;
    }

    private T Load<T>() where T : UIBase
    {
        // 캔버스 생성
        GameObject parentCanvas = new GameObject(typeof(T).Name + " Canvas");
        
        var canvas = parentCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var canvasScaler = parentCanvas.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2 (screenWidth, screenHeight);

        parentCanvas.AddComponent<GraphicRaycaster>();

        // ui 프리팹 로드
        string uiName = typeof(T).Name;
        //string path = Path.Combine(Paths.Prefabs.UI, uiName);

        var ui = ResourceManager.Instance.LoadAsset<T>(uiName, Paths.Prefabs.UI);

        // ui 생성
        var go = Instantiate(ui, parentCanvas.transform);
        go.name = go.name.Replace("(Clone)", "");

        var result = go.GetComponent<UIBase>();
        result.canvas = canvas;

        // SortingOrder 초기값 설정은 Show 메서드에서 관리하므로 여기서 uis.Count를 사용하지 않음
        result.canvas.sortingOrder = 0;

        return (T)result;
    }

    public void ClearUIPool()
    {
        CloseAllPopupsWithoutSound(); // 모든 팝업 닫기

        // 모든 UI를 닫고 딕셔너리 클리어
        foreach (var ui in uis.Values)
        {
            ui.CloseWithoutSound();
            Destroy(ui.gameObject);
        }

        uis.Clear();
    }
}
