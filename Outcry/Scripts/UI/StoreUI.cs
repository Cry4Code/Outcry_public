using Cysharp.Threading.Tasks;
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


    private void Awake()
    {
        buyBtn.onClick.AddListener(Popupbuy);
        exitBtn.onClick.AddListener(Exit);
        buyBtn.interactable = true; //버튼 비활성화로 초기화


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
            Message = "Proceed with purchase?",
            Type = EConfirmPopupType.SKILL_ACQUIRE_OK_CANCEL,
            ItemSprite = skillSprite,
            OnClickOK = () =>
            {
                Buy();
            }
        });
    }


    private void Exit()
    {
        //스토어 나가는 코드
        UIManager.Instance.Hide<StoreUI>();
        CursorManager.Instance.SetInGame(true);
        PlayerManager.Instance.player.PlayerInputEnable();
    }
}
