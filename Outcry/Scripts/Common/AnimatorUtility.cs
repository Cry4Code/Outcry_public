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
}
