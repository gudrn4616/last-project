using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;

public class PopupSetting : UIBase
{
    [SerializeField] private Slider bgm;
    [SerializeField] private Slider effect;
    public override void Opened(object[] param)
    {
        
    }

    public override void HideDirect()
    {
        UIManager.Hide<PopupSetting>();
    }

    public void OnChangeBgm(float value)
    {
        AudioManager.instance.SetBgmVolume(value);
    }

    public void OnChangeEffect(float value)
    {
        AudioManager.instance.SetEffectVolume(value);
    }

    public void OnLogout()
    {
        DataManager.instance.OnLogout();
        HideDirect();
    }
}