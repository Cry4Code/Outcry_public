using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class AchievementsBar : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI conditionsText;
    [SerializeField] private TextMeshProUGUI counts;
    [SerializeField] private Image gradeImage;
    [SerializeField] private TextMeshProUGUI percentText;
    [SerializeField] private Image progressFill;

    [Serializable]
    public struct GradeSpritePair
    {
        public string grade;   
        public Sprite sprite;
    }

    [Header("Grade ↔ Sprite 매핑")]
    [SerializeField] private List<GradeSpritePair> gradeSpriteTable = new();

    private Dictionary<string, Sprite> _gradeMap;

    private string _cachedValue;  // 마지막으로 받은 문자열 캐싱

    private void Awake()
    {
        // 대소문자 무시 딕셔너리 구성
        _gradeMap = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in gradeSpriteTable)
        {
            if (!string.IsNullOrWhiteSpace(e.grade))
                _gradeMap[e.grade.Trim()] = e.sprite;
        }
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
        counts.text = target.ToString()+"PT";
    }

    private void Apply(string value)
    {
        if (conditionsText == null) return;

        conditionsText.text = value;
        conditionsText.ForceMeshUpdate();
        Canvas.ForceUpdateCanvases();
    }

    public void SetGrade(string grade)
    {
        if (gradeImage == null) return;

        if (string.IsNullOrWhiteSpace(grade))
        {
            gradeImage.enabled = false;
            return;
        }

        if (_gradeMap != null && _gradeMap.TryGetValue(grade.Trim(), out var sp) && sp != null)
        {
            gradeImage.enabled = true;
            gradeImage.sprite = sp;
            gradeImage.preserveAspect = true;
        }
        else
        {
            // 매핑 없으면 숨김(또는 기본 스프라이트를 하나 만들어 넣어도 됨)
            gradeImage.enabled = false;
        }
    }
    public void SetPercent(int percent)
    {
        if (!percentText) return;
        percentText.text = $"{percent}%";

        //퍼센트로 막대도 함께 갱신하고 싶다면 아래 한 줄 유지
        SetProgress01(percent / 100f);
    }

    public void SetProgress01(float ratio01)
    {
        if (!progressFill) return;
        progressFill.type = Image.Type.Filled;                 // 안전 보정
        progressFill.fillAmount = Mathf.Clamp01(ratio01);      // 0~1
    }

}
