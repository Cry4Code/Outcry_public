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
    [SerializeField] private Button buyBtn;
    [SerializeField] private TextMeshProUGUI selfInfoText;   // ★추가
    [SerializeField] private Image iconImage;
    [SerializeField] private Image soulsprite;
    
    [SerializeField] private ShowSkillPreview previewPlayer;
    public void SetPreviewPlayer(ShowSkillPreview p) { previewPlayer = p; }

    public event Action<SkillBtn, SkillData> OnSelected;  // (sender, data)
    public SkillData Data { get; private set; }

    public void SetBuyButton(Button button)
    {
        buyBtn = button;
    }
    [Serializable] public struct SkillIconPair { public int id; public Sprite icon; } 
    [SerializeField] private SkillIconPair[] iconPairs;

    public void SetToggleGroup(ToggleGroup group)
    {
        if (toggle == null) toggle = GetComponentInChildren<Toggle>(true);
        if (toggle == null)
        {
            Debug.LogError($"[SkillBtn] Toggle을 찾지 못했습니다. ({name})", this);
            return;
        }
        toggle.group = group;
    }


    private void Awake()
    {

        toggle.onValueChanged.AddListener(_ => Debug.Log("[RAW] toggle changed"));

        var t = GetComponent<Toggle>(); // 프리팹에서 씬을 참조할 수 없어서 직접 부모 오브젝트의 토글 그룹을 추가함
        if (t != null && t.group == null)
            t.group = GetComponentInParent<ToggleGroup>(); // 가장 가까운 부모의 그룹 자동 연결

        toggle.onValueChanged.AddListener(OnToggleChanged); // ★핸들러 연결

    }

    private void Start()
    {
#if UNITY_EDITOR
        if (toggle == null)
        {
            Debug.LogError($"[SkillBtn] Toggle 자체가 연결되지 않았습니다. ({gameObject.name})", this);
        }
        else if (toggle.group == null)
        {
            Debug.LogError($"[SkillBtn] ToggleGroup 연결 실패! ToggleGroup 부모를 찾지 못했거나 할당되지 않았습니다. ({gameObject.name})", this);
        }
        else
        {
            Debug.Log($"[SkillBtn] ToggleGroup 연결 확인 완료 ({gameObject.name})", this);
        }
#endif

    }

    private void OnEnable()
    {
        RefreshSkillInformation();
    }

    public void Bind(SkillData data)
    {
        Data = data;
        Debug.Log($"[SkillBtn] Bind 완료: {data?.Skill_id}", this);
        Debug.Log($"[Bind] {gameObject.name} / id={data?.Skill_id}", this);

        // 보스 소울 스프라이트 바인딩
        Sprite soul = GameManager.Instance.GetSprite(data.NeedSoul);
        if (soul != null)
        {
            soulsprite.sprite = soul;
        }

        // ★추가: 아이콘 바인딩 (Skill_id 기준)
        if (iconImage != null && iconPairs != null)
        {
            Sprite picked = null;
            for (int i = 0; i < iconPairs.Length; i++)
            {
                if (iconPairs[i].id == data.Skill_id)
                {
                    picked = iconPairs[i].icon;
                    break;
                }
            }
            if (picked != null)
            {
                iconImage.sprite = picked;
                iconImage.enabled = true;
            }
        }

        RefreshSkillInformation();

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

    }

    private void RefreshSkillInformation()
    {
        // ★추가: 텍스트 즉시 출력(동적 생성 시 바로 보이게)
        if (selfInfoText != null && Data != null)
        {
            int[] d = Data.Damages ?? Array.Empty<int>();
            // 0은 합산에서 제외
            int total = 0;
            for (int i = 0; i < d.Length; i++)
            {
                if (d[i] > 0) total += d[i];
            }

            string dmgText = (d.Length == 0 || total == 0) ? "NONE" : total.ToString();
            float cd = Data.Cooldown;
            string cond = Data.Condition;
            // selfInfoText.text = $"Skillname: {Data.P_Skill_Name}\nSkillDamage: {dmgText}\nSkillCost: {Data.Stamina}\n"+$"Cooldown: {cd}\n" +$"Condition: {cond}";
            var localizedNameText = LocalizationUtility.GetLocalizedValueByKey(LocalizationStrings.Skill.NAME);
            var localizedDamageText = LocalizationUtility.GetLocalizedValueByKey(LocalizationStrings.Skill.DAMAGE);
            var localizedCostText = LocalizationUtility.GetLocalizedValueByKey(LocalizationStrings.Skill.COST);
            var localizedCooldownText = LocalizationUtility.GetLocalizedValueByKey(LocalizationStrings.Skill.COOLDOWN);
            var localizedSkillName = LocalizationUtility.IsCurrentLanguage("en") ? Data.P_Skill_Name : Data.P_Skill_Name_Ko;
            var localizedCondition = LocalizationUtility.IsCurrentLanguage("en") ? Data.Condition : Data.Condition_Ko;
            // Debug.Log("[한글화]" + localizedSkillName);
            selfInfoText.text = $"{localizedNameText}: {localizedSkillName}\n{localizedDamageText}: {dmgText}\n{localizedCostText}: {Data.Stamina}\n{localizedCooldownText}:{cd}\n{localizedCondition}";
        }
    }
    public void OnToggleChanged(bool isOn)
    {
        Debug.Log($"토글 체인지드 호출됨");

        // 여기서 infoPanel 같은 출력 오브젝트에 Data를 넘겨주도록 연결 가능
        // (예: SkillInfoPanel.Show(Data))
        if (!isOn || Data == null)
        {
            previewPlayer.Stop(); 
            return;
        }

        if (isOn && Data != null)
        {
            OnSelected?.Invoke(this, Data); //StoreUI에게 즉시 전달
            previewPlayer.Play(Data.Skill_id);
        }




        //추가: 필요 소울 ID 가져오기
        int requiredSoulId = Data.NeedSoul;

        //스킬을 이미 가지고 있는지 체크
        bool alreadyOwned = GameManager.Instance.CurrentUserData.AcquiredSkillIds
            .Contains(Data.Skill_id);

        //소울을 가지고 있고, 스킬을 아직 보유하지 않은 경우에만 구매 버튼 활성화
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
