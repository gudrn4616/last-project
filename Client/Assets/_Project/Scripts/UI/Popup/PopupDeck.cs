using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;
using TMPro;

public class PopupDeck : UIListBase<Card>
{
    [SerializeField] private UIPagingViewController uiPagingViewController;
    [SerializeField] private GameObject select;
    [SerializeField] private Button use;
    [SerializeField] private Transform weaponSlot;
    [SerializeField] private List<Transform> equipSlots;
    public override void Opened(object[] param)
    {
        SetList();
        if (GameManager.instance.userCharacter.IsState<CharacterPrisonState>() ||
            GameManager.instance.userCharacter.IsState<CharacterStopState>())
        {
            use.gameObject.SetActive(false);
        }
        uiPagingViewController.OnChangeValue += OnChangeValue;
        uiPagingViewController.OnMoveStart += OnMoveStart;
        uiPagingViewController.OnMoveEnd += OnMoveEnd;
        UIGame.instance.OnSelectDirectTarget(false);
    }

    private void Update()
    {
        if(GameManager.instance.userCharacter.IsState<CharacterIdleState>() ||
            GameManager.instance.userCharacter.IsState<CharacterWalkState>())
        {
            use.gameObject.SetActive(true);
        }

    }

    public void OnChangeValue(int idx)
    {
        use.interactable = UserInfo.myInfo.handCards[idx].isUsable;
    }

    public override void HideDirect()
    {
        UIGame.instance.SetDeckCount();
        UIManager.Hide<PopupDeck>();
    }

    public void ClearWeapon()
    {
        if (weaponSlot.childCount > 0)
            Destroy(weaponSlot.GetChild(0).gameObject);
    }

    public void AddWeapon(CardDataSO data)
    {
        var item = Instantiate(itemPrefab, weaponSlot);
        item.Init(data);
        item.rectTransform.anchoredPosition = Vector2.zero;
        item.rectTransform.sizeDelta = new Vector2(225, 300);
    }

    public void ClearEquips()
    {
        for(int i = 0; i < equipSlots.Count; i++)
        {
            if (equipSlots[i].childCount > 0)
                Destroy(equipSlots[i].GetChild(0).gameObject);
        }
    }

    public void AddEquip(CardDataSO data, int idx)
    {
        var slot = equipSlots[idx];
        var item = Instantiate(itemPrefab, slot);
        item.Init(data);
        item.rectTransform.anchoredPosition = Vector2.zero;
        item.rectTransform.sizeDelta = new Vector2(225, 300);
    }

    public override void SetList()
    {
        ClearList();
        ClearEquips();
        ClearWeapon();
        var datas = UserInfo.myInfo.handCards;
        foreach (var data in datas)
        {
            var item = AddItem();
            item.Init(data, OnClickItem);
        }
        if(UserInfo.myInfo.weapon != null)
        {
            AddWeapon(UserInfo.myInfo.weapon);
        }
        for (int i = 0; i < UserInfo.myInfo.equips.Count; i++)
        {
            AddEquip(UserInfo.myInfo.equips[i], i);
        }
    }

    public void OnClickItem(CardDataSO data)
    {
        
    }

    private void OnMoveStart()
    {
        select.SetActive(false);
    }

    private void OnMoveEnd()
    {
        select.SetActive(true);
    }

    public void OnClickUse()
    {
        var idx = uiPagingViewController.selectedIdx;
        var card = UserInfo.myInfo.handCards[idx];
        if (card.rcode == "CAD00005")
        {
            UIManager.ShowAlert("누구에게 사용 하시겠습니까?", "119 호출", "나에게", "모두에게", () =>
            {
                UserInfo.myInfo.OnUseCard(idx);
                GameManager.instance.SendSocketUseCard(UserInfo.myInfo, UserInfo.myInfo, card.rcode);
            }, () =>
            {
                UserInfo.myInfo.OnUseCard(idx);
                GameManager.instance.SendSocketUseCard(null, UserInfo.myInfo, card.rcode);
            });
        }
        else
        {
            OnUseCard();
        }
    }

    public void OnUseCard()
    {
        var idx = uiPagingViewController.selectedIdx;
        var card = UserInfo.myInfo.OnUseCard(idx);
        if (card.isTargetSelect || (UserInfo.myInfo.isSniper && card.cardType == CardType.Bbang))
        {
            UIGame.instance.OnSelectDirectTarget(true);
        }
        if (!card.isDirectUse)
        {
            HideDirect();
        }
        else
        {
            GameManager.instance.OnUseCard(card.rcode);
            SetList();
        }
    }
}