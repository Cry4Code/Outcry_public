using UnityEngine;
using UnityEngine.InputSystem;

public class CursorManager : Singleton<CursorManager>
{
    #region 커서들
    private RectTransform cursorRectTransform;
    private RectTransform parentRectTransform;
    private Canvas parentCanvas;
    private PlayerInputs.PlayerActions playerInputMap;
    private InputAction cursorInput;
    private Vector2 mousePos;
    #endregion

    [SerializeField] private Camera mainCamera;

    private Plane gamePlane;

    public bool IsInGame { get; set; } = false;
    private bool isInitialized = false; // 중복 초기화 방지 플래그

    public Vector3 mousePosition;

    protected override void Awake()
    {
        base.Awake();

        Cursor.visible = false;

        gamePlane = new Plane(Vector3.forward, Vector3.zero);

        // mainCamera 변수가 인스펙터에서 할당되지 않았다면 Camera.main을 사용
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    /// <summary>
    /// PlayerManager가 플레이어가 준비되었다고 알려주면 호출될 메서드
    /// </summary>
    public void InitializeForInGame(PlayerController player)
    {
        // 어드레서블 시스템을 통해 커서 프리팹 비동기 로드
        if (isInitialized || player == null)
        {
            return;
        }

        // U커서 UI 생성
        InGameCursorUI cursorUI = UIManager.Instance.Show<InGameCursorUI>();

        if (cursorUI == null)
        {
            Debug.LogError("InGameCursorUI를 UIManager에서 생성하는데 실패했습니다.");
            return;
        }

        // 생성된 UI의 RectTransform 참조를 얻어옴
        cursorRectTransform = cursorUI.GetComponent<RectTransform>();

        parentRectTransform = cursorRectTransform.parent as RectTransform;
        parentCanvas = parentRectTransform.GetComponent<Canvas>();

        // UI가 최상단에 보이도록 SortingOrder 매우 높게 설정
        if (cursorUI.canvas != null)
        {
            cursorUI.canvas.sortingOrder = 999; // 가장 높은 값으로 설정
        }

        Debug.Log("[CursorManager] 플레이어 준비 완료 신호를 받아 초기화를 시작합니다.");

        playerInputMap = player.Inputs.Player;
        cursorInput = playerInputMap.Look;
        SetInGame(true); // 커서 활성화 및 입력 활성화
        isInitialized = true;
    }

    /// <summary>
    /// 커서 토글용
    /// </summary>
    /// <param name="isInGame">전투중이면 True 부르면 됨</param>
    public void SetInGame(bool isInGame)
    {
        IsInGame = isInGame;

        Cursor.visible = !isInGame;
        if (isInGame)
        {
            playerInputMap.Enable();
            UIManager.Instance.Show<InGameCursorUI>();
            Cursor.lockState = CursorLockMode.Confined;
        }
        else
        {
            UIManager.Instance.Hide<InGameCursorUI>();
            playerInputMap.Disable();
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void LateUpdate()
    {
        if (cursorRectTransform == null || !IsInGame)
        {
            return;
        }

        mousePos = cursorInput.ReadValue<Vector2>();

        // 시각적 커서(UI) 위치 업데이트
        // RectTransformUtility를 사용하여 스크린 좌표를 UI의 로컬 좌표로 변환
        if (parentRectTransform != null)
        {
            Camera eventCamera = (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : mainCamera;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRectTransform,
                mousePos,
                eventCamera,
                out Vector2 localPoint))
            {
                cursorRectTransform.localPosition = localPoint;
            }
        }

        // 논리적 커서(World) 위치 업데이트
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        if (gamePlane.Raycast(ray, out float distance))
        {
            mousePosition = ray.GetPoint(distance);
        }
    }

    public bool IsLeftThan(Transform transform)
    {
        /*Debug.Log($"[CursorManager] mousePosition.x = {mousePosition.x}, transform.x = {transform.position.x}");*/
        return mousePosition.x < transform.position.x;
    }
}
