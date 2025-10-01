using UnityEngine;
using UnityEngine.UI;

public class HeartIcon : MonoBehaviour
{
    [SerializeField] private Image fill; // 쓰면 유지, 안 쓰면 삭제해도 됨
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>(); // 같은 오브젝트에 Animator가 있다고 가정
    }

    public void Show(bool on)
    {
        if (on)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void SetFilled(bool on)
    {
        if (fill == null)
        {
            return;
        }

        if (on)
        {
            fill.enabled = true;
        }
        else
        {
            fill.enabled = false;
        }
    }

    public void Break()
    {
        if (animator) animator.SetTrigger("Break");
    }
}