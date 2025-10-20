using UnityEngine;

public class CameraAutoScroller : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 1.5f;
    private bool isScrolling = false;
    private GameObject deathPlane;

    private void Awake()
    {
        deathPlane = GetComponentInChildren<DeathPlaneController>(true)?.gameObject;

        if (deathPlane != null)
        {
            deathPlane.SetActive(false);
        }
    }

    private void OnEnable()
    {
        EventBus.Subscribe(EventBusKey.ChangePlayerDead, OnPlayerDiedHandler);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(EventBusKey.ChangePlayerDead, OnPlayerDiedHandler);
    }

    private void Update()
    {
        if (isScrolling)
        {
            transform.Translate(Vector3.up * scrollSpeed * Time.deltaTime);
        }
    }

    public void StartScroll()
    {
        isScrolling = true;
        if (deathPlane != null)
        {
            deathPlane.SetActive(true);
        }
    }

    public void StopScroll()
    {
        isScrolling = false;
        if (deathPlane != null)
        {
            deathPlane.SetActive(false);
        }
    }

    /// <summary>
    /// 플레이어 사망 이벤트가 발생했을 때 호출될 핸들러 메서드
    /// </summary>
    /// <param name="data">이벤트와 함께 전달된 데이터 (이 경우 true)</param>
    private void OnPlayerDiedHandler(object data)
    {
        // 플레이어가 사망했다는 신호(true)를 받으면
        if ((bool)data)
        {
            Debug.Log("플레이어 사망 신호 수신. 카메라 스크롤을 중지합니다.");
            // 즉시 스크롤을 멈춤
            StopScroll();
        }
    }
}
