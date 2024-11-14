using System.Collections.Generic;
using UnityEngine;
using Ironcow;
#if USE_CLOUD_CODE
using Unity.Services.Core;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models.Data.Player;
using System.Runtime.CompilerServices;
using System;
using Unity.Services.Leaderboards;
using Unity.Services.CloudCode;

public class UgsManager : MonoSingleton<UgsManager>
{
    string lastLoginId { get => PlayerPrefs.GetString("ugs_id", ""); set => PlayerPrefs.SetString("ugs_id", value); }
    public bool isInit = false;
    bool isQuit = false;

    public async void Init()
    {
        await UnityServices.InitializeAsync();
        CheckDirty();
        isInit = true;
    }

    public async UniTask<bool> OnLogin()
    {
#if UNITY_EDITOR
        var userId = SystemInfo.deviceUniqueIdentifier;
#else
        var userId = Social.localUser.id;   
#endif
        userId = SystemInfo.deviceUniqueIdentifier;
        bool isLogin = false;
        if (string.IsNullOrEmpty(lastLoginId))
        {
            isLogin = await SignUp(userId.Replace("-", ""));
        }
        else
        {
            try
            {
                await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(lastLoginId, "Ha012#$%");
                isLogin = true;
                await SetLeaderBoard();
            }
            catch (RequestFailedException e)
            {
                isLogin = await SignUp(userId);
            }
        }
        return isLogin;
    }

    public async UniTask<bool> SignUp(string id)
    {
        try
        {
            id = id.Substring(0, Mathf.Min(20, id.Length));
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(id, "Ha012#$%");
            lastLoginId = id;
            await SetLeaderBoard();
            return true;
        }
        catch (RequestFailedException e)
        {
            try
            {
                await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(id, "Ha012#$%");
                lastLoginId = id;
                await SetLeaderBoard();
                return true;
            }
            catch (RequestFailedException e2)
            {

            }
        }
        return false;
    }

    public async UniTask<DateTime> OnLoadUserInfo()
    {
        var keys = new HashSet<string> { nameof(UserInfo) };
        var data = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);
        if (data.Count == 0)
        {
            DataManager.instance.userInfo = new UserInfo();
            UserInfo.myInfo.Init();
            await OnSaveUserInfo();
        }
        else
        {
            var json = data[nameof(UserInfo)].Value.GetAsString();
            UserInfo.FromJson(json);
        }
        return data.ContainsKey(nameof(UserInfo)) ? data[nameof(UserInfo)].Modified.Value.ToUniversalTime() : DateTime.Now.ToUniversalTime();
    }

    public async UniTask OnSaveUserInfo()
    {
        var datas = new Dictionary<string, object> { { nameof(UserInfo), UserInfo.myInfo.ToJson() } };
        var data = await CloudSaveService.Instance.Data.Player.SaveAsync(datas);
        if (!isQuit)
            UserInfo.myInfo.isDirty = false;
    }

    public async void OnSavePublicData()
    {
        var datas = new Dictionary<string, object> { { nameof(PublicData), UserInfo.publicData.ToJson() } };
        await CloudSaveService.Instance.Data.Player.SaveAsync(datas, new Unity.Services.CloudSave.Models.Data.Player.SaveOptions(new PublicWriteAccessClassOptions()));
    }

    public async UniTask<PublicData> OnLoadPublicData()
    {
        var keys = new HashSet<string> { nameof(PublicData) };
        var data = await CloudSaveService.Instance.Data.Player.LoadAsync(keys, new LoadOptions(new PublicReadAccessClassOptions()));
        if (data.Count == 0)
        {
            UserInfo.publicData = new PublicData();
            UserInfo.publicData.Init();
            OnSavePublicData();
        }
        else
        {
            var json = data[nameof(PublicData)].Value.GetAsString();
            UserInfo.publicData = JsonUtility.FromJson<PublicData>(json);
            if(UserInfo.publicData.selectedAnimals.Count == 0)
            {
                UserInfo.publicData.selectedAnimals.Add(new UserSelectedAnimal() { rcode = "CAD00001" });
                UserInfo.publicData.oasisLevel = 1;
            }
        }
        return UserInfo.publicData;
    }

    public async UniTask SetLeaderBoard()
    {
        await LeaderboardsService.Instance.AddPlayerScoreAsync("UserList", 0);
    }

    public async UniTask<PublicData> GetRandomUserPublicData()
    {
        var list = await LeaderboardsService.Instance.GetPlayerRangeAsync("UserList");
        list.Results.RemoveAll(obj => obj.PlayerId == lastLoginId);
        var randTarget = list.Results.RandomValue();
        GameManager.instance.anotherTargetId = randTarget.PlayerId;
        var keys = new HashSet<string> { nameof(PublicData) };
        var data = await CloudSaveService.Instance.Data.Player.LoadAsync(keys, new LoadOptions(new PublicReadAccessClassOptions(GameManager.instance.anotherTargetId)));
        return JsonUtility.FromJson<PublicData>(data[nameof(PublicData)].Value.GetAsString());
    }

    public async void CheckDirty()
    {
        while(true)
        {
            await UniTask.WaitUntil(() => isQuit || (DataManager.instance.userInfo != null && UserInfo.myInfo != null && UserInfo.myInfo.isDirty));
            if (isQuit) break;
            await UniTask.WaitForSeconds(1);
            await OnSaveUserInfo();
        }
    }

    public async UniTask OnAttackAnother(string targetId, string targetAnimal)
    {
        Dictionary<string, object> data = new Dictionary<string, object>();
        data.Add(nameof(targetId), targetId);
        data.Add(nameof(targetAnimal), targetAnimal);
        var result = await CloudCodeService.Instance.CallEndpointAsync("AttackAnother", data);
    }

    public async void OnApplicationQuit()
    {
        isQuit = true;
        await OnSaveUserInfo();
        //OnSavePublicData();
    }
}
#endif