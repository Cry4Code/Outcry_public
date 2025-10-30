using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SkillSelectBtn : MonoBehaviour

{

    [SerializeField] public Toggle toggle;
    [SerializeField] private TextMeshProUGUI SkillName;

    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject overlay;


    private Image linkedExternalImage;
    public event Action<SkillSelectBtn, SkillData> OnSelected;

    // 프리팹의 Toggle
    public SkillData Data { get; private set; }

    public void SetLinkedExternalImage(Image target)
    {
        linkedExternalImage = target;
    }


    private void Awake()
    {
        toggle.onValueChanged.AddListener(_ => Debug.Log("[RAW] toggle changed"));

        var t = GetComponent<Toggle>(); // 프리팹에서 씬을 참조할 수 없어서 직접 부모 오브젝트의 토글 그룹을 추가함
        if (t != null && t.group == null)
            t.group = GetComponentInParent<ToggleGroup>(); // 가장 가까운 부모의 그룹 자동 연결


    }

    private void OnEnable()
    {
        RefreshSkillName();
    }

    private void RefreshSkillName()
    {
        if (SkillName != null && Data != null)
        {
            if (LocalizationUtility.IsCurrentLanguage("en"))
            {
                string result = Regex.Replace(Data.P_Skill_Name, "([A-Z])", "\n$1");
                result = result.TrimStart('\n');
                SkillName.text = result;
            }
            else
            {
                string result = Regex.Replace(Data.P_Skill_Name_Ko, " ", "\n");
                SkillName.text = result;
            }
        }
    }

    public void Bind(SkillData data)
    {
        Data = data;
        Debug.Log($"[SkillBtn] Bind 완료: {data?.Skill_id}", this);
        Debug.Log($"[Bind] {gameObject.name} / id={data?.Skill_id}", this);
        RefreshSkillName();

    }

    public void SetOverlayActive(bool active)
    {
        if (overlay)
            overlay.SetActive(active);
    }

    public void SetIcon(Sprite s)
    {
        if (iconImage && s) iconImage.sprite = s;
    }

    public void OnToggleChanged(bool isOn)
    {
        Debug.Log($"토글 체인지드 호출됨");

        if (!isOn || Data == null)
        {
            return;
        }

        EffectManager.Instance.PlayEffectByIdAndTypeAsync(UIEffectID.EquipSkill, EffectType.Sound).Forget();
        
        // ★ 추가: 선택 이벤트 발행 (내 자신과 데이터 전달)
        OnSelected?.Invoke(this, Data);

        //int[] d = Data.Damages;
        //string dmgText = d.All(x => x == 0)
        //    ? "없음"
        //    : (d.Length == 1 ? $"{d[0]}"
        //                     : string.Join(", ", d.Select((val, idx) => val == 0 ? null : $"{idx + 1}타: {val}").Where(s => s != null)));

        //outputText.text = $"Skillname: {Data.P_Skill_Name}\nSkillDamage: {dmgText}\nSkillCost: {Data.Stamina}";

        if (linkedExternalImage && iconImage && iconImage.sprite)
            linkedExternalImage.sprite = iconImage.sprite;

    }


}
