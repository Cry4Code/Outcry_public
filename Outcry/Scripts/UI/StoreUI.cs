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

    //[SerializeField] private ShowSkillPreview previewPlayer; // 프리뷰 오브젝트(씬) 드래그

    private void Awake()
    {
        buyBtn.onClick.AddListener(Buy);
        exitBtn.onClick.AddListener(Exit);

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

            //스킬을 해금하는데 필요한 소울을 가지고&& 스킬을 아직 구매하지 않은 상태라면
            //해당 소울로 해금할 수 있는 버튼 활성화, 나머지 비활성화

        }

        if (CanBuy() == true)
        {
            buyBtn.interactable = true;   // 버튼 활성화
        }
        else
        {
            buyBtn.interactable = false;  // 버튼 비활성화
        }

        //StoreManager.Instance.BindSkillButtonsUnder(btnParents);
    }

    private void Buy()
    {
        //스킬 미리보기 버튼이 눌려있으면 해당 스킬 구매

        //유저 정보에 스킬 추가 하기
    }

    private void Exit()
    {
        //스토어 나가는 코드
        UIManager.Instance.Hide<StoreUI>();
        CursorManager.Instance.SetInGame(true);
        PlayerManager.Instance.player.PlayerInputEnable();
    }

    private bool CanBuy(/*매개변수로 스킬 번호 또는 스킬 내부 데이터 가져옴*/)
    {
        if (activeBtn == true/*임시 코드*/) //스킬을 해금하는데 필요한 소울을 가지고&& 스킬을 아직 구매하지 않은 상태라면
              //해당 소울로 해금할 수 있는 버튼 활성화, 나머지 비활성화
        {
            return activeBtn;
        }
        return false;
    }

    private void ActPreview(int btnNum)
    {
        //버튼이 활성화 되어있다면
        //스킬을 해금하는데 필요한 소울을 가지고 있다면 해당 소울을
        //만약 다른 스킬이 미리보기중이면 종료 -->토글을 통해서 해결
        //누른 스킬 번호를 받아와서 미리보기창에 반복 재생
    }


}
