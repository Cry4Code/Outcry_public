using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    [SerializeField] private Slider progressBar; // 인스펙터에서 프로그레스 바 연결
    [SerializeField] private TextMeshProUGUI progressText; // 진행 상황 텍스트

    public void UpdateProgress(float progress, string text = null)
    {
        if (progressBar != null)
        {
            progressBar.value = progress;
        }
        if (progressText != null && text != null)
        {
            progressText.text = text;
        }
    }
}