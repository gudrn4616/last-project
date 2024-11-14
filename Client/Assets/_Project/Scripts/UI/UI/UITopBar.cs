using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;
using TMPro;

public class UITopBar : UIBase
{
    [SerializeField] private Image thumbnail;
    [SerializeField] private TMP_Text nickname;

    public override void Opened(object[] param)
    {
        
    }

    public override void HideDirect()
    {
        UIManager.Hide<UITopBar>();
    }

    public void OnClickSetting()
    {
        UIManager.Show<PopupSetting>();
    }

    public void OnClickNickname()
    {
        UIManager.Show<PopupNickname>();
    }

    public void SetUserInfo(UserInfo userInfo)
    {
        nickname.text = userInfo.nickname;
    }
}