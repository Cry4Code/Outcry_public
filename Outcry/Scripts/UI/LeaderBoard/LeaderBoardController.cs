using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Services.Leaderboards.Models;
using UnityEngine;

public class LeaderboardController : MonoBehaviour
{
    // 데이터 로딩 시작을 UI에 알려주는 이벤트
    public event Action OnDataFetchStarted;
    // 데이터 로딩이 완료되면 UI에 알려줌
    public event Action OnDataUpdated;

    public List<LeaderboardEntry> CurrentScores { get; private set; }

    private bool isFetching = false;
    private const int FetchLimit = 20; // 한 번에 상위 20명만 가져옴

    // UI의 버튼에서 호출될 메서드
    public void RequestLeaderboard(string leaderboardId)
    {
        // 이미 데이터를 가져오는 중이면 중복 요청 방지
        if (isFetching)
        {
            return;
        }

        // 비동기 작업 시작 직전에 로딩 시작 이벤트 발생
        OnDataFetchStarted?.Invoke();

        FetchScoresAsync(leaderboardId).Forget();
    }

    private async UniTask FetchScoresAsync(string leaderboardId)
    {
        isFetching = true;

        // UGSManager를 통해 상위 랭킹 데이터 요청
        CurrentScores = await UGSManager.Instance.GetLeaderboardScoresAsync(leaderboardId, FetchLimit);

        if (CurrentScores == null)
        {
            // 데이터 로딩 실패 시 빈 리스트로 초기화
            CurrentScores = new List<LeaderboardEntry>();
            Debug.LogError($"Failed to fetch scores for {leaderboardId}");
        }

        isFetching = false;

        // 데이터가 준비되었음을 UI에 알림
        OnDataUpdated?.Invoke();
    }
}