using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // LayoutRebuilder

public class AchievementScreen : MonoBehaviour
{
    [SerializeField] private AchievementsBar barPrefab;   // 인스펙터에서 지정
    [SerializeField] private Transform contentParent;      // Vertical ScrollView의 Content

    private readonly List<GameObject> spawned = new();

    private void OnEnable()
    {
        Rebuild();
    }

    private void OnDisable()
    {
        Clear();
    }

    private void Rebuild()
    {
        if (barPrefab == null || contentParent == null)
        {
            Debug.LogError("[AchievementScreen] barPrefab / contentParent 할당 필요");
            return;
        }

        Clear();

        var entries = AchievementManager.Instance.GetAllAchievementsSorted();
        if (entries == null || entries.Count == 0) return;

        foreach (var entry in entries)
        {
            var bar = Instantiate(barPrefab, contentParent);
            bar.name = $"Achievement_{entry.id}";

            // 설명(Conditions) 먼저
            var label = string.IsNullOrWhiteSpace(entry.data.Conditions)
                ? $"목표: {entry.data.Counts}"
                : entry.data.Conditions;
            bar.SetConditions(label);

            // ★ 목표값(Condition) 숫자도 별도 TMP에 표시
            bar.SetTarget(entry.data.Counts);

            // ★ 나중에 AchievementsBar에 초기화 메서드를 만들면 여기서 호출
            // var bar = go.GetComponent<AchievementsBar>();
            // bar.SetData(entry.id, entry.data); // 원하는 시그니처로 추가하면 됨

            spawned.Add(bar.gameObject);
        }

        // 레이아웃 즉시 갱신 (스크롤 컨텐츠 높이 반영)
        if (contentParent is RectTransform rt)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }
    }

    private void Clear()
    {
        // 추적 리스트 기반 제거
        for (int i = 0; i < spawned.Count; i++)
        {
            if (spawned[i] != null) Destroy(spawned[i]);
        }
        spawned.Clear();

        // 혹시 외부에서 자식이 추가된 경우까지 깔끔히 초기화하고 싶다면:
        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }
    }
}
