using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;
using TMPro;

public class PopupCardSelection : UIListBase<Card>
{
    [SerializeField] private UIPagingViewController uiPagingViewController;
    [SerializeField] private Button use;
    [SerializeField] private Transform weaponSlot;
    [SerializeField] private List<Transform> equipSlots;
    [SerializeField] private List<Transform> debuffSlots;

    Card selectCard;
    UserInfo targetUserInfo;
    string targetRcode;

    public override void Opened(object[] param)
    {
        targetUserInfo = DataManager.instance.users.Find(obj => obj.id == (string)param[0]);
        targetRcode = (string)param[1];
        SetList();
    }

    public override void HideDirect()
    {
        UIGame.instance.SetDeckCount();
        UIManager.Hide<PopupCardSelection>();
    }

    public void ClearWeapon()
    {
        if (weaponSlot.childCount > 0)
            Destroy(weaponSlot.GetChild(0).gameObject);
    }

    public void AddWeapon(CardDataSO data)
    {
        var item = Instantiate(itemPrefab, weaponSlot);
        item.Init(data, OnClickItem);
        item.rectTransform.anchoredPosition = Vector2.zero;
        item.rectTransform.sizeDelta = new Vector2(180, 246);
    }

    public void ClearEquips()
    {
        for (int i = 0; i < equipSlots.Count; i++)
        {
            if (equipSlots[i].childCount > 0)
                Destroy(equipSlots[i].GetChild(0).gameObject);
        }
    }

    public void AddEquip(CardDataSO data, int idx)
    {
        var slot = equipSlots[idx];
        var item = Instantiate(itemPrefab, slot);
        item.Init(data, OnClickItem);
        item.rectTransform.anchoredPosition = Vector2.zero;
        item.rectTransform.sizeDelta = new Vector2(180, 246);
    }

    public void ClearDebuffs()
    {
        for (int i = 0; i < debuffSlots.Count; i++)
        {
            if (debuffSlots[i].childCount > 0)
                Destroy(debuffSlots[i].GetChild(0).gameObject);
        }
    }

    public void AddDebuff(CardDataSO data, int idx)
    {
        var slot = debuffSlots[idx];
        var item = Instantiate(itemPrefab, slot);
        item.Init(data, OnClickItem);
        item.rectTransform.anchoredPosition = Vector2.zero;
        item.rectTransform.sizeDelta = new Vector2(180, 246);
    }

    public override void SetList()
    {
        ClearList();
        ClearEquips();
        ClearWeapon();
        ClearDebuffs();
        for (int i = 0; i < targetUserInfo.handcardCount; i++)
        {
            var item = AddItem();
            item.Init(i, OnClickItem);
        }
        if(targetUserInfo.weapon != null)
        {
            AddWeapon(targetUserInfo.weapon);
        }
        for (int i = 0; i < targetUserInfo.equips.Count; i++)
        {
            AddEquip(targetUserInfo.equips[i], i);
        }
        for (int i = 0; i < targetUserInfo.debuffs.Count; i++)
        {
            AddDebuff(targetUserInfo.debuffs[i], i);
        }
    }

    public void OnClickItem(Card card)
    {
        selectCard?.OnSelect(false);
        selectCard = card;
        card.OnSelect(true);
    }

    public void OnClickUse()
    {
        if (SocketManager.instance.isConnected)
        {
            GamePacket packet = new GamePacket();
            SelectCardType selectType = selectCard.transform.parent == weaponSlot ? SelectCardType.Weapon : equipSlots.Find(obj => obj == selectCard.transform.parent) != null ? SelectCardType.Equip : debuffSlots.Find(obj => obj == selectCard.transform.parent) != null ? SelectCardType.Debuff : SelectCardType.Hand;
            packet.CardSelectRequest = new C2SCardSelectRequest() { SelectType = selectType, SelectCardType = selectType == 0 ? CardType.None : selectCard.cardData.cardType };
            SocketManager.instance.Send(packet);
        }
        else
        {
            GameManager.instance.OnSelectCard(targetUserInfo, selectCard.cardData.rcode, UserInfo.myInfo, targetRcode);
        }
    }
}