using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    // 씬 전환 마다 uis 클리어
    private Dictionary<string, UIBase> uis = new Dictionary<string, UIBase>();
    public static int screenWidth = 1920;
    public static int screenHeight = 1080;

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

        ui.Close();
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
        result.canvas.sortingOrder = uis.Count;

        return (T)result;
    }

    public void ClearUIPool()
    {
        // 모든 UI를 닫고 딕셔너리 클리어
        foreach (var ui in uis.Values)
        {
            ui.Close();
            Destroy(ui.gameObject);
        }

        uis.Clear();
    }
}
