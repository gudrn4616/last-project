using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;
using TMPro;
using JetBrains.Annotations;
using Google.Protobuf.Collections;

public class PopupPleaMarket : UIBase
{
    [SerializeField] private Transform grid;
    [SerializeField] private List<GameObject> cardObjects;
    [SerializeField] private List<TMP_Text> nameTexts;
    [SerializeField] private TMP_Text timer;

    List<Card> cards = new List<Card>();
    string id;
    float time = 0;
    public bool isMyTurn;

    public bool isInitCards { get => cards.Count > 0; }
    public override async void Opened(object[] param)
    {

    }

    public async void Init(string id)
    { 
        this.id = id;
        GameManager.instance.SetPleaMarketCards();
        cardObjects.ForEach(obj => obj.SetActive(false));
        for (int i = 0; i < GameManager.instance.pleaMarketCards.Count; i++)
        {
            var cardData = GameManager.instance.pleaMarketCards[i];
            var card = Instantiate(await ResourceManager.instance.LoadAsset<Card>("Card", eAddressableType.Prefabs), grid);
            card.Init(cardData, i, (id == UserInfo.myInfo.id ? OnClickItem : null));
            cards.Add(card);
            cardObjects[i].SetActive(true);
        }
    }

    public void OnClickItem(int idx)
    {
        if (!isMyTurn) return;
        if (SocketManager.instance.isConnected)
        {
            GamePacket packet = new GamePacket();
            packet.FleaMarketPickRequest = new C2SFleaMarketPickRequest() { PickIndex = idx };
            SocketManager.instance.Send(packet);
            StopAllCoroutines();
            timer.text = "";
        }
        else
        {
            GameManager.instance.OnSelectCard(UserInfo.myInfo, cards[idx].cardData.rcode, DataManager.instance.users.Find(obj => obj.id == id), "CAD00010");
        }
    }

    public void SetNextUserTurn(UserInfo userinfo, int selectedCardIdx)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].Init(GameManager.instance.pleaMarketCards[i], i, userinfo.id == UserInfo.myInfo.id ? OnClickItem : null);
        }
        SetNameText(selectedCardIdx, DataManager.instance.users.Find(obj => obj.id == id));
        id = userinfo.id;
    }

    public void SetNameText(int idx, UserInfo userinfo)
    {
        nameTexts[idx].text = userinfo.nickname;
    }

    public override void HideDirect()
    {
        UIManager.Hide<PopupPleaMarket>();
    }

    public async void SetCards(RepeatedField<CardType> cards)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            var cardData = cards[i].GetCardData();
            var card = Instantiate(await ResourceManager.instance.LoadAsset<Card>("Card", eAddressableType.Prefabs), grid);
            card.Init(cardData, i, OnClickItem);
            this.cards.Add(card);
        }
    }

    public void OnSelectedCard(RepeatedField<int> idxs)
    {
        for(int i = 0; i < idxs.Count; i++)
        {
            this.cards[idxs[i]].SetActive(false);
        }
    }

    public void SetUserSelectTurn(int time)
    {
        isMyTurn = true;
        if(time == 0)
        {
            time = 5;
        }
        this.time = time;
        timer.text = time.ToString();
        //StartCoroutine(SetTimer());
    }

    IEnumerator SetTimer()
    {
        while(time > 0)
        {
            time -= Time.deltaTime;
            timer.text = string.Format("{0:0}", time);
            yield return null;
        }
        OnClickItem(cards.FindIndex(obj => obj.gameObject.activeInHierarchy));
    }
}