using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Unity.VisualScripting;
using Google.Protobuf.Collections;
using System;

[Serializable]
public class ResultUserSlot
{
    public GameObject root;
    public TMP_Text nickname;
    public Image character;
}

public class PopupResult : UIBase
{
    [SerializeField] private List<ResultUserSlot> resultUserSlots;
    [SerializeField] private Image role;
    [SerializeField] private TMP_Text roleText;
    [SerializeField] private GameObject exit;

    private string[] roleTexts = new string[3] { "보디가드와 타겟", "히트맨", "싸이코패스" };

    public override async void Opened(object[] param)
    {
        var winners = (RepeatedField<string>)param[0];
        var winnerType = (WinType)param[1];
        role.gameObject.SetActive(true);
        roleText.gameObject.SetActive(true);
        role.sprite = await ResourceManager.instance.LoadAsset<Sprite>("Role_" + winnerType.ToString(), eAddressableType.Thumbnail);
        roleText.text = roleTexts[(int)winnerType];
        await Task.Delay(500);
        for (int i = 0; i < resultUserSlots.Count; i++)
        {
            if (winners.Count > i)
            {
                resultUserSlots[i].root.SetActive(true);
                var userInfo = DataManager.instance.users.Find(obj => obj.id == winners[i]);
                resultUserSlots[i].nickname.text = userInfo.nickname;
                resultUserSlots[i].character.sprite = await ResourceManager.instance.LoadAsset<Sprite>(userInfo.selectedCharacterRcode, eAddressableType.Thumbnail); 
            }
            else
            {
                resultUserSlots[i].root.SetActive(false);
            }
        }
        await Task.Delay(2000);
        exit.gameObject.SetActive(true);
    }

    public override void HideDirect()
    {
        UIManager.Hide<PopupResult>();
    }

    public async void OnClickExit()
    {
        UserInfo.myInfo.Clear();
        DataManager.instance.users.Clear();
        await SceneManager.LoadSceneAsync("Main");
    }
}