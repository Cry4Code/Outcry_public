using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialUIAOC : UIBase
{

    [SerializeField] private Animator animator;                     // 없으면 자동으로 GetComponent
    [SerializeField] private RuntimeAnimatorController baseController; // 베이스 컨트롤러(.controller 또는 AOC도 가능)

    [SerializeField] private List<ClipMapping> overrides = new();

    [SerializeField] private bool playOnAwake = true;

    [SerializeField] private string stateToPlay = ""; // 예: "Loop" / "Attack_0"

    private AnimatorOverrideController instanceAOC;

    [Serializable]
    public struct ClipMapping
    {
        public AnimationClip placeholder; // 베이스 컨트롤러가 참조하는 '키' 클립
        public AnimationClip replacement; // 이 오브젝트가 실제로 재생할 클립
    }

    void Awake()
    {
        // 오브젝트 전용 AOC 인스턴스 생성 (공유 에셋을 직접 바꾸지 않음!)
        instanceAOC = new AnimatorOverrideController(baseController);

        // 베이스의 모든 키 목록을 받아서 필요한 것만 교체
        var list = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        instanceAOC.GetOverrides(list);

        // placeholder → replacement 적용
        for (int i = 0; i < list.Count; i++)
        {
            var key = list[i].Key;
            var rep = FindReplacementFor(key);
            if (rep != null)
            {
                list[i] = new KeyValuePair<AnimationClip, AnimationClip>(key, rep);
            }
        }
        instanceAOC.ApplyOverrides(list);

        // Animator에 장착
        animator.runtimeAnimatorController = instanceAOC;

        if (playOnAwake)
        {
            if (!string.IsNullOrEmpty(stateToPlay))
                animator.Play(stateToPlay, 0, 0f);
            else
                animator.Play(0, 0, 0f); // 레이어0의 Default State
            animator.Update(0f); // 즉시 반영(첫 프레임 깜빡임 방지)
        }
    }

    AnimationClip FindReplacementFor(AnimationClip placeholder)
    {
        for (int i = 0; i < overrides.Count; i++)
        {
            if (overrides[i].placeholder == placeholder)
                return overrides[i].replacement;
        }
        return null;
    }

    /// <summary>런타임에 특정 placeholder만 교체하고 즉시 재생(선택)</summary>
    public void SetOverride(AnimationClip placeholder, AnimationClip replacement, bool restart = false, string stateName = "")
    {
        if (instanceAOC == null) return;
        if (placeholder == null) return;

        instanceAOC[placeholder] = replacement;

        if (restart)
        {
            if (!string.IsNullOrEmpty(stateName))
                animator.Play(stateName, 0, 0f);
            else
                animator.Play(0, 0, 0f);
            animator.Update(0f);
        }
    }
}
