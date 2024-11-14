using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class Card : UIListItem
{
	[SerializeField] Image thumbnail;
    [SerializeField] TMP_Text cardName;
    [SerializeField] TMP_Text cardDesc;
	[SerializeField] private GameObject select;
    [SerializeField] private GameObject back;

	public CardDataSO cardData;
    UnityAction<int> callback;
    UnityAction<CardDataSO> dataCallback;
    UnityAction<Card> cardCallback;
    public int idx;

    public async void Init(CardDataSO cardData)
    {
        select.gameObject.SetActive(false);
        this.cardData = cardData;
        thumbnail.sprite = await ResourceManager.instance.LoadAsset<Sprite>(cardData.rcode, eAddressableType.Thumbnail);
        cardName.text = cardData.displayName;
        cardDesc.text = cardData.description;
    }

    public async void Init(CardDataSO cardData, UnityAction<CardDataSO> callback)
    {
        select.gameObject.SetActive(false);
        this.cardData = cardData;
        this.dataCallback = callback;
        thumbnail.sprite = await ResourceManager.instance.LoadAsset<Sprite>(cardData.rcode, eAddressableType.Thumbnail);
        cardName.text = cardData.displayName;
        cardDesc.text = cardData.description;
    }

    public void Init(int idx, UnityAction<Card> callback)
    {
        this.idx = idx;
        select.gameObject.SetActive(false);
        this.cardCallback = callback;
        SetCardBack();
    }

    public async void Init(CardDataSO cardData, int idx, UnityAction<int> callback)
    {
        select.gameObject.SetActive(false);
        this.idx = idx;
        this.cardData = cardData;
        this.callback = callback;
        thumbnail.sprite = await ResourceManager.instance.LoadAsset<Sprite>(cardData.rcode, eAddressableType.Thumbnail);
        cardName.text = cardData.displayName;
        cardDesc.text = cardData.description;
    }


    public async void Init(CardDataSO cardData, UnityAction<Card> callback)
    {
        select.gameObject.SetActive(false);
        this.cardData = cardData;
        this.cardCallback = callback;
        thumbnail.sprite = await ResourceManager.instance.LoadAsset<Sprite>(cardData.rcode, eAddressableType.Thumbnail);
        cardName.text = cardData.displayName;
        cardDesc.text = cardData.description;
    }

    public void OnClickCard()
    {
        callback?.Invoke(idx);
        dataCallback?.Invoke(cardData);
        cardCallback?.Invoke(this);
	}

    public void OnSelect(bool isSelect)
    {
        select.gameObject.SetActive(isSelect);
    }

    public void SetCardBack(bool isBack = true)
    {
        back.SetActive(isBack);
    }
}