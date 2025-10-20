using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSelectUI : UIPopup
{
    //[SerializeField] private SelectBtn;
    [SerializeField] private Button exitBtn;

    [SerializeField] private SkillSelectBtn btnPrefab;
    [SerializeField] private Transform btnParents;

    [SerializeField] private bool activeBtn = true;
    [SerializeField] private TextMeshProUGUI infoText;

    [SerializeField] private Button OnSkill;
    [SerializeField] private Button OnAchieve;

    [SerializeField] private GameObject SkillWindow;
    [SerializeField] private GameObject AchieveWindow;

    [SerializeField] private GameObject SelectedSkill;

    [SerializeField] private Image sidePreviewImage;


    [System.Serializable] public struct SkillIconPair { public int id; public Sprite icon; }

    [Header("Icon Mapping")]
    [SerializeField] private List<SkillIconPair> iconPairs = new List<SkillIconPair>();

    private Dictionary<int, Sprite> iconMap;

    private SkillSelectBtn _selectedBtn;
    private SkillData _selectedData;


    private void Awake()
    {
        //buyBtn.onClick.AddListener(Buy);
        exitBtn.onClick.AddListener(Exit);
        OnAchieve.onClick.AddListener(OnAchieveWindow);
        OnSkill.onClick.AddListener(OnSkillWindow);

        // iconMap은 최소한 빈 딕셔너리로
        iconMap = new Dictionary<int, Sprite>();
        foreach (var p in iconPairs)
        {
            if (p.icon != null)
                iconMap[p.id] = p.icon; // 같은 id가 있으면 마지막이 유효
        }
    }

    private void Start()
    {
        var skills = StoreManager.Instance.GetOrderedSkills();
        for (int i = 0; i < skills.Count; i++)
        {

            var btn = Instantiate(btnPrefab, btnParents);
            btn.name = $"Skill_{i + 1}";//스킬의 갯수 만큼 동적 생성
            btn.Bind(skills[i]); // ★ 여기서 바로 바인딩
            btn.SetOutputText(infoText);

            // 2) 아이콘 조회용 변수 (한 번만 선언)
            Sprite iconSprite = null;

            // 스킬 ID로 먼저 시도
            if (iconMap != null)
            {
                iconMap.TryGetValue(skills[i].Skill_id, out iconSprite);

                // (옵션) 못 찾으면 NeedSoul로 폴백
                if (iconSprite == null)
                    iconMap.TryGetValue(skills[i].NeedSoul, out iconSprite);

            }

            // 3) 프리팹 내부 이미지에 주입
            btn.SetIcon(iconSprite); // null이면 기존 sprite 유지

            btn.SetLinkedExternalImage(sidePreviewImage);

            btn.OnSelected += HandleSelected;//  추가: 버튼의 "선택 이벤트"를 구독
        }

        DisableUnownedSkillToggles();

    }

    private void OnEnable()
    {
        var user = GameManager.Instance.CurrentUserData;

        if (user == null || user.AcquiredSkillIds == null || user.AcquiredSkillIds.Count == 0)
        {
            Debug.Log("[SkillSelectUI] 유저가 가진 스킬 없음");
        }
        else
        {
            Debug.Log("[SkillSelectUI] 유저가 가진 스킬 ID 목록: " +
                string.Join(", ", user.AcquiredSkillIds));
        }
        DisableUnownedSkillToggles(); // ← 여기서 매번 갱신
    }

    private void SetOwnedVisualOverlay(SkillSelectBtn btn, bool owned)
    {
        btn.toggle.interactable = owned;
        btn.SetOverlayActive(!owned);
    }


    private void Exit()
    {
        //스토어 나가는 코드
        UIManager.Instance.Hide<SkillSelectUI>();
        CursorManager.Instance.SetInGame(true);
        PlayerManager.Instance.player.PlayerInputEnable();
    }

    // ★ 추가: 이벤트 핸들러 — 눌린 토글 정보 저장
    private void HandleSelected(SkillSelectBtn sender, SkillData data)
    {
        _selectedBtn = sender;
        _selectedData = data;
        // 필요하면 이후 단계에서 _selectedData를 사용해 Buy 등 연결 가능
    }

    private void OnSkillWindow()
    {
        SkillWindow.SetActive(true);
        AchieveWindow.SetActive(false);
        SelectedSkill.SetActive(true);
    }

    private void OnAchieveWindow()
    {
        SkillWindow.SetActive(false);
        AchieveWindow.SetActive(true);
        SelectedSkill.SetActive(false);
    }

    private void DisableUnownedSkillToggles()
    {
        var user = GameManager.Instance.CurrentUserData;
        if (user == null) return;

        // 현재 UI 내부 버튼 전체 조회
        var allBtns = btnParents.GetComponentsInChildren<SkillSelectBtn>(includeInactive: true);

        foreach (var btn in allBtns)
        {
            if (btn.Data == null) continue;

            bool owned = user.AcquiredSkillIds != null &&
                         user.AcquiredSkillIds.Contains(btn.Data.Skill_id);

            // 소유하지 않은 스킬은 클릭 불가로 만든다
            SetOwnedVisualOverlay(btn, owned);
        }
    }

}
