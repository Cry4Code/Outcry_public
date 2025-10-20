using StageEnums;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ChapterUI : UIPopup
{
    [Header("Animation Settings")]
    [SerializeField] private Animator mapAnimator;
    [SerializeField] private GameObject bossSelectionGroup;
    private const string UNROLL_ANIM_NAME = "AnimatedMap";

    [SerializeField] private Button RuinsOfTheFallenKingBtn;
    [SerializeField] private Button AbandonedMineBtn;
    [SerializeField] private Button HallOfBloodBtn;
    [SerializeField] private Button exitBtn;

    private Coroutine showButtonsCoroutine;

    private void Awake()
    {
        RuinsOfTheFallenKingBtn.onClick.AddListener(OnRuinsOfTheFallenKingClicked);
        AbandonedMineBtn.onClick.AddListener(OnAbandonedMineClicked);
        HallOfBloodBtn.onClick.AddListener(OnHallOfBloodClicked);
        exitBtn.onClick.AddListener(OnExitButtonClicked);

        // TEMP
        AbandonedMineBtn.interactable = false;
        HallOfBloodBtn.interactable = false;
    }

    private void OnEnable()
    {
        // 이전에 실행 중이던 코루틴이 있다면 중지시켜 중복 실행 방지
        if (showButtonsCoroutine != null)
        {
            StopCoroutine(showButtonsCoroutine);
        }

        // 새로운 코루틴 시작하고 변수에 저장
        showButtonsCoroutine = StartCoroutine(ShowButtonsAfterAnimation());
    }

    private void OnDisable()
    {
        if (showButtonsCoroutine != null)
        {
            StopCoroutine(showButtonsCoroutine);
            showButtonsCoroutine = null;
        }
    }

    /// <summary>
    /// 애니메이션을 재생하고 끝난 뒤에 버튼 그룹을 활성화하는 코루틴
    /// </summary>
    private IEnumerator ShowButtonsAfterAnimation()
    {
        // 버튼 숨김
        if (bossSelectionGroup != null)
        {
            bossSelectionGroup.SetActive(false);
            exitBtn.gameObject.SetActive(false);
        }

        // 애니메이션 처음부터 재생
        if (mapAnimator != null)
        {
            if (mapAnimator.runtimeAnimatorController == null)
            {
                Debug.LogError("mapAnimator에 Animator Controller가 할당되지 않았습니다!");
                yield break; // 코루틴을 즉시 중단
            }

            mapAnimator.Play(UNROLL_ANIM_NAME);

            // 애니메이터가 상태를 전환할 때까지 한 프레임 기다림
            yield return null;

            // 현재 재생 중인 애니메이션 클립의 길이를 가져옴
            float animationLength = mapAnimator.GetCurrentAnimatorStateInfo(0).length;

            // 애니메이션 길이만큼 기다림
            yield return new WaitForSeconds(animationLength);
        }

        // 기다림이 끝나면 버튼 활성화
        if (bossSelectionGroup != null)
        {
            bossSelectionGroup.SetActive(true);
            exitBtn.gameObject.SetActive(true);
        }

        // 코루틴이 완료되었으므로 null로 초기화
        showButtonsCoroutine = null;
    }

    private void OnRuinsOfTheFallenKingClicked()
    {
        GameManager.Instance.StartStage((int)EStageType.RuinsOfTheFallenKing);
    }

    private void OnAbandonedMineClicked()
    {
        GameManager.Instance.StartStage((int)EStageType.AbandonedMine);
    }

    private void OnHallOfBloodClicked()
    {
        GameManager.Instance.StartStage((int)EStageType.HallOfBlood);
    }

    private void OnExitButtonClicked()
    {
        UIManager.Instance.Hide<ChapterUI>();
        CursorManager.Instance.SetInGame(true);
        PlayerManager.Instance.player.PlayerInputEnable();
    }
}
