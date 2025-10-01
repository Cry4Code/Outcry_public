using UnityEngine;
using UnityEngine.UI;

public class VolumeSettingsUI : UIBase
{
    [SerializeField] private VolumeSlider[] volumeSliders;
    [SerializeField] private Button ExitBtn;

    private void Awake()
    {
        if (ExitBtn != null)
        {
            ExitBtn.onClick.AddListener(OnExitButtonClicked);
        }
    }

    private void OnExitButtonClicked()
    {
        UIManager.Instance.Hide<VolumeSettingsUI>();
    }
}
