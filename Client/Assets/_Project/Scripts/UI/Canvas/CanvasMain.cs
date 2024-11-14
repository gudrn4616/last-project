using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;

public class CanvasMain : CanvasBase<CanvasMain>
{
    protected override void Awake()
    {
        base.Awake();
        if (!SocketManager.instance.isConnected)
        {
            UIManager.Show<PopupLogin>();
        }
        else
        {
            UIManager.Show<UIMain>();
            UIManager.Show<UITopBar>();
            UIManager.Show<UIGnb>();
        }
    }
}