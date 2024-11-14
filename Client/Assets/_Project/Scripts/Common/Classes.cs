

using Google.Protobuf.Collections;
using Ironcow;
using System.Collections.Generic;

public partial class UserInfo
{
    public string id;
    public string nickname { get => id; set => id = value; }
    public int coin;
    public string selectedCharacterRcode { get => characterData.GetRcode(); set => characterData.CharacterType = (CharacterType)int.Parse(value.Substring(3, 5)); }
    public eRoleType roleType { get => (eRoleType)characterData.RoleType; set => characterData.RoleType = (RoleType)value; }
    public CharacterData characterData = new CharacterData();
    public List<CardDataSO> handCards = new List<CardDataSO>();
    public CardDataSO weapon;
    public List<CardDataSO> equips = new List<CardDataSO>();
    public List<CardDataSO> debuffs = new List<CardDataSO>();
    public int handcardCount { get => characterData.HandCardsCount; }
    public int hp { get => characterData.Hp; set => characterData.Hp = value; }
    public int maxHp;

    public bool isStelth { get => equips.Find(obj => obj.rcode == "CAD00020") != null; }
    public bool isRaider { get => equips.Find(obj => obj.rcode == "CAD00018") != null; }
    public int slotRange { get => 1 + (isRaider ? 1 : 0) + (selectedCharacterRcode == "CHA00009" ? 1 : 0); }
    public int slotFar { get => (isStelth ? 1 : 0) + (selectedCharacterRcode == "CHA00012" ? 1 : 0); }

    public bool isMultiShotCharacter { get => selectedCharacterRcode == "CHA00003" ? true : false; }

    public int needShieldCount { get => (equips.Find(obj => obj.rcode == "CAD00017") == null ? 1 : 2) + (isMultiShotCharacter ? 1 : 0); }

    public bool isSniper { get => weapon != null && weapon.rcode == "CAD00013"; }

    public int bbangCount { get => (weapon != null && weapon.rcode == "CAD00016") || characterData.CharacterType == CharacterType.Red ? 99 : ((weapon != null && weapon.rcode == "CAD00014") ? 2 : 1); }

    public bool isPowerShot { get => weapon != null && weapon.rcode == "CAD00015"; }

    public int shotCount { get => characterData.BbangCount; }

    public bool isShotPossible { get => shotCount < bbangCount; }

    public int index { get => DataManager.instance.users.FindIndex(obj => obj.id == id); }

    static List<string> firstName = new List<string>() { "발빠른", "신중한", "개구진", "멋쟁이", "귀여운", "핸섬한", "맛있는", "재밌는" };
    static List<string> lastName = new List<string>() { "제이지", "케토피", "피카츄", "마자용", "모래성데스", "라이언", "상어군", "르탄이", "거북손이당" };

    public UserInfo()
    {

    }

    public UserInfo(UserData userData)
    {
        this.id = userData.Id;
        //this.nickname = userData.Nickname;
        characterData = userData.Character;
        if (characterData != null)
        {
            this.maxHp = userData.Character.Hp;
            foreach (var card in userData.Character.HandCards)
            {
                for (int i = 0; i < card.Count; i++)
                {
                    if(card.Type != CardType.None)
                        handCards.Add(card.GetCardData());
                }
            }
            if (userData.Character.Weapon > 0)
            {
                weapon = DataManager.instance.GetData<CardDataSO>(string.Format("CAD{0:00000}", userData.Character.Weapon));
            }
            foreach (var card in userData.Character.Equips)
            {
                equips.Add(DataManager.instance.GetData<CardDataSO>(string.Format("CAD{0:00000}", card)));
            }
            foreach (var card in userData.Character.Debuffs)
            {
                debuffs.Add(DataManager.instance.GetData<CardDataSO>(string.Format("CAD{0:00000}", card)));
            }
        }
    }

    public UserData ToUserData()
    {
        var userData = new UserData();
        userData.Id = this.id;
        //userData.Nickname = this.nickname;
        return userData;
    }

    public void OnDayOfAfter()
    {
        //shotCount = 0;
    }

    public void UpdateUserInfo(UserData userData)
    {
        characterData = userData.Character;
        if (characterData != null)
        {
            handCards.Clear();
            equips.Clear();
            debuffs.Clear();
            foreach (var card in userData.Character.HandCards)
            {
                for (int i = 0; i < card.Count; i++)
                {
                    if (card.Type != CardType.None)
                        handCards.Add(card.GetCardData());
                }
            }
            if (userData.Character.Weapon > 0)
            {
                weapon = DataManager.instance.GetData<CardDataSO>(string.Format("CAD{0:00000}", userData.Character.Weapon));
            }
            foreach (var card in userData.Character.Equips)
            {
                equips.Add(DataManager.instance.GetData<CardDataSO>(string.Format("CAD{0:00000}", card)));
            }
            foreach (var card in userData.Character.Debuffs)
            {
                debuffs.Add(DataManager.instance.GetData<CardDataSO>(string.Format("CAD{0:00000}", card)));
            }
        }
    }

    public void UpdateHandCard(RepeatedField<CardData> handCards)
    {
        this.handCards.Clear();
        foreach (var card in handCards)
        {
            for (int i = 0; i < card.Count; i++)
            {
                this.handCards.Add(card.GetCardData());
            }
        }
    }

    public void Clear()
    {
        handCards.Clear();
        equips.Clear();
        debuffs.Clear();
        weapon = null;
        characterData = null;
    }

    public static UserInfo CreateRandomUser()
    {
        var userinfo = new UserInfo();
        userinfo.nickname = firstName.RandomValue() +" " + lastName.RandomValue();
        userinfo.id = "user_" + Util.Random(10000, 99999).ToString();

        return userinfo;
    }

    public void AddHandCard(CardDataSO card)
    {
        handCards.Add(card);
    }

    public CardDataSO OnUseCard(int idx)
    {
        return OnUseCard(handCards[idx]);
    }

    public CardDataSO OnUseCard(string rcode)
    {
        return OnUseCard(handCards.Find(obj => obj.rcode == rcode));
    }

    public CardDataSO OnUseCard(CardDataSO card)
    {
        switch(card.type)
        {
            case eCardType.active:
                {
                    if (!card.isDirectUse)
                        GameManager.instance.SelectedCard = card;
                    else
                    {
                        handCards.Remove(card);
                        GameManager.instance.TrashCard(card);
                    }
                }
                break;
            case eCardType.weapon:
                {
                    if (weapon != null)
                    {
                        GameManager.instance.TrashCard(weapon);
                    }
                    weapon = card;
                    handCards.Remove(card);
                    UIGame.instance.SetDeckCount();
                }
                break;
            case eCardType.equip:
                {
                    var idx = equips.FindIndex(obj => obj.rcode == card.rcode);
                    if (idx >= 0)
                    {
                        var old = equips[idx];
                        GameManager.instance.TrashCard(old);
                        equips.Remove(old);
                    }
                    equips.Add(card);
                    handCards.Remove(card);
                    UIGame.instance.SetDeckCount();
                }
                break;
            case eCardType.debuff:
                {
                    debuffs.Add(card);
                    if (!card.isDirectUse)
                        GameManager.instance.SelectedCard = card;
                    else
                    {
                        handCards.Remove(card);
                        GameManager.instance.TrashCard(card);
                    }
                }
                break;
        }
        return card;
    }
}