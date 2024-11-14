using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;
using TMPro;
using System;

public class PopupBattle : UIListBase<Card>
{
    [SerializeField] private UIPagingViewController uiPagingViewController;
    [SerializeField] private GameObject select;
    [SerializeField] private Button useButton;
    [SerializeField] private Button takeButton;
    [SerializeField] private Transform useCardSlot;
    [SerializeField] private TMP_Text title;
    [SerializeField] private GameObject nonCardText;
    [SerializeField] private TMP_Text timer;

    float time = 0;

    CardDataSO targetCard;
    UserInfo targetUser;
    List<CardDataSO> cards;
    Action<int, string> callback;
    public override void Opened(object[] param)
    {
        targetCard = DataManager.instance.GetData<CardDataSO>(param[0].ToString());
        targetUser = DataManager.instance.users.Find(obj => obj.id == (string)param[1]);
        if (SocketManager.instance.isConnected)
        {
            callback = (Action<int, string>)param[2];
        }
        SetList();
        uiPagingViewController.OnChangeValue += OnChangeValue;
        uiPagingViewController.OnMoveStart += OnMoveStart;
        uiPagingViewController.OnMoveEnd += OnMoveEnd;
        title.text = targetCard.displayName;
        AddUseCard(targetCard);
    }

    public void SetActiveControl(bool isActive)
    {
        useButton.interactable = isActive;
        takeButton.interactable = isActive;
    }

    public void OnChangeValue(int idx)
    {
        useButton.interactable = UserInfo.myInfo.handCards[idx].isUsable;
    }

    public override void HideDirect()
    {
        UIManager.Hide<PopupBattle>();
    }

    public void AddUseCard(CardDataSO data)
    {
        var item = Instantiate(itemPrefab, useCardSlot);
        item.Init(data);
    }

    public override void SetList()
    {
        ClearList();
        cards = UserInfo.myInfo.handCards.FindAll(obj => obj.rcode == targetCard.defCard);
        foreach (var data in cards)
        {
            var item = AddItem();
            item.Init(data, OnClickItem);
        }
        select.SetActive(items.Count > 0);
        nonCardText.SetActive(items.Count == 0);
    }

    public void OnClickItem(CardDataSO data)
    {
        
    }

    private void OnMoveStart()
    {
        select.SetActive(false);
        useButton.interactable = false;
    }

    private void OnMoveEnd()
    {
        select.SetActive(true);
        useButton.interactable = true;
    }

    public void OnClickUse()
    {
        var idx = uiPagingViewController.selectedIdx;
        var card = cards[idx];
        if (SocketManager.instance.isConnected)
        {
            callback.Invoke((int)card.cardType, targetUser.id);
        }
        else
        {
            UserInfo.myInfo.OnUseCard(card);
            GameManager.instance.OnSelectCard(UserInfo.myInfo, card.rcode, targetUser, targetCard.rcode);
            AddUseCard(card);
            SetList();
        }

    }

    public void OnClickDamage()
    {
        if (SocketManager.instance.isConnected)
        {
            callback.Invoke(0, "");
        }
        HideDirect();
    }

    public void SetUserSelectTurn(int time)
    {
        this.time = time;
        timer.text = time.ToString();
        StartCoroutine(SetTimer());
    }

    IEnumerator SetTimer()
    {
        while (time >= 0)
        {
            time -= Time.deltaTime;
            timer.text = string.Format("{0:0}", time);
            yield return null;
        }
    }
}