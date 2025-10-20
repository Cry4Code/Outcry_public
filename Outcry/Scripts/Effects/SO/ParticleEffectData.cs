using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "ParticleEffectData", menuName = "ScriptableObjects/Effects/ParticleEffectData", order = 5)]
public class ParticleEffectData : BaseEffectData
{
    [field: SerializeField] public float Duration { get; private set; }
    [field: SerializeField] public string path { get; private set; }

    [field: Header("Spawn"), SerializeField] public bool AttachToTarget { get; private set; }
    [field: SerializeField] public Vector3 DefaultLocalOffset { get; private set; } = Vector3.zero;

#if UNITY_EDITOR
    [Header("Editor Only")] 
    public GameObject particlePrefab;
    private void OnValidate()
    {
        this.effectType = EffectType.Particle;
        if (particlePrefab != null)
        {
            path = AssetDatabase.GetAssetPath(particlePrefab).Replace(AddressablePaths.ROOT, "");
            particlePrefab = null;
        }
    }
#endif
    public override async UniTask EffectAsync(EffectOrder order, CancellationToken token, GameObject target = null, Vector3 position = default(Vector3))
    {
        // 기본 부모(타겟) 설정, 없으면 EffectManager를 부모로 사용
        Debug.Log($"[이펙트: UniTask (ID: {effectId} Type: {effectType})] EffectAsync operation try entered.");
        if (target == null)        
            target = EffectManager.Instance.gameObject;

        GameObject effectInstance = null;        

        try
        {
            // 자식으로 생성할지 월드에서 생성할지 결정
            Transform parent = AttachToTarget ? target.transform : null;
            Vector3 localOffset = DefaultLocalOffset + position;

            // 좌우 반전 보정 - 스프라이트 이펙트와 동일
            Vector3 scale = target.transform.localScale;
            /*localOffset.x *= Mathf.Sign(scale.x);
            localOffset.y *= Mathf.Sign(scale.y);
            localOffset.z *= Mathf.Sign(scale.z);*/

            // 풀에서 파티클 프리팹 확득 (부모 위치 저장)
            effectInstance = await ObjectPoolManager.Instance.GetObjectAsync(path, parent, localOffset);

            // AttachToTarget == false면, world 좌표에 배치
            if (!AttachToTarget)
            {
                if (target != null)
                    effectInstance.transform.position = target.transform.TransformPoint(localOffset);
            }

            // 파티클 재생 준비/재생
            var ps = effectInstance.GetComponent<ParticleSystem>();
            
            if (ps != null)
            {
                ps.Clear(true);
                ps.Simulate(0f, true, true);
                ps.Play(true);
            }

            // 대기 시간 결정
            //  - SO에서 Duration 지정하면 그 값을 사용
            //  - 지정 안 했다면 파티클 메인 duration을 참고
            float waitSeconds = Duration;
            if (waitSeconds <= 0f && ps != null)
            {
                var main = ps.main;
                // 루프면 main.duration만큼 대기, 아니면 startLifeTime을 더해줌
                float startLifetimeMax = 0f;
                var sl = main.startLifetime;
                switch (sl.mode)
                {
                    case ParticleSystemCurveMode.Constant:
                        startLifetimeMax = sl.constant;
                        break;
                    case ParticleSystemCurveMode.TwoConstants:
                        startLifetimeMax = sl.constantMax;
                        break;
                    default:
                        startLifetimeMax = 0f;
                        break;
                }
                waitSeconds = main.loop ? Mathf.Max(0.1f, main.duration) : Mathf.Max(0.1f, main.duration + startLifetimeMax);
            }
            if (waitSeconds <= 0f)
            {
                // 파티클 컴포넌트가 없으면 최소 대기
                waitSeconds = 0.25f;
            }

            // 취소 가능 대기
            await UniTask.Delay((int)(waitSeconds * 1000), cancellationToken: token);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning($"[ParticleEffectData] (ID: {effectId}) EffectAsync operation cancelled");
        }
        catch (Exception e)
        {
            Debug.LogError($"[ParticleEffectData] (ID: {effectId}) EffectAsync failed: {e}");
        }
        finally
        {
            // 정지 후 풀 반환
            if (effectInstance != null)
            {
                var ps = effectInstance.GetComponent<ParticleSystem>();
                    //?? effectInstance.GetComponentInChildren<ParticleSystem>();
                if (ps != null)
                {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }

                ObjectPoolManager.Instance.ReleaseObject(path, effectInstance);
            }
        }
    }
}
