using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;

[CreateAssetMenu(fileName = "DeckData", menuName = "ScriptableObjects/DeckData")]
public class DeckData : BaseDataSO
{
    public string targetRcode;
    public int count;
}