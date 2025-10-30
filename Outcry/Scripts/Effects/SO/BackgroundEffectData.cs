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
        var affected = new List<SpriteRenderer>();

        try
        {
           //빨강. 컬러를 이 SO의 PointColor로 바꾸기
            //검정. 컬러를 검정으로 바꾸기
            ChangeMapColor(StageManager.Instance.ColorBgs, FirstPointColor);
            ChangeMapColor(StageManager.Instance.BlackBgs, SecondPointColor);

           
            ChangeCharacterColor(PlayerManager.Instance.player.gameObject, SecondPointColor, affected);
            for (int i = 0; i < StageManager.Instance.currentStageController.aliveMonsters.Count; i++)
            {
                ChangeCharacterColor(StageManager.Instance.currentStageController.aliveMonsters[i], SecondPointColor, affected);
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

            for (int i = 0; i < affected.Count; i++)
                if (affected[i]) affected[i].SetPropertyBlock(null);

            /* 기존 코드
            ChangeCharacterColor(PlayerManager.Instance.player.gameObject, Color.white);
            for (int i = 0; i < StageManager.Instance.currentStageController.aliveMonsters.Count; i++)
            {
                ChangeCharacterColor(StageManager.Instance.currentStageController.aliveMonsters[i], Color.white);
            }
            */
        }
    }

    private void ChangeMapColor(List<Renderer> renderers, Color color)
    {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        for (int i = 0; i < renderers.Count; i++)
        {
            renderers[i].GetPropertyBlock(mpb);
            mpb.SetColor(ColorID, color);
            renderers[i].SetPropertyBlock(mpb);
        }
    }

    private void ChangeCharacterColor(GameObject parent, Color color, List<SpriteRenderer> record = null)
    {
        SpriteRenderer characterRenderer = parent.GetComponentInChildren<SpriteRenderer>();

        if (characterRenderer == null) return;

        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        characterRenderer.GetPropertyBlock(mpb);
        mpb.SetColor(ColorID, color);
        characterRenderer.SetPropertyBlock(mpb);

        record?.Add(characterRenderer);
    }
    private void ResetMapColor(List<Renderer> rederers)
    {
        var mpb = new MaterialPropertyBlock();
        for (int i = 0; i < rederers.Count; i++)
            rederers[i].SetPropertyBlock(null);
    }
}
