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

    [SerializeField] private TextMeshProUGUI infoText;

    [SerializeField] private ToggleGroup toggleGroup; //토글 자식들을 가져오기 위해서

    private SkillBtn _selectedBtn;
    private SkillData _selectedData;


    private void Awake()
    {
        buyBtn.onClick.AddListener(Buy);
        exitBtn.onClick.AddListener(Exit);
        buyBtn.interactable = false; //버튼 비활성화로 초기화


    }

    private void Start()
    {
        var skills = StoreManager.Instance.GetOrderedSkills();
        for (int i = 0; i < skills.Count; i++)
        {

            var icon = Instantiate(btnPrefab, btnParents);
            icon.name = $"Skill_{i + 1}";//스킬의 갯수 만큼 동적 생성
            icon.Bind(skills[i]); // ★ 여기서 바로 바인딩
            icon.SetOutputText(infoText);
            //icon.SetPreviewPlayer(previewPlayer);          // ★ 프리뷰 재생기 주입

            icon.SetBuyButton(buyBtn);

            // ★핵심: 선택 이벤트 구독
            icon.OnSelected += HandleSelected;

            buyBtn.interactable = false;

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

        GameManager.Instance.GainSkill(skillId);
    }

    private void Exit()
    {
        //스토어 나가는 코드
        UIManager.Instance.Hide<StoreUI>();
        CursorManager.Instance.SetInGame(true);
        PlayerManager.Instance.player.PlayerInputEnable();
    }


    private void ActPreview(int btnNum)
    {
        //버튼이 활성화 되어있다면
        //스킬을 해금하는데 필요한 소울을 가지고 있다면 해당 소울을
        //만약 다른 스킬이 미리보기중이면 종료 -->토글을 통해서 해결
        //누른 스킬 번호를 받아와서 미리보기창에 반복 재생
    }


}
