using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class StoreUI : UIPopup
{
    [SerializeField] private Button buyBtn;
    [SerializeField] private Button exitBtn;


    [SerializeField] private SkillBtn btnPrefab;
    [SerializeField] private Transform btnParents;

    [SerializeField] private bool activeBtn = true; 
    [SerializeField] private ShowSkillPreview previewPlayer;

    [SerializeField] private TextMeshProUGUI infoText;

    [SerializeField] private ToggleGroup toggleGroup; //토글 자식들을 가져오기 위해서

    private SkillBtn _selectedBtn;
    private SkillData _selectedData;

    [SerializeField] private List<TextMeshProUGUI> soulCountTxts;

    private void Awake()
    {
        buyBtn.onClick.AddListener(Popupbuy);
        exitBtn.onClick.AddListener(Exit);
        buyBtn.interactable = true; //버튼 비활성화로 초기화
    }

    private void OnEnable()
    {
        GameManager.Instance.OnSoulCountChanged += HandleSoulCountChanged;
        // UI가 켜질 때 현재 소울 개수로 전체 업데이트
        UpdateAllSoulCountsUI();
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제(메모리 누수 방지)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnSoulCountChanged -= HandleSoulCountChanged;
        }
    }

    private void Start()
    {
        var skills = StoreManager.Instance.GetOrderedSkills();
        for (int i = 0; i < skills.Count; i++)
        {
            var icon = Instantiate(btnPrefab, btnParents);
            icon.name = $"Skill_{i + 1}";//스킬의 갯수 만큼 동적 생성
            icon.Bind(skills[i]); // ★ 여기서 바로 바인딩
            icon.SetBuyButton(buyBtn);

            icon.SetToggleGroup(toggleGroup);

            // 초기 이벤트 폭주 방지: 전부 OFF로 시작
            if (icon.toggle != null)
                icon.toggle.SetIsOnWithoutNotify(false);

            //선택 이벤트 구독
            icon.OnSelected += HandleSelected;
            buyBtn.interactable = true;

            icon.SetPreviewPlayer(previewPlayer);
        }
    }


    private void HandleSelected(SkillBtn sender, SkillData data)
    {
        _selectedBtn = sender;
        _selectedData = data;
        Debug.Log($" 선택됨: id={_selectedData.Skill_id}, name={_selectedData.P_Skill_Name}");
    }

    private void Buy()
    {
        if (_selectedData == null)
            return;
        int skillId = _selectedData.Skill_id;
        int soulId = _selectedData.NeedSoul;

        // 이미 가지고 있으면 종료
        if (GameManager.Instance.CurrentUserData.AcquiredSkillIds.Contains(skillId))
            return;
        if (!GameManager.Instance.TrySpendSouls(soulId, 1))
        {
            return;
        }


        EffectManager.Instance.PlayEffectByIdAndTypeAsync(UIEffectID.BuySkill, EffectType.Sound).Forget();

        GameManager.Instance.GainSkill(skillId);
    }

    private void Popupbuy()
    {
        Sprite skillSprite = GameManager.Instance.GetSprite(_selectedData.Skill_id);
        ConfirmUI popup = UIManager.Instance.Show<ConfirmUI>();
        popup.Setup(new ConfirmPopupData
        {
            Title = "",
            // Message = "Proceed with purchase?",
            Message = LocalizationUtility.GetLocalizedValueByKey(LocalizationStrings.Store.PURCHASECONFIRM),
            Type = EConfirmPopupType.SKILL_ACQUIRE_OK_CANCEL,
            ItemSprite = skillSprite,
            OnClickOK = () =>
            {
                Buy();
            }
        });
    }

    /// <summary>
    /// 모든 소울 개수 UI를 현재 데이터에 맞게 갱신
    /// </summary>
    private void UpdateAllSoulCountsUI()
    {
        // GameManager가 가진 모든 소울 데이터를 순회
        foreach (var soulData in GameManager.Instance.CurrentUserData.AcquiredSouls)
        {
            // UI 텍스트 리스트를 순회하며 일치하는 것을 찾음
            foreach (TextMeshProUGUI txt in soulCountTxts)
            {
                if (txt.name == soulData.SoulId.ToString())
                {
                    txt.text = $"X {soulData.Count}";
                    break; // 일치하는 UI를 찾았으니 내부 루프 탈출
                }
            }
        }
    }

    /// <summary>
    /// 소울 개수 변경 이벤트 발생 시 호출될 메서드
    /// </summary>
    private void HandleSoulCountChanged(int soulId, int newCount)
    {
        // UI 텍스트 리스트를 순회하며 일치하는 것을 찾음
        foreach (TextMeshProUGUI txt in soulCountTxts)
        {
            // soulId와 이름이 같은 TextMeshProUGUI를 찾음
            if (txt.name == soulId.ToString())
            {
                // 해당 텍스트만 업데이트
                txt.text = $"X {newCount}";
                Debug.Log($"StoreUI: Soul ID {soulId}의 개수를 {newCount}로 업데이트했습니다.");
                break; // 일치하는 UI를 찾았으니 루프 탈출
            }
        }
    }

    private void Exit()
    {
        UIManager.Instance.ClosePopupAndResumeGame<StoreUI>();
    }
}
