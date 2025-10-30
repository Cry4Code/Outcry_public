using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QTEController : MonoBehaviour
{
    public enum EQTEState { Idle, InProgress, Success, Failure }
    public EQTEState CurrentState { get; private set; } = EQTEState.Idle;

    // QTE 키를 표시할 UI Text/Image들 연결(이미지로 변경?)
    [SerializeField] private List<TextMeshProUGUI> keyPrefabs;
    [SerializeField] private RectTransform keyParent;

    private string requiredSequence; // "QWER" 등 필요한 키 순서
    private int currentIndex;
    private float timeLimitPerKey = 2.0f; // 키 하나당 제한 시간
    private Coroutine timeoutCoroutine;

    // 문자와 프리팹을 매핑하는 딕셔너리
    private Dictionary<char, TextMeshProUGUI> keyPrefabMap = new Dictionary<char, TextMeshProUGUI>();
    // 실제로 생성된 UI 오브젝트들을 관리하는 리스트
    private List<TextMeshProUGUI> instantiatedKeyTexts = new List<TextMeshProUGUI>();

    private void Awake()
    {
        // 미리 프리팹들을 딕셔너리에 매핑하여 검색 속도 높임
        foreach (var prefab in keyPrefabs)
        {
            // 프리팹의 이름을 기준으로 문자 추출
            if (!string.IsNullOrEmpty(prefab.name))
            {
                char key = prefab.name.ToUpper()[0];
                if (!keyPrefabMap.ContainsKey(key))
                {
                    keyPrefabMap.Add(key, prefab);
                }
            }
        }
    }

    /// <summary>
    /// 새로운 QTE 시작
    /// </summary>
    public void StartNewQTE(string sequence)
    {
        requiredSequence = sequence.ToUpper();
        currentIndex = 0;
        CurrentState = EQTEState.InProgress;
        Debug.Log($"QTE 시작! 입력 순서: {requiredSequence}");

        // UI 초기화
        GenerateQTEUI();
        UpdateUI();

        // 타임아웃 코루틴 시작
        if (timeoutCoroutine != null) StopCoroutine(timeoutCoroutine);
        timeoutCoroutine = StartCoroutine(TimeoutRoutine());
    }

    private void Update()
    {
        if (CurrentState != EQTEState.InProgress) return;

        // 키 입력 감지
        if (Input.anyKeyDown)
        {
            // 올바른 키를 눌렀는지 확인
            if (Input.GetKeyDown(requiredSequence[currentIndex].ToString().ToLower()))
            {
                EffectManager.Instance.PlayEffectByIdAndTypeAsync(Stage3BossEffectID.BloodMoon * 10 + 5,
                    EffectType.Sound).Forget();
                currentIndex++;
                Debug.Log($"QTE 성공: {requiredSequence[currentIndex - 1]}");
                UpdateUI(); // UI 업데이트 (예: 누른 키 색상 변경)

                // 타임아웃 재시작
                if (timeoutCoroutine != null)
                {
                    StopCoroutine(timeoutCoroutine);
                }
                timeoutCoroutine = StartCoroutine(TimeoutRoutine());

                // 모든 키를 다 눌렀다면 최종 성공
                if (currentIndex >= requiredSequence.Length)
                {
                    StartCoroutine(QTESuccessSound());
                    CurrentState = EQTEState.Success;
                    Debug.Log("QTE 최종 성공!");
                    if (timeoutCoroutine != null)
                    {
                        StopCoroutine(timeoutCoroutine);
                    }
                }
            }
            // 잘못된 키를 눌렀다면 즉시 실패
            else
            {
                EffectManager.Instance.PlayEffectByIdAndTypeAsync(Stage3BossEffectID.BloodMoon * 10 + 7,
                    EffectType.Sound).Forget();
                CurrentState = EQTEState.Failure;
                Debug.Log("QTE 실패: 잘못된 키 입력");
                if (timeoutCoroutine != null)
                {
                    StopCoroutine(timeoutCoroutine);
                }
            }
        }
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    private IEnumerator TimeoutRoutine()
    {
        yield return new WaitForSeconds(timeLimitPerKey);
        // 제한 시간 안에 키를 누르지 못했다면 실패
        if (CurrentState == EQTEState.InProgress)
        {
            CurrentState = EQTEState.Failure;
            Debug.Log("QTE 실패: 시간 초과");
        }
    }

    private IEnumerator QTESuccessSound()
    {
        yield return new WaitForSeconds(0.5f);
        EffectManager.Instance.PlayEffectByIdAndTypeAsync(Stage3BossEffectID.BloodMoon * 10 + 6,
            EffectType.Sound).Forget();
    }

    /// <summary>
    /// QTE UI 동적으로 생성
    /// </summary>
    private void GenerateQTEUI()
    {
        // 이전에 생성된 UI가 있다면 모두 파괴
        ClearQTEUI();

        // requiredSequence의 각 문자에 해당하는 프리팹 생성
        foreach (char keyChar in requiredSequence)
        {
            if (keyPrefabMap.TryGetValue(keyChar, out TextMeshProUGUI prefab))
            {
                TextMeshProUGUI newKeyText = Instantiate(prefab, keyParent);
                instantiatedKeyTexts.Add(newKeyText);
            }
            else
            {
                Debug.LogWarning($"'{keyChar}'에 해당하는 키 프리팹을 찾을 수 없습니다.");
            }
        }
    }

    /// <summary>
    /// 생성된 모든 QTE UI 제거
    /// </summary>
    private void ClearQTEUI()
    {
        foreach (var text in instantiatedKeyTexts)
        {
            Destroy(text.gameObject);
        }
        instantiatedKeyTexts.Clear();
    }

    /// <summary>
    /// 실제로 생성된 UI 색상 업데이트
    /// </summary>
    private void UpdateUI()
    {
        for (int i = 0; i < instantiatedKeyTexts.Count; i++)
        {
            // 입력에 성공한 키는 초록색, 나머지는 흰색으로 표시(예시)
            instantiatedKeyTexts[i].color = (i < currentIndex) ? Color.green : Color.white;
        }
    }

    // QTE UI 없애는 코루틴
    public IEnumerator FadeOut(float duration)
    {
        // TODO: CanvasGroup 등을 이용해 UI 전체를 서서히 투명하게 만드는 로직 구현
        yield return new WaitForSeconds(duration);
        gameObject.SetActive(false); // 비활성화
    }
}