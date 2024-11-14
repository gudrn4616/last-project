using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;
using TMPro;

public class PopupConnection : UIBase
{
    [SerializeField] private TMP_InputField ip;
    [SerializeField] private TMP_InputField port;

    public override void Opened(object[] param)
    {
        ip.text = PlayerPrefs.GetString("ip", "");
        port.text = PlayerPrefs.GetString("port", "");
    }

    public override void HideDirect()
    {
        UIManager.Hide<PopupConnection>();
    }

    public void OnClickConnection()
    {
        if (string.IsNullOrEmpty(ip.text)) ip.text = "127.0.0.1";
        if (string.IsNullOrEmpty(port.text)) port.text = "3000";
        PlayerPrefs.SetString("ip", ip.text);
        PlayerPrefs.SetString("port", port.text);
        if (SocketManager.instance.isConnected)
        {
            SocketManager.instance.Disconnect();
        }
        SocketManager.instance.Init(ip.text, int.Parse(port.text));
        SocketManager.instance.Connect();
        HideDirect();
    }

    public void OnClickClose()
    {
        var ip = PlayerPrefs.GetString("ip");
        var port = PlayerPrefs.GetString("port");
        if (SocketManager.instance.isConnected)
        {
            SocketManager.instance.Disconnect();
        }
        SocketManager.instance.Init(ip, int.Parse(port));
        SocketManager.instance.Connect();
        HideDirect();
    }
}