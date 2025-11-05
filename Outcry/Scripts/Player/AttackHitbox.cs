using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public enum AttackState
{
    NormalAttack,
    NormalJumpAttack,
    SpecialAttack,
    DownAttack,
    
}

public class AttackHitbox : MonoBehaviour   
{
    private PlayerController controller;
    private PlayerAttack attack;
    [field : SerializeField] public AttackState AttackState { get; set; }
    public SpriteRenderer spriteRenderer;

    public bool specialAttackDamaged = false;
    
    public void Init(PlayerController player)
    {
        controller = player;
        attack = controller.Attack;
        attack.SetDamage(10);
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        specialAttackDamaged = false;
    }

    // 애니메이션 이벤트 받기용
    public void SetDamageInList(int index)
    {
        attack.SetDamageInList(index);
    }

    private async void OnTriggerEnter2D(Collider2D other)
    {
        if (controller.Attack.isStartParry)
        {
            if (other.TryGetComponent(out ICountable countable))
            {
                Debug.Log("[플레이어] 플레이어 패링 시도");

                if (other.TryGetComponent(out MonsterAttackController attackController))
                {
                    if (attackController.GetIsCountable())
                    {
                        controller.Attack.successParry = true;
                        controller.Condition.SetInvincible(controller.Data.parryInvincibleTime); 
                        controller.Attack.JustSpecialAttack(other.gameObject.GetComponentInChildren<Animator>());
                        Debug.Log("[플레이어] 플레이어 패링 성공");
                        countable.CounterAttacked();
                        // controller.Attack.successParry = false;
                        await EffectManager.Instance.PlayEffectByIdAndTypeAsync(PlayerEffectID.SuccessParrying, EffectType.Sprite, controller.gameObject, 
                            /*(other.transform.position - controller.transform.position).normalized 
                            * Vector2.Distance(other.transform.position, controller.transform.position) * 0.5f*/
                            (Vector3.right * 2f));
                        return;
                    }
                }
            }
        }

        if (controller.Attack.isStartJustAttack)
        {
            if (other.TryGetComponent(out ICountable countable))
            {
                if (other.TryGetComponent(out MonsterAttackController attackController))
                {
                    if (attackController.GetIsCountable())
                    {
                        Debug.Log("[플레이어] 플레이어 저스트 어택!");
                        controller.Attack.successJustAttack = true;
                        controller.Condition.SetInvincible(0.5f + controller.Attack.justAttackStopTime);
                        
                        controller.Attack.JustSpecialAttack(other.gameObject.GetComponentInChildren<Animator>());
                        countable.CounterAttacked();
                        controller.Condition.health.Add(3);
                        
                        int stageId = StageManager.Instance.CurrentStageData.Stage_id;
                        if (stageId != StageID.Village)
                        {
                            UGSManager.Instance.LogDoAction(stageId, PlayerEffectID.JustSpecialAttack);
                        }
                        EffectManager.Instance.PlayEffectsByIdAsync(PlayerEffectID.JustSpecialAttack, EffectOrder.SpecialEffect, controller.gameObject).Forget();
                    } 
                }
                else
                {
                    controller.Condition.SetInvincible(0.5f + controller.Attack.justAttackStopTime);
                    countable.CounterAttacked();
                }
                return;
            }
        }
            
        if (other.TryGetComponent<IDamagable>(out var damagable))
        {
            // 패리랑 섬단은 데미지 판단 여기서 안함
            if (controller.Attack.isStartParry || controller.Attack.isStartSpecialAttack) return;
            
            ShakeCameraUsingState();
            damagable?.TakeDamage(attack.AttackDamage + attack.AdditionalDamage);
            Debug.Log($"[플레이어] 플레이어가 몬스터에게 {attack.AttackDamage + attack.AdditionalDamage} 만큼 데미지 줌");
        } 
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 섬단 데미지 판단 여기서 함
        if (controller.Attack.isStartSpecialAttack)
        {
            if (other.TryGetComponent<IDamagable>(out var damagable))
            {
                CheckSpecialAttack(damagable);
            }

            try
            {
                var parentDamagable = other.GetComponentInParent<IDamagable>();
                if (parentDamagable != null)
                {
                    CheckSpecialAttack(parentDamagable);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"[플레이어] 몬스터의 Damagable을 찾을 수 없음. 사유 : {e}");
            }
        }
    }

    private void CheckSpecialAttack(IDamagable damagable)
    {
        if (!specialAttackDamaged)
        {
            // 저섬 성공
            if (controller.Attack.isStartSpecialAttack && controller.Attack.successJustAttack)
            {
                controller.Attack.SetDamage(controller.Data.justSpecialAttackDamage);
                damagable?.TakeDamage(attack.AttackDamage + attack.AdditionalDamage);
                Debug.Log($"[플레이어] 플레이어가 몬스터에게 저스트 섬단으로 {attack.AttackDamage + attack.AdditionalDamage} 만큼 데미지 줌");
            }
            
            // 저섬 실패
            else if (controller.Attack.isStartSpecialAttack)
            {
                controller.Attack.SetDamage(controller.Data.specialAttackDamage);
                damagable?.TakeDamage(attack.AttackDamage + attack.AdditionalDamage);
                Debug.Log($"[플레이어] 플레이어가 몬스터에게 일반 섬단으로 {attack.AttackDamage + attack.AdditionalDamage} 만큼 데미지 줌");
            }

        }
        
        specialAttackDamaged = true;

    }

    private void ShakeCameraUsingState()
    {
        switch (AttackState)
        {
            case AttackState.NormalAttack :
                if (controller.Attack.AttackCount == controller.Attack.MaxAttackCount)
                {
                    CameraManager.Instance.ShakeCamera(0.1f, 2f, 2f, EffectOrder.Player);
                }
                else
                {
                    CameraManager.Instance.ShakeCamera(0.1f, 1f, 2f, EffectOrder.Player);
                }
                break;
            case AttackState.NormalJumpAttack :
                break;
            case AttackState.DownAttack :
                CameraManager.Instance.ShakeCamera(0.1f, 1f, 2f, EffectOrder.Player);
                break;
            case AttackState.SpecialAttack :
                CameraManager.Instance.ShakeCamera(0.1f, 2f, 2f, EffectOrder.Player);
                break;
        }
    }
}
