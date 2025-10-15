using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ShowSkillPreview : MonoBehaviour
{/*-----------------------------------------------미리보기 삭제
  * 
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

    private AnimatorOverrideController aoc;

    private void Start()
    {
        aoc = new AnimatorOverrideController(baseController);
        animator.runtimeAnimatorController = aoc;
    }

    public void Play(int skillId)
    {
        Debug.Log("1"); // 함수 진입 확인

        var found = table.FirstOrDefault(e => e.id == skillId);

        if (found.id == 0 || found.clip == null)
        {
            Debug.LogWarning($"[Preview] 매핑 없음: ID={skillId}");
            Debug.Log("2"); // 매핑 실패

            return;
        }
        Debug.Log("3"); // 클립 매핑 성공

        // "Skill" 상태의 모션을 해당 클립으로 교체
        aoc[dummySkillClip] = found.clip;     // (O) 클립 참조 키가 안전
        Debug.Log("4"); // 오버라이드 성공


        Debug.Log("5"); // Loop 설정
        animator.ResetTrigger("Play");
        animator.SetTrigger("Play");
        animator.SetBool("Loop", true); // ★ 루프 켜기

        Debug.Log("6"); // 트리거 발동


        var sr = GetComponent<SpriteRenderer>();
        Debug.Log("SR0:" + (sr != null));
        if (sr)
        {
            sr.enabled = true; sr.color = Color.white;
            sr.sortingLayerName = "Default"; sr.sortingOrder = 5000;
            Debug.Log("SR1:" + sr.enabled + "," + sr.color.a + "," + sr.sortingOrder);
            Debug.Log("SR2:sprNull=" + (sr.sprite == null));
        }

    }
    public void Stop()
    {
        animator.SetBool("Loop", false); // ★ 루프 끄기 → Idle로 복귀
    }*/
}
