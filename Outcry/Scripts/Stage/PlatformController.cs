using System.Collections;
using UnityEngine;

public class PlatformController : MonoBehaviour
{
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Vector2 checkBoxSize = new Vector2(2.5f, 0.1f);
    [SerializeField] private float checkOffsetY = 2f;
    [SerializeField] private float minStandTime = 0.5f; // 최소 착지 시간

    private SpriteRenderer spriteRenderer;
    private bool isTriggered = false;      // 코루틴이 중복 실행되는 것을 방지하기 위한 플래그

    private Coroutine checkLandingCoroutine = null;

    private void Awake()
    {
        playerLayer = LayerMask.GetMask("Player");
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 플레이어와 충돌했는지 확인
        // 코루틴이 이미 실행 중이 아닌지 확인 (isTriggered == false)
        // 플레이어가 위쪽에서 밟았는지 확인 (충돌 지점의 법선 벡터(normal)의 y값이 음수이면 위에서 온 것)
        // 플레이어가 위에서 닿았고 아직 파괴 과정이 시작되지 않았을 때만 착지 확인 시작
        if (collision.gameObject.CompareTag("Player")
            && !isTriggered
            && checkLandingCoroutine == null
            && collision.contacts[0].normal.y < -0.5f) // 위에서 닿았는지 법선 벡터로 확인
        {
            checkLandingCoroutine = StartCoroutine(CheckLanding());
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // 착지 확인 중에 떠났다면 확인 코루틴을 중지시켜 모든 것 리셋
            if (checkLandingCoroutine != null)
            {
                StopCoroutine(checkLandingCoroutine);
                checkLandingCoroutine = null;
            }
        }
    }

    private IEnumerator CheckLanding()
    {
        // 0.5초간 착지 상태를 유지하는지 기다림
        yield return new WaitForSeconds(minStandTime);

        // 기다린 후 플랫폼 바로 위에 센서를 만들어 플레이어가 여전히 있는지 최종 확인한다.
        Vector2 checkPosition = (Vector2)transform.position + new Vector2(0, checkOffsetY);
        Collider2D playerCollider = Physics2D.OverlapBox(checkPosition, checkBoxSize, 0f, playerLayer);

        // 센서 안에 플레이어가 감지되었다면 파괴 코루틴 시작
        if (playerCollider != null)
        {
            isTriggered = true; // 파괴 과정이 시작됐음을 표시하여 중복 실행 방지
            StartCoroutine(PlatformDestroy());
        }

        checkLandingCoroutine = null;
    }

    private IEnumerator PlatformDestroy()
    {
        isTriggered = true; // 코루틴이 시작되었음을 표시

        // 플레이어가 밟고 1초 대기
        yield return new WaitForSeconds(1f);

        // 4초 동안 깜빡이기
        float blinkDuration = 4f; // 깜빡임이 지속될 전체 시간
        float blinkTimer = 0f;

        // 깜빡임 간격의 시작 값과 끝 값 설정
        float startBlinkInterval = 0.4f; // 처음엔 0.4초 간격으로 깜빡임
        float endBlinkInterval = 0.05f;  // 마지막엔 0.05초 간격으로 매우 빠르게 깜빡임

        while (blinkTimer < blinkDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;

            // 진행도(0.0 ~ 1.0)에 따라 현재 깜빡임 간격 계산
            // progress가 0에 가까우면 0.4초에 가깝고 1에 가까워지면 0.05초에 가까워짐
            float progress = blinkTimer / blinkDuration;
            float currentInterval = Mathf.Lerp(startBlinkInterval, endBlinkInterval, progress);

            // 계산된 현재 간격만큼 대기
            yield return new WaitForSeconds(currentInterval);

            // 타이머에 고정된 값이 아닌 현재 간격을 더해줌
            blinkTimer += currentInterval;
        }

        // 4초 깜빡임이 끝나면 오브젝트 파괴
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        // 기즈모의 색상을 초록색으로 설정합니다.
        Gizmos.color = Color.green;

        // CheckLanding 코루틴에서 사용하는 것과 똑같은 위치와 크기로 계산합니다.
        Vector2 checkPosition = (Vector2)transform.position + new Vector2(0, checkOffsetY);

        // 계산된 위치에 와이어(선) 형태의 사각형을 그립니다.
        Gizmos.DrawWireCube(checkPosition, checkBoxSize);
    }
}
