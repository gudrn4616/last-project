
using Ironcow;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public partial class UserInfo
{
    public static UserInfo myInfo { get => DataManager.instance.userInfo; set => DataManager.instance.userInfo = value; }
}