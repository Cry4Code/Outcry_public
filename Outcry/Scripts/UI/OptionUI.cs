using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionUI : UIBase
{
    [SerializeField] private Button volumeBtn;
    [SerializeField] private Button keyBindBtn;
    [SerializeField] private Button EmailLinkBtn;
    [SerializeField] private Button ExitBtn;

    private void Awake()
    {
        volumeBtn.onClick.AddListener(OnClickVolume);
        keyBindBtn.onClick.AddListener(OnClickKeyBind);
        EmailLinkBtn.onClick.AddListener(OnClickEmailLink);
        ExitBtn.onClick.AddListener(OnClickExit);
    }

    private void OnClickVolume()
    {
        Debug.Log("Volume Clicked");
        UIManager.Instance.Show<VolumeSettingsUI>();
    }

    private void OnClickKeyBind()
    {
        Debug.Log("Key Bind Clicked");

        // TODO: KeyBindUI 구현 후 연결
    }

    private void OnClickEmailLink()
    {
        Debug.Log("Email Link Clicked");
        EmailUI email = UIManager.Instance.Show<EmailUI>();
        email.Setup(EEmailUIType.Link);
    }

    private void OnClickExit()
    {
        Debug.Log("Exit Clicked");
        UIManager.Instance.Hide<OptionUI>();
    }
}
