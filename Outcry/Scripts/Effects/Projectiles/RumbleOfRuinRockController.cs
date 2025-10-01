using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RumbleOfRuinRockController : MonoBehaviour
{
    [SerializeField] private Transform target; // 투사체가 향할 타겟
    [SerializeField] private float speed = 10f;
    
    private void Start()
    {
        // 타겟이 설정되지 않은 경우, 가장 가까운 적을 타겟으로 설정
        if (target == null)
        {
            target = Camera.main.transform;
        }
    }
    
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
