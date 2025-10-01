using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ShowSkillInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI staminaText;

    public void Show(SkillData data)
    {
        if (data == null) return;

        nameText.text = data.P_Skill_Name;
        damageText.text = data.Damages.ToString();
        staminaText.text = data.Stamina.ToString();
    }

    public void Clear()
    {
        nameText.text = "";
        damageText.text = "";
        staminaText.text = "";
    }
}
