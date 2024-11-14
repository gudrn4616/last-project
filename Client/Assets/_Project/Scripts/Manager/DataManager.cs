using Ironcow;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DataManager : DataManagerBase<DataManager>
{
    public List<UserInfo> users = new List<UserInfo>();

    public override void Init()
    {
        base.Init();
        userInfo = UserInfo.CreateRandomUser();
    }

    public async void OnLogout()
    {
        if (SocketManager.instance.isConnected)
        {
            SocketManager.instance.Disconnect();
        }
        else
        {
            if (SceneManager.GetActiveScene().name != "Main")
            {
                await SceneManager.LoadSceneAsync("Main");
            }
            else
            {
                UIManager.Hide<UITopBar>();
                UIManager.Hide<UIGnb>();
                await UIManager.Show<PopupLogin>();
            }
        }
    }
}
