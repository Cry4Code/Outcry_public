using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using UnityEngine.UI;

public class ShowSkillPreview : MonoBehaviour
{
    [System.Serializable] 
    public struct Entry
    {
        public int id;                 // 스킬 ID
        public AnimationClip clip;     // 재생할 클립
    }

    [SerializeField] private Animator animator;
    [SerializeField] private RuntimeAnimatorController baseController; // SkillBaseController
    [SerializeField] private Entry[] table;
    [SerializeField] private AnimationClip dummySkillClip; // ← Skill 상태에 들어간 EmptyLooper를 드래그
    [SerializeField] private Image image;

    private AnimatorOverrideController aoc;

    private void Start()
    {
        aoc = new AnimatorOverrideController(baseController);
        image =  GetComponent<Image>();
        animator.runtimeAnimatorController = aoc;
        Debug.Log($"[Preview] animator name = {animator.name}");
        image.type = Image.Type.Simple;
        image.preserveAspect = true;
    }

    public void Play(int skillId)
    {
        Debug.Log($"[Preview] 재생 시도: skillId = {skillId}");

        var found = table.FirstOrDefault(e => e.id == skillId);

        if (found.id == 0 || found.clip == null)
        {
            Debug.LogWarning($"[Preview] 매핑 없음: ID={skillId}");

            return;
        }

        if (dummySkillClip == null)
        {
            Debug.LogError("[Preview] dummySkillClip이 지정되지 않음!");
            return;
        }
        
        aoc[dummySkillClip] = found.clip;     

        
        animator.ResetTrigger("Play");
        animator.SetTrigger("Play");
        animator.SetBool("Loop", true); // ★ 루프 켜기

        if (image != null)
        {
            image.enabled = true;
            image.color = Color.white; // 혹시 투명해져 있었을 경우
        }
    }
    public void Stop()
    {
        image.color = Color.clear;
        animator.SetBool("Loop", false); // ★ 루프 끄기 → Idle로 복귀
        Debug.Log("[Preview] 스킬 프리뷰 정지");
    }
}
