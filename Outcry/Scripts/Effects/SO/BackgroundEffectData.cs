using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "BackgroundEffectData", menuName = "ScriptableObjects/Effects/BackgroundEffectData", order = 4)]
public class BackgroundEffectData : BaseEffectData
{
    [field: SerializeField] public float Duration { get; private set; }
    [field: SerializeField] public Color FirstPointColor { get; private set; } = new Color(0, 0, 0, 1);
    [field: SerializeField] public Color SecondPointColor { get; private set; } = new Color(0, 0, 0, 1);

    private static readonly int ColorID = Shader.PropertyToID("_Color");

#if UNITY_EDITOR
    private void OnValidate()
    {
        this.effectType = EffectType.Background;
    }
#endif       

    public override async UniTask EffectAsync(EffectOrder order, CancellationToken token, GameObject target = null, Vector3 position = default(Vector3))
    {
        var affected = new List<Renderer>();

        try
        {
            //빨강. 컬러를 이 SO의 PointColor로 바꾸기
            //검정. 컬러를 검정으로 바꾸기
            ChangeMapColor(StageManager.Instance.ColorBgs, FirstPointColor);
            ChangeMapColor(StageManager.Instance.BlackBgs, SecondPointColor);

            ChangeCharacterColor(PlayerManager.Instance.player.gameObject, SecondPointColor, affected);

            var monsters = StageManager.Instance.currentStageController.aliveMonsters;
            for (int i = 0; i < monsters.Count; i++)
            {
                ChangeCharacterColor(monsters[i], SecondPointColor, affected);
            }

            //Duration초 동안 지속되도록 하기
            //끝나면 원래대로 돌려놓기
            await UniTask.Delay((int)(Duration * 1000), cancellationToken: token);
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning($"[이펙트: UniTask (ID : {effectId})] EffectAsync operation cancelled");
        }
        catch (Exception e)
        {
            Debug.LogError($"[이펙트: UniTask (ID : {effectId})] EffectAsync operation failed: {e}");
        }
        finally
        {
            // 맵은 덧칠 대신 MPB 제거
            ResetMapColor(StageManager.Instance.ColorBgs);
            ResetMapColor(StageManager.Instance.BlackBgs);

            ResetCharacterColor(affected);

            /* 기존 코드
            ChangeCharacterColor(PlayerManager.Instance.player.gameObject, Color.white);
            for (int i = 0; i < StageManager.Instance.currentStageController.aliveMonsters.Count; i++)
            {
                ChangeCharacterColor(StageManager.Instance.currentStageController.aliveMonsters[i], Color.white);
            }
            */
        }
    }

    private void ChangeMapColor<T>(List<T> renderers, Color color) where T : Renderer
    {
        // 시작 시 1차 정리
        // 리스트 내부 요소가 null이거나 파괴된 경우 제거
        renderers.RemoveAll(r => !r || r.Equals(null) || r.gameObject == null);

        // 리스트 마지막부터 정리 (Remove에서의 꼬임 방지)
        for (int i = renderers.Count - 1; i >= 0; --i)
        {
            var r = renderers[i];
            // null 방지 한번 더
            if (!r || r.Equals(null) || r.gameObject == null)
            {
                renderers.RemoveAt(i);
                continue;
            }

            if (!IsTintableRenderer(r))
                continue;

            try
            {
                MaterialPropertyBlock mpb = new MaterialPropertyBlock();

                mpb.Clear();
                r.GetPropertyBlock(mpb);
                mpb.SetColor(ColorID, color);
                r.SetPropertyBlock(mpb);
            }
            catch (MissingReferenceException)
            {
                renderers.RemoveAt(i);  // 죽은 참조는 즉시 제거
            }
        }
    }

    private void ResetMapColor<T>(List<T> renderers) where T : Renderer
    {
        for (int i = renderers.Count - 1; i >= 0; --i)
        {
            var r = renderers[i];

            if (!r || r.Equals(null) || r.gameObject == null)
            {
                renderers.RemoveAt(i);
                continue;
            }

            if (!IsTintableRenderer(r))
                continue;

            try
            {
                r.SetPropertyBlock(null);
            }
            catch (MissingReferenceException)
            {
                renderers.RemoveAt(i);
            }
        }
    }

    private void ChangeCharacterColor(GameObject parent, Color color, List<Renderer> record = null)
    {
        if (!parent || parent.Equals(null))
            return;

        var renderers = parent.GetComponentsInChildren<Renderer>(includeInactive: true);
        if (renderers == null || renderers.Length == 0)
            return;

        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if (!r || r.Equals(null) || r.gameObject == null)
                continue;

            if (!IsTintableRenderer(r))
                continue;

            try
            {
                MaterialPropertyBlock mpb = new MaterialPropertyBlock();

                mpb.Clear();
                r.GetPropertyBlock(mpb);
                mpb.SetColor(ColorID, color);
                r.SetPropertyBlock(mpb);

                record?.Add(r);
            }
            catch (MissingReferenceException) { /* 파괴된 경우 건너뜀 */ }
        }
    }

    private void ResetCharacterColor(List<Renderer> renderers)
    {
        if (renderers == null)
            return;

        for (int i = renderers.Count - 1; i >= 0; --i)
        {
            var r = renderers[i];
            if (!r || r.Equals(null) || r.gameObject == null)
            {
                renderers.RemoveAt(i);
                continue;
            }

            if (!IsTintableRenderer(r))
                continue;

            try
            {
                r.SetPropertyBlock(null);
            }
            catch (MissingReferenceException) { }
        }
    }

    private static bool IsTintableRenderer(Renderer r)
    {
        return r is SpriteRenderer || r is TilemapRenderer;
    }
}
