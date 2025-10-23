using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIBase : MonoBehaviour
{
    public Canvas canvas;

    public virtual void Open()
    {
        gameObject.SetActive(true);
    }

    public virtual void Close() 
    {
        EffectManager.Instance.ButtonSound();
        gameObject.SetActive(false);
    }

}
