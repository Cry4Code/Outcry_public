using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SlashEffect : MonoBehaviour
{
    private void OnEnable()
    {
        transform.localRotation = Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360)));
    }
}
