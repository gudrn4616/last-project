using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;
using TMPro;

public class PopupNickname : UIBase
{
    [SerializeField] TMP_InputField nickname;

    public override void Opened(object[] param)
    {
        
    }

    public override void HideDirect()
    {
        UIManager.Hide<PopupNickname>();
    }

    public void OnClickOk()
    {
        if (SocketManager.instance.isConnected)
        {
            GamePacket packet = new GamePacket();
            
            //SocketManager.instance.Send(packet);
        }
        else
        {
            UserInfo.myInfo.nickname = nickname.text;
            UIManager.Get<UITopBar>().SetUserInfo(UserInfo.myInfo);
        }
    }

    public void OnNicknameResult()
    {
        HideDirect();
    }
}