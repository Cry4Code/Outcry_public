using System;
using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraManager : Singleton<CameraManager>
{
    public CinemachineVirtualCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin perlin;
    
    private EffectOrder currentShakeOrder;
    private Coroutine shakeCoroutine;

    public Camera MainCamera;

    protected override void Awake()
    {
        base.Awake();

        TryInit();
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;    //todo. SceneLoadManager에 역할 이관
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void OnActiveSceneChanged(Scene oldS, Scene newS)
    {
        virtualCamera = null;
        perlin = null;
        TryInit();
    }

    private bool TryInit()
    {
        if (virtualCamera == null)
        {
            virtualCamera = FindFirstObjectByType<CinemachineVirtualCamera>();
            if (virtualCamera == null)
                Debug.LogError("CinemachineVirtualCamera not found in the scene.");
        }

        if (virtualCamera != null && perlin == null)
        {
            perlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (perlin == null)
                Debug.LogError("CinemachineBasicMultiChannelPerlin component not found on the virtual camera.");
        }

        return perlin != null;
    }

    public CinemachineVirtualCamera GetCurrentVirtualCamera()
    {
        return virtualCamera;
    }

    public void ShakeCamera(float duration, float magnitude, float frequency, EffectOrder shakeOrder)
    {
        if (!TryInit()) return;
        if (shakeCoroutine == null)
        {
            currentShakeOrder = shakeOrder;
            shakeCoroutine = StartCoroutine(ShakeCameraCoroutine(duration, magnitude, frequency));
        }
        else
        {
            if (currentShakeOrder <= shakeOrder)
            {
                StopCoroutine(shakeCoroutine);
                StopCameraShake();
                
                currentShakeOrder = shakeOrder;
                
                Debug.Log($"[카메라] {currentShakeOrder}의 요청에 의해 카메라 흔들림");
                shakeCoroutine = StartCoroutine(ShakeCameraCoroutine(duration, magnitude, frequency));
            }
        }
    }

    private IEnumerator ShakeCameraCoroutine(float duration, float magnitude, float frequency)
    {
        perlin.m_AmplitudeGain = magnitude;
        perlin.m_FrequencyGain = frequency;
        yield return new WaitForSeconds(duration);
        StopCameraShake();
        shakeCoroutine = null;
    }

    public void StopCameraShake()
    {
        if (perlin == null) return;
        perlin.m_AmplitudeGain = 0f;
        perlin.m_FrequencyGain = 0f;
    }
    
    public void StopAllCameraCoroutine()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }

        StopCameraShake(); // 현재 진행 중인 셰이크 즉시 정지
    }
}
