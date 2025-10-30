using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SlashEffect : MonoBehaviour
{
    private void OnEnable()
    {
        float[] angles = new[]
        {
            Random.Range(-30f, -60f),
            Random.Range(110f, 150f),
        };
        
        
        transform.localRotation = Quaternion.Euler(new Vector3(0, 0, angles[Random.Range(0, 2)]));
    }
}
