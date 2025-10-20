using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SkillBtn : MonoBehaviour
{

    [SerializeField] public Toggle toggle;
    [SerializeField] private TextMeshProUGUI outputText;

    [SerializeField] private Button buyBtn;

    public event Action<SkillBtn, SkillData> OnSelected;  // (sender, data)


    public void SetBuyButton(Button button)
    {
        buyBtn = button;
    }
    // 프리팹의 Toggle
    public SkillData Data { get; private set; }
    public void SetOutputText(TextMeshProUGUI t) { outputText = t; }



    private void Awake()
    {

        toggle.onValueChanged.AddListener(_ => Debug.Log("[RAW] toggle changed"));

        var t = GetComponent<Toggle>(); // 프리팹에서 씬을 참조할 수 없어서 직접 부모 오브젝트의 토글 그룹을 추가함
        if (t != null && t.group == null)
            t.group = GetComponentInParent<ToggleGroup>(); // 가장 가까운 부모의 그룹 자동 연결

        toggle.onValueChanged.AddListener(OnToggleChanged); // ★핸들러 연결

    }

    public void Bind(SkillData data)
    {
        Data = data;
        Debug.Log($"[SkillBtn] Bind 완료: {data?.Skill_id}", this);
        Debug.Log($"[Bind] {gameObject.name} / id={data?.Skill_id}", this);

    }

    public void OnToggleChanged(bool isOn)
    {
        Debug.Log($"토글 체인지드 호출됨");

        // 여기서 infoPanel 같은 출력 오브젝트에 Data를 넘겨주도록 연결 가능
        // (예: SkillInfoPanel.Show(Data))
        if (!isOn || Data == null)
        {
            return;
        }


        //여기 코드는 기본적인 내가 생각할 수 있는 코드

        /*
        int[] damages = Data.Damages;

        // 전체가 0인지 확인
        bool allZero = damages.All(d => d == 0);

        if (allZero)
        {
            outputText.text =
                $"Skillname: {Data.P_Skill_Name}\n" +
                $"SkillDamage: 없음\n" +
                $"SkillCost: {Data.Stamina}";
        }
        else if (damages.Length == 1)
        {
            // 단타 스킬
            outputText.text =
                $"Skillname: {Data.P_Skill_Name}\n" +
                $"SkillDamage: {damages[0]}\n" +
                $"SkillCost: {Data.Stamina}";
        }
        else
        {
            // 멀티히트 스킬 (0 제외)
            var dmgList = new List<string>();
            for (int i = 0; i < damages.Length; i++)
            {
                if (damages[i] != 0)
                    dmgList.Add($"{i + 1}타: {damages[i]}");
            }

            string dmgText = string.Join(", ", dmgList);
            outputText.text =
                $"Skillname: {Data.P_Skill_Name}\n" +
                $"SkillDamage: {dmgText}\n" +
                $"SkillCost: {Data.Stamina}";
        }
        */


        // 위 코드의 간소화된 버전 정확한 작동원리는 아직 파악 못함
        int[] d = Data.Damages;
        string dmgText = d.All(x => x == 0)
            ? "없음"
            : (d.Length == 1 ? $"{d[0]}"
                             : string.Join(", ", d.Select((val, idx) => val == 0 ? null : $"{idx + 1}타: {val}").Where(s => s != null)));

        outputText.text = $"Skillname: {Data.P_Skill_Name}\nSkillDamage: {dmgText}\nSkillCost: {Data.Stamina}";


        Debug.Log($"선택된 스킬 출력에 성공함");
        Debug.Log($" ID: {Data.Skill_id} skillName :{Data.P_Skill_Name} Damage:{Data.Damages} " +
            $" Stamina:{Data.Stamina} Cooldewn:{Data.Cooldown}");


        if (isOn && Data != null)
        {
            OnSelected?.Invoke(this, Data); // ★ StoreUI에게 즉시 전달
        }


        // ★ 추가: 필요 소울 ID 가져오기
        int requiredSoulId = Data.NeedSoul;

        // ★ 스킬을 이미 가지고 있는지 체크
        bool alreadyOwned = GameManager.Instance.CurrentUserData.AcquiredSkillIds
            .Contains(Data.Skill_id);

        // ★ 소울을 가지고 있고, 스킬을 아직 보유하지 않은 경우에만 구매 버튼 활성화
        if (StoreManager.Instance.HaveSoul(requiredSoulId, 1) && !alreadyOwned)
        {
            buyBtn.interactable = true;   // 버튼 활성화
            Debug.Log($"[SkillBtn] 구매 가능 - {Data.P_Skill_Name}");
        }
        else
        {
            buyBtn.interactable = false;  // 버튼 비활성화
            Debug.Log($"[SkillBtn] 구매 불가 - 소울 부족 또는 이미 보유");
        }
    }

}
