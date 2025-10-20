using UnityEngine;
using UnityEngine.InputSystem;

public class CursorManager : Singleton<CursorManager>
{
    #region 커서들
    private Transform inGameCursor; // 인게임용
    private PlayerInputs.PlayerActions playerInputMap;
    private InputAction cursorInput;
    private Vector2 mousePos;
    #endregion

    private Camera mainCam;
    public bool IsInGame { get; set; } = false;
    private bool isInitialized = false; // 중복 초기화 방지 플래그

    public Vector3 mousePosition;

    protected override void Awake()
    {
        base.Awake();

        Cursor.visible = false;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // ResourceManager.Instance가 아직 파괴되지 않고 존재하는 경우에만 리소스 해제 시도
        if (!string.IsNullOrEmpty(Paths.Prefabs.Cursor) && ResourceManager.Instance != null)
        {
            ResourceManager.Instance.UnloadAddressableAsset(Paths.Prefabs.Cursor);
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

        GameObject cursorPrefab = ResourceManager.Instance.GetLoadedAsset<GameObject>(Paths.Prefabs.Cursor);

        if (cursorPrefab == null)
        {
            Debug.LogError($"'{Paths.Prefabs.Cursor}' 주소의 커서 프리팹을 로드하는데 실패했습니다.");
            return;
        }

        // 로드한 프리팹 씬에 생성
        GameObject cursorObj = Instantiate(cursorPrefab);
        var renderer = cursorObj.GetComponent<SpriteRenderer>();
        renderer.sortingOrder = 500;

        inGameCursor = cursorObj.transform;
        inGameCursor.SetParent(CameraManager.Instance.MainCamera.transform, true); // 카메라의 자식으로 설정
        inGameCursor.gameObject.SetActive(false); // 일단 비활성화

        if (isInitialized || player == null)
        {
            return;
        }

        if (inGameCursor == null)
        {
            return;
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
        inGameCursor.gameObject.SetActive(isInGame);

        Cursor.visible = !isInGame;
        if (isInGame)
        {
            playerInputMap.Enable();
        }
        else
        {
            playerInputMap.Disable();
        }
    }

    private void LateUpdate()
    {
        if (inGameCursor == null)
        {
            //Debug.LogError("커서가 없삼");
            return;
        }
        mousePos = cursorInput.ReadValue<Vector2>();
        
        mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0f));
        mousePosition.z = 0f;
        inGameCursor.position = mousePosition;
    }

    public bool IsLeftThan(Transform transform)
    {
        /*Debug.Log($"[CursorManager] mousePosition.x = {mousePosition.x}, transform.x = {transform.position.x}");*/
        return mousePosition.x < transform.position.x;
    }
}
