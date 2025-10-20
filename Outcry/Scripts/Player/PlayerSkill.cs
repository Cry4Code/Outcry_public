using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class PlayerSkill : MonoBehaviour
{
    public SkillBase CurrentSkill;
    public int skillCode; // 테스트 코드
    private PlayerController controller;

    // 로비에서 스테이지로 넘어올 때 처음에 SetSkill로 불러줘야 됨

    
    public void Init(PlayerController controller)
    {
        this.controller = controller;
    }

    private void Start()
    {
        if (skillCode != 0)
        {
            SetSkill(skillCode);
        }
    }

    public void SetSkill(int skillId)
    {
        // SkillId가 0이면 아무런 스킬도 장착하지 않음.
        if (skillId == 0)
        {
            CurrentSkill = null;
            return;
        }
        
        if (DataManager.Instance.AllSkills.TryGetValue(skillId, out var skillBase))
        {
            CurrentSkill = skillBase;
            if (controller == null)
            {
                Debug.LogError("[플레이어] controller is null");
                return;
            }
            controller.Animator.SetIntAniamtion(AnimatorHash.PlayerAnimation.AdditionalAttackID, skillId);
            Debug.Log($"[플레이어] 스킬 {skillId} 이 세팅됨");
        }
        else
        {
            Debug.LogError($"[플레이어] 스킬 {skillId} 는 없는 번호임");
        }
    }

    public int GetSKillID()
    {
        return CurrentSkill.skillId;
    }
}
