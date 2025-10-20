using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffUnderEffect : MonoBehaviour
{
    private Quaternion initialRotation;

    void Start()
    {
        initialRotation = transform.rotation;
    }

    void LateUpdate()
    {
        // 부모 오브젝트 돌아가도 얘는 여전히 initialRotation으로 있도록
        transform.rotation = initialRotation;
    }
}
