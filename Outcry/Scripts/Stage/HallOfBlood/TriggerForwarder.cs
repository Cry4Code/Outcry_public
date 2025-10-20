using System;
using UnityEngine;

/// <summary>
/// 2D 트리거 이벤트를 감지하여 외부로 전달하는 역할
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class TriggerForwarder : MonoBehaviour
{
    // OnTriggerEnter2D 이벤트가 발생했을 때 호출될 액션
    public event Action<Collider2D> OnTriggerEnter_2D;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 등록된 함수가 있다면 충돌한 객체 정보를 전달하며 호출
        OnTriggerEnter_2D?.Invoke(other);
    }
}
