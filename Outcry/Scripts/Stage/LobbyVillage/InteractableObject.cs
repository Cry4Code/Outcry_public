using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private float yOffset = 0.5f; // 프롬프트의 Y축 오프셋

    private BoxCollider2D boxCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    public virtual void Interact()
    {
        // 상호작용 시 커서 변경
        CursorManager.Instance.SetInGame(false);
    }

    // 플레이어가 콜라이더 범위에 들어왔을 때
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (interactionPrompt != null)
        {
            // 콜라이더의 월드 공간 경계를 가져옴
            Bounds bounds = boxCollider.bounds;

            // 상단 중앙 위치 계산(중앙 x, 상단 y)
            // (bounds.center.y + bounds.extents.y)가 정확히 상단 가장자리
            float topY = bounds.center.y + bounds.extents.y + yOffset;
            Vector3 promptPosition = new Vector3(bounds.center.x, topY, 0);

            // 프롬프트 위치 설정
            interactionPrompt.transform.position = promptPosition;

            // 위치 설정한 뒤 활성화
            interactionPrompt.SetActive(true);
        }
    }

    // 플레이어가 콜라이더 범위에서 나갔을 때
    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }
}
