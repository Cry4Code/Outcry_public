using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDUI : UIBase
{
    //체력바 -체력이 채워지는 칸
    //체력칸 -실제 체력을 보여주는 붉은 칸
    //체력바 는 항상 남아있음(안에 체력칸만 사라짐)
    [SerializeField] private UnityEngine.UI.Image staminaFill;   // 스태미너 바 이미지
    [SerializeField] private TextMeshProUGUI playerName;         // 플레이어 이름 텍스트
    [SerializeField] private Image healthcase;                   // 체력바 테두리용 이미지 (Sliced 가능)
    [SerializeField] private RectTransform healthFillRect;       // ★ 변경: 체력칸(RectTransform 직접 제어)
    [SerializeField] private Animator animator;

    private bool prevUnder50;


    [SerializeField] private FaceUI faceUI;                      // 얼굴 UI (죽을 때 애니메이션 재생)

    private int maxHealth;
    private int maxStamina;

    private int beforeChangeHealth;
    private int beforeChangeStamina;

    private Coroutine coHealthLerp;  // ★ 추가: 체력바 부드러운 줄이기용 코루틴
    private float fullHealthWidth;   // ★ 추가: 초기 체력바 전체 길이 저장용



    private void OnEnable()
    {
        // 체력 / 스태미너 변경 이벤트 구독
        EventBus.Subscribe(EventBusKey.ChangeHealth, OnHealthChanged);
        EventBus.Subscribe(EventBusKey.ChangeStamina, OnStaminaChanged);
    }

    private void OnDisable()
    {
        // 구독 해제
        EventBus.Unsubscribe(EventBusKey.ChangeHealth, OnHealthChanged);
        EventBus.Unsubscribe(EventBusKey.ChangeStamina, OnStaminaChanged);
    }

    //최대 체력만큼 체력바 생성, 체력칸 채움
    private void Awake()
    {
        // 필요 시 초기화 로직 작성 가능
    }

    private void Start()
    {
        // 최대 체력 / 스태미너 값 가져오기
        maxHealth = PlayerManager.Instance.player.Data.maxHealth;
        maxStamina = PlayerManager.Instance.player.Data.maxStamina;

        // 체력/스태미너 변경 이전 값 기록
        beforeChangeHealth = maxHealth;
        beforeChangeStamina = maxStamina;

        // ★ 추가: 체력바 RectTransform의 초기 길이 저장 (전체 체력 기준)
        if (healthFillRect != null)
            fullHealthWidth = healthFillRect.sizeDelta.x;


        // 닉네임 표시
        if (UGSManager.Instance.IsAnonymousUser)
        {
            playerName.text = "Guest";
        }
        else
        {
            playerName.text = GameManager.Instance.CurrentUserData.Nickname;
        }
    }

    // 체력 변경 이벤트가 발생했을 때 호출됨
    private void OnHealthChanged(object value)
    {
        int changedHealth = (int)value;
        Debug.Log($"[로그] 체력 변경 이벤트: {changedHealth}/{maxHealth}");

        // 변경 전 / 후 체력 비교 및 UI 반영
        ChangeHealth(beforeChangeHealth, changedHealth);

        // 이전 체력 갱신
        beforeChangeHealth = changedHealth;
    }

    // 스태미너 변경 이벤트가 발생했을 때 호출됨
    private void OnStaminaChanged(object value)
    {
        int changedStamina = (int)value;
        ChangeStamina(beforeChangeStamina, changedStamina);
        Debug.Log($"[로그] 스태미나 변경: {changedStamina}/{maxStamina}");

        // 이전 스태미너 갱신
        beforeChangeStamina = changedStamina;
    }

    // 체력 변경 시 실행되는 메서드
    private void ChangeHealth(int beforeChangeHealth, int changedHealth)
    {
        Debug.Log("체인지 헬스 호출됨");

        int newHealth = Mathf.Clamp(changedHealth, 0, maxHealth);
        if (newHealth == beforeChangeHealth) return;   // 체력 변화 없으면 리턴

        // ★ 변경: fillAmount 대신 RectTransform의 X 길이로 조절
        if (healthFillRect == null) return;

        // 체력 비율 계산 (0~1 사이)
        float ratio = Mathf.Clamp01((float)newHealth / Mathf.Max(1, maxHealth));
        float targetWidth = fullHealthWidth * ratio; // 전체 길이에 비례한 새 길이 계산

        bool isUnder50 = ratio < 0.5f;
        if (isUnder50 != prevUnder50)
        {
            animator.SetBool("Under50", isUnder50);
            prevUnder50 = isUnder50;
        }


        // ★ 변경: 코루틴으로 부드럽게 줄이기
        if (coHealthLerp != null)
        {

            StopCoroutine(coHealthLerp);
        }
        coHealthLerp = StartCoroutine(LerpHealthWidth(targetWidth, 0.2f));

   

        if (newHealth <= 0)
        {
            // 얼굴 UI 깨짐 애니메이션 재생
            faceUI?.Break();
        }


    }

    // ★ 추가: 체력바의 너비를 부드럽게 줄이는 보간 코루틴
    private IEnumerator LerpHealthWidth(float targetWidth, float duration)
    {
        float startWidth = healthFillRect.sizeDelta.x;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            // t 값을 0~1로 정규화
            float t = Mathf.Clamp01(time / duration);

            // 살짝 자연스러운 S-curve (가속-감속 형태)
            t = t * t * (3f - 2f * t);

            // 현재 프레임에서의 새 너비 계산
            float newWidth = Mathf.Lerp(startWidth, targetWidth, t);
            healthFillRect.sizeDelta = new Vector2(newWidth, healthFillRect.sizeDelta.y);

            yield return null;
        }

        // 마지막에는 목표 길이로 고정
        healthFillRect.sizeDelta = new Vector2(targetWidth, healthFillRect.sizeDelta.y);
    }

    // 스태미너 게이지 변경 시 실행되는 메서드
    private void ChangeStamina(int beforeChangeStamina, int changedStamina)
    {
        // 스태미나는 기존과 동일: fillAmount로 즉시 반영
        float ratio = (float)changedStamina / Mathf.Max(1, maxStamina);
        staminaFill.fillAmount = ratio;

        Debug.Log($" 현재 스태미너 비율 {ratio}");
    }

}
