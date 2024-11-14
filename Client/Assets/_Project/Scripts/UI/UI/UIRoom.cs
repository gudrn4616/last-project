using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using TMPro;
using Google.Protobuf.Collections;

public class UIRoom : UIBase
{
    [SerializeField] private List<ItemRoomSlot> slots;
    [SerializeField] private Button buttonExit;
    [SerializeField] private Button buttonStart;
    [SerializeField] private TMP_Text roomNo;
    [SerializeField] private TMP_Text roomName;
    [SerializeField] private TMP_Text roomCount;

    private List<UserInfo> users = new List<UserInfo>();
    private int maxUserCount;
    RoomData roomData;

    public override void Opened(object[] param)
    {
        UIManager.Hide<UIGnb>();
        roomData = (RoomData)param[0];
        SetRoomInfo(roomData);
    }

    public void SetRoomInfo(RoomData roomData)
    {
        roomNo.text = roomData.Id.ToString();
        roomName.text = roomData.Name;
        maxUserCount = roomData.MaxUserNum;
        roomCount.text = string.Format("{0}/{1}", roomData.Users.Count, roomData.MaxUserNum);
        for (int i = 0; i < roomData.Users.Count; i++)
        {
            if(users.Find(obj => obj.id == roomData.Users[i].Id) != null)
            {
                continue;
            }
            if (roomData.Users[i].Id == UserInfo.myInfo.id)
            {
                AddUserInfo(roomData.Users[i].ToUserInfo());
            }
            else
            {
                var userinfo = new UserInfo(roomData.Users[i]);
                AddUserInfo(userinfo);
            }
        }
        if(roomData.Users.Count == 0)
        {
            AddUserInfo(UserInfo.myInfo);
        }
        buttonStart.interactable = roomData.State == 0 && roomData.Users.Count > 1;
        buttonExit.interactable = roomData.State == 0;
        buttonStart.gameObject.SetActive(roomData.OwnerId == UserInfo.myInfo.id);
    }

    public void AddUserInfo(UserInfo userinfo)
    {
        users.Add(userinfo);
        for (int i = 0; i < slots.Count; i++)
        {
            var userInfo = users.Count > i ? users[i] : null;
            slots[i].SetItem(userInfo, userInfo != null ? null : (!SocketManager.instance.isConnected ? OnClickTestAddUser : null));
        }
        buttonStart.interactable = users.Count > 1;
        roomCount.text = string.Format("{0}/{1}", users.Count, maxUserCount);
    }

    public void RemoveUserInfo(string userId)
    {
        users.RemoveAll(obj => obj.id == userId);
        for (int i = 0; i < slots.Count; i++)
        {
            var userInfo = users.Count > i ? users[i] : null;
            slots[i].SetItem(userInfo, userInfo != null ? null : (!SocketManager.instance.isConnected ? OnClickTestAddUser : null));
        }
        roomCount.text = string.Format("{0}/{1}", users.Count, maxUserCount);
    }

    public void OnClickTestAddUser(int slot)
    {
        var newUser = UserInfo.CreateRandomUser();
        users.Add(newUser);
        slots[slot].SetItem(newUser, null);
        roomCount.text = string.Format("{0}/{1}", users.Count, maxUserCount);
        buttonStart.gameObject.SetActive(true);
        buttonStart.interactable = true;
    }

    public override void HideDirect()
    {
        UIManager.Hide<UIRoom>();
        UIManager.Show<UIGnb>();
    }

    public void OnClickGameStart()
    {
        //if (users.Count < 4) return;
        if (SocketManager.instance.isConnected)
        {
            GamePacket packet = new GamePacket();
            packet.GamePrepareRequest = new C2SGamePrepareRequest();
            SocketManager.instance.Send(packet);
        }
        else
        {
            var roles = new Dictionary<int, List<eRoleType>>() {
                { 4, new List<eRoleType>() { eRoleType.target, eRoleType.psychopass, eRoleType.hitman, eRoleType.hitman } },
                { 5, new List<eRoleType>() { eRoleType.target, eRoleType.psychopass, eRoleType.hitman, eRoleType.hitman, eRoleType.bodyguard } },
                { 6, new List<eRoleType>() { eRoleType.target, eRoleType.psychopass, eRoleType.hitman, eRoleType.hitman, eRoleType.hitman, eRoleType.bodyguard } },
                { 7, new List<eRoleType>() { eRoleType.target, eRoleType.psychopass, eRoleType.hitman, eRoleType.hitman, eRoleType.hitman, eRoleType.bodyguard, eRoleType.bodyguard } }
            };

            var role = roles[users.Count];

            users.ForEach(obj =>
            {
                var rand = Random.Range(0, role.Count);
                obj.roleType = role[rand];
                role.RemoveAt(rand);
            });

            var characters = new List<CharacterDataSO>(DataManager.instance.GetDatas<CharacterDataSO>());
            users.ForEach(obj =>
            {
                var rand = Random.Range(0, characters.Count);
                obj.selectedCharacterRcode = characters[rand].rcode;
                characters.RemoveAt(rand);
            });

            OnPrepare(users);
        }
    }

    public void OnPrepare(RepeatedField<UserData> users)
    {
        this.users.UpdateUserData(users);
        OnPrepare(this.users);
    }

    public async void OnPrepare(List<UserInfo> userDatas)
    {
        users = userDatas;
        //타겟 표시
        var idx = users.FindIndex(obj => obj.roleType == eRoleType.target);
        if(idx >= 0)
            slots[idx].OnTargetMark();
        //내 역할 표시
        var myIdx = users.FindIndex(obj => obj.id == UserInfo.myInfo.id);
        slots[myIdx].SetRoleIcon(users[myIdx].roleType);

        await Task.Delay(1000);
        
        for (int i = 0; i < users.Count; i++)
        {
            slots[i].OnChangeCharacter(users[i].selectedCharacterRcode);
        }

        await Task.Delay(3000);

        DataManager.instance.users = users;

        if (SocketManager.instance.isConnected)
        {
            if (UserInfo.myInfo.id == roomData.OwnerId)
            {
                GamePacket packet = new GamePacket();
                packet.GameStartRequest = new C2SGameStartRequest();
                SocketManager.instance.Send(packet);
            }
        }
        else
        {
            OnGameStart();
        }
    }

    public async void OnGameStart()
    {
        await SceneManager.LoadSceneAsync("Game");
    }

    public void OnClickExit()
    {
        HideDirect();
    }
}