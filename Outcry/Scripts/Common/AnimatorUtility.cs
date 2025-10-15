using UnityEngine;

public static class AnimatorUtility
{
    public static bool IsAnimationPlaying(Animator animator, int animationHash,
        float startTime = 0.0f, float endTime = 1.0f)
    {
        if (animator == null) return false;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.shortNameHash != animationHash)
            return false;

        if (stateInfo.loop)
        {
            float normalizedTime = stateInfo.normalizedTime % 1.0f;
            return normalizedTime >= startTime && normalizedTime < endTime;
        }
        else
        {
            return stateInfo.normalizedTime >= startTime && stateInfo.normalizedTime < endTime;
        }
    }
    
    public static bool TryGetAnimationLengthByNameHash(Animator animator, int animationHash, out float length)
    {
        if (animator == null)
        {
            Debug.LogError("Animator is null.");
            length = 0f;
            return false;
        }

        RuntimeAnimatorController ac = animator.runtimeAnimatorController;
        foreach (AnimationClip clip in ac.animationClips)
        {
            if (Animator.StringToHash(clip.name) == animationHash)
            {
                length = clip.length;
                return true;
            }
        }
        length = 0f;
        return false; // 애니메이션 클립을 찾지 못한 경우
    }
    
    /// <summary>
    /// 애니메이션 재생 직후 호출해서 애니메이션이 시작될 때까지 대기할때 사용합니다.
    /// 애니메이션 재생 지시 후 즉시 호출해야 합니다.
    /// 아닐 경우 올바르게 작동하지 않습니다.
    /// </summary>
    /// <param name="animator"></param>
    /// <param name="animationNameHase"></param>
    /// <param name="isAnimationStarted"></param>
    public static bool IsAnimationStarted(Animator animator, int animationNameHase)
    {
        if (AnimatorUtility.IsAnimationPlaying(animator, animationNameHase))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
