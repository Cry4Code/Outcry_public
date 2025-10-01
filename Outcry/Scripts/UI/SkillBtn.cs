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

    // 프리팹의 Toggle
    public SkillData Data { get; private set; }

    public void SetOutputText(TextMeshProUGUI t) { outputText = t; }

    private void Awake()
    {
        toggle.onValueChanged.AddListener(_ => Debug.Log("[RAW] toggle changed"));

        var t = GetComponent<Toggle>(); // 프리팹에서 씬을 참조할 수 없어서 직접 부모 오브젝트의 토글 그룹을 추가함
        if (t != null && t.group == null)
            t.group = GetComponentInParent<ToggleGroup>(); // 가장 가까운 부모의 그룹 자동 연결
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

        outputText.text = $"Skillname: {Data.P_Skill_Name}\n skilldamage : {Data.Damages}\n skillcost: {Data.Stamina}";

        Debug.Log($"선택된 스킬 출력에 성공함");

        Debug.Log($" ID: {Data.Skill_id} skillName :{Data.P_Skill_Name} Damage:{Data.Damages} " +
            $" Stamina:{Data.Stamina} Cooldewn:{Data.Cooldown}");
    }

}
