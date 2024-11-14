using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using System.Threading.Tasks;
using System;
using UnityEngine.TextCore.Text;
using Unity.Cinemachine;

public class GameManager : MonoSingleton<GameManager>
{
    public bool isInit;
    public Camera mainCamera;
    public Character userCharacter;
    public Character targetCharacter;
    private CardDataSO selectedCard;
    public CardDataSO SelectedCard
    {
        get => selectedCard;
        set
        {
            selectedCard = value;
            UIGame.instance.SetSelectCard(value);
        }
    }
    public Dictionary<string, Character> characters = new Dictionary<string, Character>();
    [SerializeField] private GameObject cover;
    [SerializeField] private GameObject deco;
    [SerializeField] public CinemachineCamera virtualCamera;
    [SerializeField] private TilemapRenderer tilemapRenderer;
    [SerializeField] private Controller controller;
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private CinemachineTargetGroup targetGroup;

    private Queue<CardDataSO> worldDeck = new Queue<CardDataSO>();

    private int day = 0;
    private bool isAfternoon = true;
    public bool isPlaying;

    public List<CardDataSO> pleaMarketCards = new List<CardDataSO>();
    List<Transform> spawns;
    public bool isSelectBombTarget = false;

    private void Start()
    {
        if (!SocketManager.instance.isConnected) Init();
        if(spawnPoints != null)
            spawns = new List<Transform>(spawnPoints);
    }

    public async void Init()
    {
        // 카드 덱을 먼저 구성
        var deckDatas = DataManager.instance.GetDatas<DeckData>();
        var cards = new List<CardDataSO>();
        foreach (var deckData in deckDatas)
        {
            for (int i = 0; i < deckData.count; i++)
            {
                cards.Add(DataManager.instance.GetData<CardDataSO>(deckData.targetRcode).Clone());
            }
        }
        worldDeck = new Queue<CardDataSO>(cards.Shuffle());

        //유저 캐릭터 세팅
        var bounds = tilemapRenderer.bounds;
        var myIndex = DataManager.instance.users.FindIndex(obj => obj == UserInfo.myInfo); 
        spawns = new List<Transform>(spawnPoints);
        for (int i = 0; i < DataManager.instance.users.Count; i++)
        {
            var userinfo = DataManager.instance.users[i];
            var chara = await AddCharacter(userinfo.selectedCharacterRcode, userinfo == UserInfo.myInfo ? eCharacterType.playable : eCharacterType.non_playable, userinfo.id);
            chara.transform.position = spawns.RandomPeek().position; //new Vector3(Util.Random(bounds.min.x, bounds.max.x), Util.Random(bounds.min.y, bounds.max.y));
            chara.OnChangeState<CharacterStopState>();
            if (userinfo.roleType == eRoleType.target)
                chara.SetTargetMark();
            chara.OnVisibleMinimapIcon(Util.GetDistance(myIndex, i, DataManager.instance.users.Count) + userinfo.slotFar <= UserInfo.myInfo.slotRange && myIndex != i); // 가능한 거리에 있는 유저 아이콘만 표시
            chara.userInfo = userinfo;
            var data = DataManager.instance.GetData<CharacterDataSO>(userinfo.selectedCharacterRcode);
            userinfo.maxHp = data.health + (userinfo.roleType == eRoleType.target ? 1 : 0);
            for(int j = 0; j < userinfo.hp; j++)
            {
                OnDrawCard(userinfo);
            }
        }
        if (!SocketManager.instance.isConnected)
        {
            foreach (var user in DataManager.instance.users)
            {
                OnDrawCard(user);
                OnDrawCard(user);
            }
        }
        OnGameStart();
        isInit = true;
    }

    public void SetGameState(GameStateData gameStateData)
    {
        SetGameState(gameStateData.PhaseType, gameStateData.NextPhaseAt);
    }

    public async void SetGameState(PhaseType PhaseType, long NextPhaseAt)
    { 
        if (PhaseType == PhaseType.Day)
        {
            UserInfo.myInfo.OnDayOfAfter();
            day++;
        }

        foreach (var key in characters.Keys)
        {
            if (PhaseType == PhaseType.Day)
                characters[key].OnChangeState<CharacterIdleState>();
            else
                characters[key].OnChangeState<CharacterStopState>();
        }

        isAfternoon = PhaseType == PhaseType.Day;
        UIManager.Get<UIGame>().OnDaySetting(day, PhaseType, NextPhaseAt);

        if (PhaseType == PhaseType.End)
        {
            if(UserInfo.myInfo.handCards.Count > UserInfo.myInfo.hp)
                UIManager.Show<PopupRemoveCardSelection>();
        }
        else
        {
            UIManager.Hide<PopupRemoveCardSelection>();
        }
        
        isPlaying = true;
        UIGame.instance.SetDeckCount();
    }

    public async void OnTimeEnd()
    {
        spawns = new List<Transform>(spawnPoints);
        if (!isAfternoon)
        {
            day++;
            if (!SocketManager.instance.isConnected)
            {
                foreach (var user in DataManager.instance.users)
                {
                    OnDrawCard(user);
                    OnDrawCard(user);
                }
            }
        }
        isAfternoon = !isAfternoon;
        //controller.currentState
        //UIManager.Get<UIGame>().OnDaySetting(day, isAfternoon);
        await Task.Delay(1000);
        isPlaying = true;
        UIGame.instance.SetDeckCount();
    }

    public async Task OnCreateCharacter(UserInfo userinfo, int idx)
    {
        var myIndex = DataManager.instance.users.FindIndex(obj => obj == UserInfo.myInfo);
        var chara = await AddCharacter(userinfo.selectedCharacterRcode, userinfo.id == UserInfo.myInfo.id ? eCharacterType.playable : eCharacterType.non_playable, userinfo.id);
        //chara.transform.position = spawns.RandomPeek().position; //new Vector3(Util.Random(bounds.min.x, bounds.max.x), Util.Random(bounds.min.y, bounds.max.y));
        chara.OnChangeState<CharacterStopState>();
        if (userinfo.roleType == eRoleType.target)
            chara.SetTargetMark();
        chara.OnVisibleMinimapIcon(Util.GetDistance(myIndex, idx, DataManager.instance.users.Count) <= UserInfo.myInfo.slotRange && myIndex != idx); // 가능한 거리에 있는 유저 아이콘만 표시
        chara.userInfo = userinfo;
    }

    public void OnDrawCard(UserInfo user)
    {
        user.AddHandCard(worldDeck.Dequeue());
    }

    public CardDataSO OnDrawCard()
    {
        return worldDeck.Dequeue();
    }

    public void SetPleaMarketCards()
    {
        for (int i = 0; i < DataManager.instance.users.Count; i++)
        {
            pleaMarketCards.Add(worldDeck.Dequeue());
        }
    }

    public void TrashCard(CardDataSO card)
    {
        worldDeck.Enqueue(card);
    }

    public async void OnGameStart()
    {
        //UIManager.Get<UIGame>().OnDaySetting(day, 1, );
        //await Task.Delay(1000);
        foreach (var chara in characters.Values)
        {
            chara.OnChangeState<CharacterIdleState>();
        }
        isPlaying = true;
        UIGame.instance.SetDeckCount();
    }

    public async Task<Character> AddCharacter(string rcode, eCharacterType characterType, string id)
    {
        var character = Instantiate(await ResourceManager.instance.LoadAsset<Character>("Character", eAddressableType.Prefabs));
        character.name = rcode;
        character.Init(DataManager.instance.GetData<CharacterDataSO>(rcode));
        character.SetCharacterType(characterType);
        characters.Add(id, character);
        if (characterType == eCharacterType.playable)
        {
            userCharacter = character;
            virtualCamera.Target.TrackingTarget = userCharacter.transform;
            //virtualCamera.target
            //virtualCamera.Follow = userCharacter.transform;
            //virtualCamera.LookAt = userCharacter.transform;
        }
        return character;
    }

    public void RemoveCharacter(string id)
    {
        characters.Remove(id);
    }

    public void SetMapInside(bool isInside)
    {
        cover.SetActive(!isInside);
        deco.SetActive(!isInside);
    }

    public void OnTargetSelect(Character character)
    {
        if (targetCharacter == character)
        {
            character.OnSelect();
            targetCharacter = null;
        }
        else
        {
            if (targetCharacter != null)
                targetCharacter.OnSelect();
            targetCharacter = character;
            character.OnSelect();
        }
    }

    public void OnUseCard(string rcode = "", UserInfo target = null)
    {
        if (!string.IsNullOrEmpty(rcode))
        {
            SendSocketUseCard(target == null ? UserInfo.myInfo : target, UserInfo.myInfo, rcode);
        }
        else if(targetCharacter != null && SelectedCard != null)
        {
            UserInfo.myInfo.handCards.Remove(SelectedCard);
            SendSocketUseCard(targetCharacter.userInfo, UserInfo.myInfo, SelectedCard.rcode);
        }
    }

    public void SendSocketUseCard(UserInfo userinfo, UserInfo useUserInfo,  string rcode)
    {
        var card = DataManager.instance.GetData<CardDataSO>(rcode);
        if (!string.IsNullOrEmpty(card.useTag) && card.useTag != targetCharacter.tag) return;
        if (SocketManager.instance.isConnected)
        {
            var cardIdx = useUserInfo.handCards.FindIndex(obj => obj.rcode == rcode);
            GamePacket packet = new GamePacket();
            //packet.UseCardRequest = new C2SUseCardRequest() { CardType = cardIdx, TargetUserId = userinfo == null ? "" : userinfo.id };
            packet.UseCardRequest = new C2SUseCardRequest() { CardType = card.cardType, TargetUserId = userinfo == null ? "" : userinfo.id };
            SocketManager.instance.Send(packet);
        }
        else
        {
            switch (rcode)
            {
                case "CAD00001":
                    {
                        if (userinfo.id == UserInfo.myInfo.id)
                        {
                            UIManager.Show<PopupBattle>(rcode, useUserInfo.id);
                        }
                        else
                        {
                            var defCard = userinfo.handCards.Find(obj => obj.rcode == card.defCard);
                            if (defCard != null)
                            {
                                userinfo.handCards.Remove(defCard);
                            }
                            else
                            {
                                userinfo.hp--;
                            }
                        }
                    }
                    break;
                case "CAD00002":
                case "CAD00007":
                    {
                        foreach (var user in DataManager.instance.users)
                        {
                            if (user.id == userinfo.id) continue;
                            if (user.id == UserInfo.myInfo.id)
                            {
                                UIManager.Show<PopupBattle>(rcode, useUserInfo.id);
                            }
                            else
                            {
                                var defCard = user.handCards.Find(obj => obj.rcode == card.defCard);
                                if (defCard != null)
                                {
                                    user.handCards.Remove(defCard);
                                }
                                else
                                {
                                    user.hp--;
                                }
                            }
                        }
                    }
                    break;
                case "CAD00004":
                    {
                        userinfo.hp = Mathf.Min(userinfo.maxHp, userinfo.hp + 1);
                    }
                    break;
                case "CAD00005":
                    {
                        if (userinfo == UserInfo.myInfo)
                        {
                            userinfo.hp = Mathf.Min(userinfo.maxHp, userinfo.hp + 1);
                        }
                        else
                        {
                            foreach (var user in DataManager.instance.users)
                            {
                                if (user.id == userinfo.id) continue;
                                user.hp = Mathf.Min(user.maxHp, user.hp + 1);
                            }
                        }
                    }
                    break;
                case "CAD00006":
                    {
                        if (userinfo == UserInfo.myInfo)
                        {
                            userinfo.hp = Mathf.Min(userinfo.maxHp, userinfo.hp + 1);
                        }
                        else
                        {
                            var usecard = DataManager.instance.GetData<CardDataSO>(rcode);
                            var defCard = useUserInfo.handCards.Find(obj => obj.rcode == card.defCard);
                            if (defCard != null)
                            {
                                useUserInfo.OnUseCard(defCard);
                                UIManager.Get<PopupBattle>().AddUseCard(defCard);
                            }
                        }
                    }
                    break;
                case "CAD00008":
                    {
                        UIManager.Show<PopupCardSelection>(userinfo, rcode);
                    }
                    break;
                case "CAD00009":
                    {
                        UIManager.Show<PopupCardSelection>(userinfo, rcode);
                    }
                    break;
                case "CAD00010":
                    {
                        UIManager.Show<PopupPleaMarket>(userinfo.id);
                    }
                    break;
                case "CAD00011":
                    {

                    }
                    break;
                case "CAD00012":
                    {

                    }
                    break;
                case "CAD00021":
                    {

                    }
                    break;
                case "CAD00022":
                    {

                    }
                    break;
                case "CAD00023":
                    {

                    }
                    break;
            }
            OnUseCardResult(userinfo, rcode);
        }
    }

    public async void OnSelectCard(UserInfo userinfo, string rcode, UserInfo useUserInfo, string actionRcode)
    {
        switch(actionRcode)
        {
            case "CAD00001":
            case "CAD00002":
            case "CAD00007":
                {
                    if (string.IsNullOrEmpty(rcode))
                    {
                        userinfo.hp--;
                    }
                }
                break;
            case "CAD00006":
                {
                    if (string.IsNullOrEmpty(rcode))
                    {
                        userinfo.hp--;
                    }
                    else
                    {
                        var card = DataManager.instance.GetData<CardDataSO>(actionRcode);
                        var defCard = useUserInfo.handCards.Find(obj => obj.rcode == card.defCard);
                        if(defCard != null)
                        {
                            useUserInfo.OnUseCard(defCard);
                            UIManager.Get<PopupBattle>().AddUseCard(defCard);
                        }
                    }
                }
                break;
            case "CAD00008":
                {
                    var card = userinfo.handCards.Find(obj => obj.rcode == rcode);
                    if (card != null)
                    {
                        userinfo.handCards.Remove(card);
                    }
                    else
                    {
                        card = userinfo.weapon.rcode == rcode ? userinfo.weapon : null;
                        if (card != null)
                            userinfo.weapon = null;
                    }
                    if (card == null)
                    {
                        card = userinfo.equips.Find(obj => obj.rcode == rcode);
                        if (card != null)
                            userinfo.equips.Remove(card);
                    }
                    if (card == null)
                    {
                        card = userinfo.debuffs.Find(obj => obj.rcode == rcode);
                        if (card != null)
                            userinfo.debuffs.Remove(card);
                    }
                    if(card != null)
                    {
                        useUserInfo.handCards.Add(card);
                    }
                }
                break;
            case "CAD00009":
                {
                    var card = userinfo.handCards.Find(obj => obj.rcode == rcode);
                    if (card != null)
                    {
                        userinfo.handCards.Remove(card);
                    }
                    else
                    {
                        card = userinfo.weapon.rcode == rcode ? userinfo.weapon : null;
                        if (card != null)
                            userinfo.weapon = null;
                    }
                    if (card == null)
                    {
                        card = userinfo.equips.Find(obj => obj.rcode == rcode);
                        if (card != null)
                            userinfo.equips.Remove(card);
                    }
                    if (card == null)
                    {
                        card = userinfo.debuffs.Find(obj => obj.rcode == rcode);
                        if (card != null)
                            userinfo.debuffs.Remove(card);
                    }
                }
                break;
            case "CAD00010":
                {
                    var card = pleaMarketCards.Find(obj => obj.rcode == rcode);
                    userinfo.AddHandCard(card);
                    var index = DataManager.instance.users.IndexOf(useUserInfo);
                    var next = index;
                    var count = pleaMarketCards.FindAll(obj => !obj.isMarketSelected).Count;
                    for (int i = 0; i < count; i++)
                    {
                        next = Util.Next(next + 1, 0, DataManager.instance.users.Count);
                        card = pleaMarketCards.FindAll(obj => !obj.isMarketSelected).RandomValue();
                        DataManager.instance.users[next].AddHandCard(card);
                        UIManager.Get<PopupPleaMarket>().SetNextUserTurn(DataManager.instance.users[next], pleaMarketCards.IndexOf(card));
                    }
                    await Task.Delay(2000);
                    UIManager.Hide<PopupPleaMarket>();
                }
                break;
        }
    }

    public void OnUseCardResult(UserInfo userInfo, string rcode)
    {
        SelectedCard = null;
        if (userInfo.id != UserInfo.myInfo.id)
        {
            if (rcode == "CAD00003") UIManager.Hide<PopupBattle>();
        }
    }

    public void OnGameEnd()
    {
        isPlaying = false;
        if(!SocketManager.instance.isConnected)
            UIManager.Show<PopupResult>(DataManager.instance.users.Find(obj => obj.hp > 0));
    }

    public void UnselectCard()
    {
        selectedCard = null;
    }

}