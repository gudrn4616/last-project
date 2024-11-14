using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;

[CreateAssetMenu(fileName = "CardData", menuName = "ScriptableObjects/CardData")]
public class CardDataSO : BaseDataSO
{
    public eCardType type;
    public bool isUsable;
    public string defCard;
    public bool isDirectUse;
    public bool isTargetSelect;
    public bool isTargetCardSelection;
    public string useTag;

    public bool isMarketSelected;
    public CardType cardType { get => (CardType)int.Parse(rcode.Substring(3, 5)); }
}