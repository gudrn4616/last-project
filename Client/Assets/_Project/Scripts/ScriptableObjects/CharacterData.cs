using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;

[CreateAssetMenu(fileName = "CharacterData", menuName = "ScriptableObjects/CharacterData")]
public class CharacterDataSO : BaseDataSO
{
    public int health;
    public bool isLeft;
}