using System;
using System.Collections;
using System.Collections.Generic;
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

    public void Init(PlayerController player)
    {
        controller = player;
        attack = controller.Attack;
        attack.SetDamage(10);
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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
                        await EffectManager.Instance.PlayEffectByIdAndTypeAsync(PlayerEffectID.SuccessParrying, EffectType.Sprite, null, 
                            controller.transform.position + ((other.transform.position - controller.transform.position).normalized 
                            * Vector2.Distance(other.transform.position, controller.transform.position) * 0.5f));
                    }
                }
            }

            return;
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
                        controller.Attack.SetDamage(controller.Data.justSpecialAttackDamage);
                        controller.Attack.JustSpecialAttack(other.gameObject.GetComponentInChildren<Animator>());
                        countable.CounterAttacked();
                        controller.Attack.successJustAttack = false;
                        await EffectManager.Instance.PlayEffectsByIdAsync(PlayerEffectID.JustSpecialAttack, EffectOrder.Player, null, 
                            controller.transform.position + ((other.transform.position - controller.transform.position).normalized 
                                                             * Vector2.Distance(other.transform.position, controller.transform.position) * 0.5f));
                    } 
                }
                else
                {
                    controller.Condition.SetInvincible(0.5f + controller.Attack.justAttackStopTime);
                    countable.CounterAttacked();
                }
            }
        }
            
        if (other.TryGetComponent<IDamagable>(out var damagable))
        {
            ShakeCameraUsingState();
            damagable?.TakeDamage(attack.AttackDamage + attack.AdditionalDamage);
            if (!controller.Attack.successJustAttack) await EffectManager.Instance.PlayEffectsByIdAsync(PlayerEffectID.NormalAttack,  EffectOrder.Player, 
                null, 
                controller.transform.position + ((other.transform.position - controller.transform.position).normalized 
                                                 * Vector2.Distance(other.transform.position, controller.transform.position) * 0.5f));
            Debug.Log($"[플레이어] 플레이어가 몬스터에게 {attack.AttackDamage + attack.AdditionalDamage} 만큼 데미지 줌");
        } 
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
