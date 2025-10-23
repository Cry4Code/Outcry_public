using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [HideInInspector] public Animator animator;
    [HideInInspector] public SpriteRenderer spriteRenderer;
    
    #region 피격 피드백 관련 변수
    
    private MaterialPropertyBlock mpb;
    private string mpbColorKey = "_Color";
    private Color originalColor;
    private Coroutine damagedFlashCoroutine;
    
    #endregion
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        originalColor = spriteRenderer.color;
        mpb = new MaterialPropertyBlock();
    }

    /// <summary>
    /// 모든 bool 끄고 특정 bool만 켜기
    /// </summary>
    public void SetBoolAnimation(int animHash)
    {
        ClearBool();

        animator.SetBool(animHash, true);
    }

    public void OnBoolParam(int animHash)
    {
        animator.SetBool(animHash, true);
    }

    public void OffBoolParam(int animHash)
    {
        animator.SetBool(animHash, false);
    }

    /// <summary>
    /// 트리거 애니메이션 실행 (중복 방지를 위해 Reset 후 Set)
    /// </summary>
    public void SetTriggerAnimation(int animHash)
    {
        ClearTrigger();

        animator.SetTrigger(animHash);
    }

    /// <summary>
    /// Int 값 수정
    /// </summary>
    /// <param name="animHash"></param>
    /// <param name="value"></param>
    public void SetIntAniamtion(int animHash, int value)
    {
        animator.SetInteger(animHash, value);
    }
    public void ClearBool()
    {
        animator.SetBool(AnimatorHash.PlayerAnimation.Idle, false);
        animator.SetBool(AnimatorHash.PlayerAnimation.Move, false);
        animator.SetBool(AnimatorHash.PlayerAnimation.Fall, false);
    }

    public void ClearTrigger()
    {
        animator.ResetTrigger(AnimatorHash.PlayerAnimation.Jump);
        animator.ResetTrigger(AnimatorHash.PlayerAnimation.DoubleJump);
        animator.ResetTrigger(AnimatorHash.PlayerAnimation.NormalAttack);
        animator.ResetTrigger(AnimatorHash.PlayerAnimation.DownAttack);
        animator.ResetTrigger(AnimatorHash.PlayerAnimation.SpecialAttack);
        animator.ResetTrigger(AnimatorHash.PlayerAnimation.Dodge);
        animator.ResetTrigger(AnimatorHash.PlayerAnimation.StartParry);
        animator.ResetTrigger(AnimatorHash.PlayerAnimation.SuccessParry);
        animator.ResetTrigger(AnimatorHash.PlayerAnimation.Potion);
        animator.ResetTrigger(AnimatorHash.PlayerAnimation.AdditionalAttack);
    }

    public void ClearInt()
    {
        animator.SetInteger(AnimatorHash.PlayerAnimation.NormalAttackCount, 0);
    }

    public void DamagedFeedback(float flashTime, float flashSpeed)
    {
        if (damagedFlashCoroutine != null)
        {
            StopCoroutine(damagedFlashCoroutine);
            ApplyColor(originalColor);
        }

        damagedFlashCoroutine = StartCoroutine(FlashRoutine(flashTime, flashSpeed));
    }

    private IEnumerator FlashRoutine(float flashTime, float flashSpeed)
    {
        float elapsed = 0f;

        while (elapsed < flashTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.PingPong(elapsed * flashSpeed, 1f);

            Color lerped = Color.Lerp(originalColor, Color.red, t);
            ApplyColor(lerped);

            yield return null;
        }

        ApplyColor(originalColor);
    }

    private void ApplyColor(Color color)
    {
        spriteRenderer.GetPropertyBlock(mpb);
        mpb.SetColor(mpbColorKey, color);
        spriteRenderer.SetPropertyBlock(mpb);
    }
}
