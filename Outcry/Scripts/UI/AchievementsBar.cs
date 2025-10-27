using TMPro;
using UnityEngine;

public class AchievementsBar : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI conditionsText;
    [SerializeField] private TextMeshProUGUI counts;

    private string _cachedValue;  // 마지막으로 받은 문자열 캐싱

    private void Awake()
    {

    }

    private void Start()
    {
        // OnEnable 이후 다른 초기화가 덮어썼을 경우 Start에서 다시 확정 적용
        if (!string.IsNullOrEmpty(_cachedValue))
            Apply(_cachedValue);
    }

    public void SetConditions(string conditions)
    {
        _cachedValue = string.IsNullOrEmpty(conditions) ? "-" : conditions;
        Apply(_cachedValue); // 즉시 1차 적용
    }

    public void SetTarget(int target)   // ★ 추가: Condition 숫자 출력
    {
        if (!counts) return;            // 인스펙터 연결 누락 방지
        counts.text = target.ToString();
    }

    private void Apply(string value)
    {
        if (conditionsText == null) return;

        conditionsText.text = value;
        conditionsText.ForceMeshUpdate();
        Canvas.ForceUpdateCanvases();
    }
}
