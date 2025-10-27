using Newtonsoft.Json;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Leaderboards.Models;
using UnityEditor.Rendering;
using UnityEngine;

public class RankEntryItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI scoreText;

    public void SetData(LeaderboardEntry entry)
    {
        rankText.text = (entry.Rank + 1).ToString();

        string characterNameToDisplay = entry.PlayerName; // 기본값으로 계정 이름 설정

        // Metadata 문자열이 비어있지 않은지 확인
        if (!string.IsNullOrEmpty(entry.Metadata))
        {
            try
            {
                // JSON 문자열 Dictionary<string, string>로 역직렬화
                var metadataDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(entry.Metadata);

                if (metadataDict != null && metadataDict.TryGetValue("characterName", out string characterName))
                {
                    characterNameToDisplay = characterName; // 캐릭터 이름으로 교체
                }
            }
            catch (JsonException e)
            {
                // JSON 파싱 실패 시 기본 이름 사용
                Debug.LogError($"Failed to parse metadata JSON: {entry.Metadata}. Error: {e.Message}");
            }
        }

        nameText.text = characterNameToDisplay; // 최종 결정된 이름 표시

        var timeSpan = System.TimeSpan.FromSeconds(entry.Score);
        scoreText.text = string.Format("{0:D2}:{1:D2}.{2:D3}",
            timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
    }
}