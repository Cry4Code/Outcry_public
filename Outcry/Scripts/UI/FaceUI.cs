using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceUI : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Break()
    {
        if (animator) animator.SetTrigger("Break");
    }

}
