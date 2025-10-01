using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] private Transform target; // 투사체가 향할 타겟
    [SerializeField] private float speed = 10f;
    [SerializeField] private float damage = 10f; // 투사체가 적에게 입힐 피해량 필요한 경우 사용.
    private void Update()
    {
        MoveTowardsTarget();
    }

    private void MoveTowardsTarget()
    {
        //x, y축 이동만 고려. 월드 좌표 기준
        if (target == null) return;
        
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * (speed * Time.deltaTime);
        
        Debug.Log($"Projectile Y: {transform.position.y}, Target Y: {target.position.y}");
    }
}
