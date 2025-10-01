using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioPlayer : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// 현재 재생 중인 오디오 클립의 전체 길이를 초 단위로 반환
    /// </summary>
    /// <returns>클립의 길이. 클립이 없으면 0 반환.</returns>
    public float GetClipLength()
    {
        if (audioSource.clip != null)
        {
            return audioSource.clip.length;
        }

        return 0f;
    }

    public void Play(AudioClip clip, float volume, float pitch)
    {
        audioSource.clip = clip;
        audioSource.volume = volume;
        // 매번 다른 소리처럼 들리게 피치를 살짝 변형
        audioSource.pitch = pitch * Random.Range(0.95f, 1.05f);

        audioSource.Play();
    }

    public void Stop()
    {
        audioSource.Stop();
    }
}
