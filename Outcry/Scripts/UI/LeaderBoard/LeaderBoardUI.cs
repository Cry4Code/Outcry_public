using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardUI : UIPopup
{
    [SerializeField] private LeaderboardController controller;

    [Header("UI References")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private RankEntryItem rankEntryPrefab;

    [Header("Boss Buttons")]
    [SerializeField] private Button goblinKingButton;
    [SerializeField] private Button witchButton;
    [SerializeField] private Button vampireLordButton;

    [SerializeField] private Button exitButton;

    [SerializeField] private GameObject scrollViewObject;
    [SerializeField] private GameObject emptyMessageObject;
    [SerializeField] private GameObject loadingImgObject;

    private const string GoblinKingLeaderboardId = "goblin_king_clear_time";
    private const string WitchLeaderboardId = "phantom_witch_clear_time";
    private const string VampireLordLeaderboardId = "vampire_lord_clear_time";

    // 오브젝트 풀링을 위한 변수
    private Queue<RankEntryItem> pool = new Queue<RankEntryItem>();
    private List<RankEntryItem> activeEntries = new List<RankEntryItem>();

    void Start()
    {
        controller.OnDataFetchStarted += ShowLoadingState;
        controller.OnDataUpdated += RefreshUI;

        goblinKingButton.onClick.AddListener(() => controller.RequestLeaderboard(GoblinKingLeaderboardId));
        witchButton.onClick.AddListener(() => controller.RequestLeaderboard(WitchLeaderboardId));
        vampireLordButton.onClick.AddListener(() => controller.RequestLeaderboard(VampireLordLeaderboardId));
        exitButton.onClick.AddListener(OnClickExitBtn);

        goblinKingButton.onClick.Invoke();
    }

    private void OnDestroy()
    {
        controller.OnDataUpdated -= RefreshUI;
    }

    private void ShowLoadingState()
    {
        loadingImgObject.SetActive(true);
        scrollViewObject.SetActive(false);
        emptyMessageObject.SetActive(false);
    }

    private void RefreshUI()
    {
        loadingImgObject.SetActive(false);

        // 현재 활성화된 모든 항목 풀에 반환
        foreach (var entry in activeEntries)
        {
            entry.gameObject.SetActive(false); // 비활성화
            pool.Enqueue(entry);              // 풀에 다시 넣기
        }
        activeEntries.Clear(); // 활성화 목록 초기화

        // 컨트롤러의 새 데이터로 UI를 다시 그림
        if (controller.CurrentScores == null)
        {
            return;
        }

        // 데이터가 있는지 없는지 확인
        bool hasScores = controller.CurrentScores != null && controller.CurrentScores.Count > 0;

        // 상태에 따라 UI 설정
        scrollViewObject.SetActive(hasScores); // 점수가 있으면 스크롤뷰 켜고
        emptyMessageObject.SetActive(!hasScores); // 점수가 없으면 안내 메시지 활성화

        if(hasScores)
        {
            foreach (var scoreData in controller.CurrentScores)
            {
                // 풀에서 아이템 가져옴
                RankEntryItem newEntry = GetEntryFromPool();
                newEntry.SetData(scoreData);
                activeEntries.Add(newEntry);
            }
        }
    }

    /// <summary>
    /// 풀에서 RankEntryItem을 가져오는 메서드. 풀이 비어있으면 새로 생성
    /// </summary>
    private RankEntryItem GetEntryFromPool()
    {
        RankEntryItem entry;

        if (pool.Count > 0)
        {
            // 풀에 재고가 있으면 꺼내서 사용
            entry = pool.Dequeue();
        }
        else
        {
            // 풀이 비어있으면 새로 생성(Instantiate는 여기서만 호출)
            entry = Instantiate(rankEntryPrefab, contentParent);
        }

        entry.gameObject.SetActive(true); // 활성화

        entry.transform.SetAsLastSibling(); // 계층 구조상 가장 마지막으로 이동

        return entry;
    }

    public void OnClickExitBtn()
    {
        UIManager.Instance.ClosePopupAndResumeGame<LeaderboardUI>();
    }
}
