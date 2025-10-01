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
        SetSkill(skillCode);
    }

    public void SetSkill(int skillId)
    {
        // TODO : 스킬 아이디로 플레이어 스킬 매니저에서 찾아와서 여기다가 넣어주면 됨
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
