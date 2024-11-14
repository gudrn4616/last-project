using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;
using TMPro;
using Unity.Multiplayer.Playmode;

public class PopupLogin : UIBase
{
    [SerializeField] private GameObject touch;
    [SerializeField] private GameObject buttonSet;
    [SerializeField] private GameObject register;
    [SerializeField] private GameObject login;
    [SerializeField] private TMP_InputField loginId;
    [SerializeField] private TMP_InputField loginPassword;
    [SerializeField] private TMP_InputField regId;
    [SerializeField] private TMP_InputField regPassword;
    [SerializeField] private TMP_InputField regPasswordRe;
    public override void Opened(object[] param)
    {
        register.SetActive(false);
        login.SetActive(false);

        var tags = CurrentPlayer.ReadOnlyTags();
        if (tags.Length == 0)
        {
            tags = new string[1] { "player1" };
        }
        loginId.text = PlayerPrefs.GetString("id" + tags[0], "");
        loginPassword.text = PlayerPrefs.GetString("password" + tags[0], "");
    }

    public override void HideDirect()
    {
        UIManager.Hide<PopupLogin>();
    }

    public void OnClickLogin()
    {
        if (!SocketManager.instance.isConnected)
        {
            var ip = PlayerPrefs.GetString("ip", "127.0.0.1");
            var port = PlayerPrefs.GetString("port", "5555");
            SocketManager.instance.Init(ip, int.Parse(port));
            SocketManager.instance.Connect(() =>
            {
                buttonSet.SetActive(false);
                login.SetActive(true);

            });
        }
        else
        {
            buttonSet.SetActive(false);
            login.SetActive(true);
        }
    }

    public void OnClickRegister()
    {
        if (!SocketManager.instance.isConnected)
        {
            var ip = PlayerPrefs.GetString("ip", "127.0.0.1");
            var port = PlayerPrefs.GetString("port", "5555");
            SocketManager.instance.Init(ip, int.Parse(port));
            SocketManager.instance.Connect(() =>
            {
                buttonSet.SetActive(false);
                register.SetActive(true);
            });
        }
        else
        {
            buttonSet.SetActive(false);
            register.SetActive(true);
        }
    }

    public void OnClickSendLogin()
    {
        GamePacket packet = new GamePacket();
        packet.LoginRequest = new C2SLoginRequest() { Id = loginId.text, Password = loginPassword.text };
        var tags = CurrentPlayer.ReadOnlyTags();
        if(tags.Length == 0)
        {
            tags = new string[1] { "player1" };
        }
        PlayerPrefs.SetString("id" + tags[0], loginId.text);
        PlayerPrefs.SetString("password" + tags[0], loginPassword.text);
        SocketManager.instance.Send(packet);
        //OnLoginEnd(true);
    }

    public void OnClickSendRegister()
    {
        if(regPassword.text != regPasswordRe.text)
        {
            UIManager.ShowAlert("비밀번호가 다릅니다.");
            return;
        }
        GamePacket packet = new GamePacket();
        packet.RegisterRequest = new C2SRegisterRequest() { Id = regId.text, Email = regId.text, Password = regPassword.text };
        var tags = CurrentPlayer.ReadOnlyTags();
        if (tags.Length == 0)
        {
            tags = new string[1] { "player1" };
        }
        PlayerPrefs.SetString("id" + tags[0], regId.text);
        PlayerPrefs.SetString("password" + tags[0], regPassword.text);
        SocketManager.instance.Send(packet);
    }

    public void OnClickCancelRegister()
    {
        buttonSet.SetActive(true);
        register.SetActive(false);
    }

    public void OnClickCancelLogin()
    {
        buttonSet.SetActive(true);
        login.SetActive(false);
    }

    public void OnTouchScreen()
    {
        touch.SetActive(false);
        buttonSet.SetActive(false);
    }

    public async void OnLoginEnd(bool isSuccess)
    {
        if (isSuccess)
        {
            await UIManager.Show<UIMain>();
            UIManager.Get<UIMain>().OnRefreshRoomList();
            HideDirect();
            await UIManager.Show<UITopBar>();
            await UIManager.Show<UIGnb>();
        }
        else
        {
            UIManager.ShowAlert("로그인 실패");
        }
    }

    public void OnRegisterEnd(bool isSuccess)
    {
        if (isSuccess)
        {
            register.SetActive(false);
            login.SetActive(true);
            var tags = CurrentPlayer.ReadOnlyTags();
            if (tags.Length == 0)
            {
                tags = new string[1] { "player1" };
            }
            loginId.text = PlayerPrefs.GetString("id" + tags[0]);
            loginPassword.text = PlayerPrefs.GetString("password" + tags[0]);
        }
        else
        {
            UIManager.ShowAlert("회원가입 실패");
        }

    }

    public void OnClickChangeServer()
    {
        UIManager.Show<PopupConnection>();
    }
}