using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 메테오 발사체  
/// </summary>
public class MeteorController : ProjectileBase
{

    /// - 속도는 8f (Rigidbody Mode = Kinematic이기 때문에, 내부 코드로 이동 구현 필요)
    private float velocity = 16f;

    private float adjustedVelocity;

    /// - 시작 시 Z축 각도가 -22.5~22.5 사이의 랜덤한 값으로 돌아감 (총합 45도)
    private float randomAngleZ;

    private Vector3 downDirection;
    /// - 크기가 1배에서 2.5 사이의 랜덤한 값으로 지정됨
    private float randomSize;

    // - 플레이어 or 바닥 or 플랫폼에 닿으면 사라짐
    public LayerMask groundMask;
    private void OnEnable()
    {
        randomAngleZ = Random.Range(-22.5f, 22.5f);
        randomSize = Random.Range(1f, 2.5f);
        
        // 생성될 때 랜덤 각도와 크기 지정
        transform.rotation = Quaternion.Euler(new  Vector3(0f, 0f, randomAngleZ));
        transform.localScale = Vector3.one * randomSize;

        downDirection = -transform.up;
        EffectManager.Instance.PlayEffectByIdAndTypeAsync(Stage2BossEffectID.Meteor * 10 + 1, EffectType.Sound, gameObject).Forget();
    }

    private void FixedUpdate()
    {
        adjustedVelocity = velocity * Time.deltaTime;
        transform.position += (downDirection * adjustedVelocity);
    }
    
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 바닥/플랫폼에 닿았는가?
        // 방법 설명 : (여기서, 땅의 레이어 번호는 6번, 플랫폼은 7번, 플레이어는 1번으로 가정)
        // 닿은걸 기준으로 설명
        // collision.gameObject.layer = 6
        // 1 << collision.gameObject.layer = 1 << 6 = 0010 0000
        // groundMask = 0110 0000
        // 따라서, 만약 바닥이나 플랫폼에 닿았다면 and 연산을 했을 때 
        // 6번 자리 혹은 7번 자리가 1이 됨. 그래서 0이 아니게 됨.
        
        // 그런데 만약 플레이어에게 닿은거라면
        // 1 << collision.gameObject.layer = 1 << 1 = 0000 0001
        // groundMask = 0100 0000
        // 따라서 and 연산을 하면 0이 되기 때문에, 이 부분은 스킵된다.
        if ((1 << collision.gameObject.layer & groundMask) != 0)
        {
            RequestRelease();
            return;
        }
        
        if ((1 << collision.gameObject.layer & playerLayer.value) != 0 &&
                 collision.TryGetComponent(out IDamagable damagable))
        {
            if (damage > 0)
            {
                damagable.TakeDamage(damage);
                Debug.Log($"[{name}] Player hit - damage {damage}");
            }
            RequestRelease();
        }
    }


    protected override void OnPrepareRelease()
    {
        
    }
}
