using UnityEngine;
using UnityEngine.UI;

public class HeartIcon : MonoBehaviour
{
    [Header("Animator Setup")]
    [SerializeField] private string breakTriggerName = "Break"; // 컨트롤러의 트리거 이름
    [SerializeField] private RuntimeAnimatorController controller; // 인스펙터에 컨트롤러 에셋 드래그

    private Animator animator;

    private void Awake()
    {
        // 같은 오브젝트에 Animator 보장 + 컨트롤러 자동 연결
        animator = GetComponent<Animator>();
        if (animator == null) animator = gameObject.AddComponent<Animator>();
        if (controller != null) animator.runtimeAnimatorController = controller;

        // UI는 항상 업데이트되도록 (타임스케일/오프스크린 영향 방지)
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
    }

    /// <summary>보이기/숨기기</summary>
    public void Show(bool on)
    {
        gameObject.SetActive(on);
    }

    /// <summary>깨짐 애니메이션 트리거</summary>
    public void Break()
    {
        if (animator == null)
        {
            Debug.LogWarning("[HealthCaseAnim] Animator가 없어 Break를 재생할 수 없습니다.");
            return;
        }

        // 트리거 중복 안전 + 발사
        animator.ResetTrigger(breakTriggerName);
        Debug.Log("[HealthCaseAnim] Break 트리거 실행");
        animator.SetTrigger(breakTriggerName);
    }

#if UNITY_EDITOR
    [ContextMenu("TEST/Break")]
    private void _TestBreak()
    {
        Break();
    }
#endif
}