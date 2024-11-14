using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using Unity.VisualScripting;
using System;

public class UIGame : UIBase
{
    public static UIGame instance { get => UIManager.Get<UIGame>(); }
    [SerializeField] private TMP_Text shotCount;
    [SerializeField] private UserInfoSlot userInfoSlot;
    [SerializeField] private UserInfoSlot anotherSlotPrefab;
    [SerializeField] private RectTransform userInfoParent;
    [SerializeField] private TMP_Text dayInfo;
    [SerializeField] private TMP_Text time;
    [SerializeField] private GameObject selectCard;
    [SerializeField] private TMP_Text selectCardText;
    [SerializeField] private TMP_Text deckCount;
    [SerializeField] private Button buttonShot;
    [SerializeField] private TMP_Text noticeText;
    [SerializeField] private TMP_Text noticeLogItem;
    [SerializeField] private GameObject noticeLog;
    [SerializeField] private Transform noticeLogParent;
    [SerializeField] public VirtualStick stick;
    [SerializeField] private Image bombButton;
    private float timer = 180;
    Dictionary<string, UserInfoSlot> userslots = new Dictionary<string, UserInfoSlot>();
    private bool isBombTargetSelect = false;
    bool isBombAlert = false;

    public override void Opened(object[] param)
    {
        StartCoroutine(Init());
    }

    public IEnumerator Init()
    {
        yield return new WaitUntil(() => GameManager.instance.isInit);
        for(int i = 0; i < DataManager.instance.users.Count; i++)
        {
            if (DataManager.instance.users[i].id != UserInfo.myInfo.id)
            {
                var item = Instantiate(anotherSlotPrefab, userInfoParent);
                yield return item.Init(DataManager.instance.users[i], i, OnClickCharacterSlot);
                userslots.Add(DataManager.instance.users[i].id, item);
            }
            else
            {
                yield return userInfoSlot.Init(UserInfo.myInfo, i, OnClickCharacterSlot);
                userInfoSlot.SetVisibleRole(true);
            }
        }
        SetShotButton(false);
    }

    public void OnClickCharacterSlot(int idx)
    {
        if (GameManager.instance.SelectedCard == null && !GameManager.instance.isSelectBombTarget) return;
        var card = GameManager.instance.SelectedCard;
        var target = DataManager.instance.users[idx];
        if (card != null && (card.isTargetSelect || (card.rcode == "CAD00001" && UserInfo.myInfo.weapon != null && UserInfo.myInfo.weapon.rcode == "CAD00013")))
        {
            GameManager.instance.SendSocketUseCard(target, UserInfo.myInfo, card.rcode);
            SetSelectCard(null);
        }
        if(GameManager.instance.isSelectBombTarget)
        {
            GameManager.instance.isSelectBombTarget = false;
            if (SocketManager.instance.isConnected)
            {
                GamePacket packet = new GamePacket();
                packet.PassDebuffRequest = new C2SPassDebuffRequest() { DebuffCardType = CardType.Bomb, TargetUserId = target.id };
                SocketManager.instance.Send(packet);
            }
            else
            {
                var targetCard = UserInfo.myInfo.debuffs.Find(obj => obj.rcode == "CAD00023");
                target.debuffs.Add(targetCard);
                UserInfo.myInfo.debuffs.Remove(targetCard);
            }
        }
        UIGame.instance.OnSelectDirectTarget(false);
    }

    public void UpdateUserSlot(List<UserInfo> users)
    {
        for(int i = 0; i < users.Count; i++)
        {
            if (userslots.ContainsKey(users[i].id))
            {
                userslots[users[i].id].UpdateData(users[i]);
            }
            else
            {
                userInfoSlot.UpdateData(users[i]);
            }
        }
        SetDeckCount();
    }

    public void SetShotButton(bool isActive)
    {
        buttonShot.interactable = isActive;
        selectCard.SetActive(isActive);
        shotCount.transform.parent.gameObject.SetActive(isActive);
    }

    public void SetDeckCount()
    {
        deckCount.text = UserInfo.myInfo.handCards.Count.ToString();
    }

    void Update()
    {
        if (!GameManager.instance.isPlaying) return;
        timer -= Time.deltaTime;
        time.text = string.Format("{0:00}:{1:00}", Mathf.Floor(timer / 60), timer % 60);
        if (timer <= 0 && !SocketManager.instance.isConnected) GameManager.instance.OnTimeEnd();
    }

    public override void HideDirect()
    {
        UIManager.Hide<UIGame>();
    }

    public void OnDaySetting(int day, PhaseType phase, long nextAt)
    {
        dayInfo.text = string.Format("Day {0} {1}", day, phase == PhaseType.Day ? "Afternoon" : phase == PhaseType.Evening ? "Evening" : "Night");
        var dt = DateTimeOffset.FromUnixTimeMilliseconds(nextAt) - DateTime.UtcNow;
        timer = (float) dt.TotalSeconds;
        //timer = phase == 1 ? 180 : 60;
    }

    public void OnClickDeck()
    {
        if (!GameManager.instance.userCharacter.IsState<CharacterStopState>() &&
            !GameManager.instance.userCharacter.IsState<CharacterDeathState>())
        {
            UIManager.Show<PopupDeck>();
        }
    }

    public void OnClickBang()
    {
        if (UserInfo.myInfo.isShotPossible || GameManager.instance.SelectedCard.cardType != CardType.Bbang)
            GameManager.instance.OnUseCard();
    }

    public void SetSelectCard(CardDataSO card = null)
    {
        if (card == null)
        {
            if (GameManager.instance.SelectedCard != null)
            {
                if (UserInfo.myInfo.handCards.Find(obj => obj.rcode == GameManager.instance.SelectedCard.rcode) == null ||
                    (GameManager.instance.SelectedCard.cardType == CardType.Bbang && !UserInfo.myInfo.isShotPossible))
                {
                    GameManager.instance.UnselectCard();
                    SetShotButton(false);
                    return;
                }
            }
            return;
        }
        card = UserInfo.myInfo.handCards.Find(obj => obj.rcode == card.rcode);
        if (card == null)
        {
            GameManager.instance.UnselectCard();
            SetShotButton(false);
            return;
        }
        var shotCount = SetShotCount();
        if (shotCount > 0)
        {
            selectCardText.text = card.displayName;
            SetShotButton(true);
        }
        else
        {
            SetShotButton(false);
        }
    }

    public int SetShotCount()
    {
        var card = GameManager.instance.SelectedCard;
        var count = UserInfo.myInfo.handCards.FindAll(obj => obj.rcode == card.rcode).Count;
        shotCount.text = count.ToString();
        if (card.cardType == CardType.Bbang)
        {
            count = Mathf.Min(UserInfo.myInfo.handCards.FindAll(obj => obj.rcode == card.rcode).Count, UserInfo.myInfo.bbangCount - UserInfo.myInfo.shotCount);
            shotCount.text = count.ToString();
        }
        return count;
    }

    public void OnClickNotice()
    {
        noticeLog.SetActive(true);
    }

    public void OnClickCloseNotice()
    {
        noticeLog.SetActive(false);
    }

    public void SetNotice(string notice)
    {
        noticeText.text = notice;
        var item = Instantiate(noticeLogItem, noticeLogParent);
        item.text = notice;
        noticeLogItem.rectTransform.sizeDelta = new Vector2(item.preferredWidth, item.preferredHeight);
    }

    public void SetBombButton(bool isActive)
    {
        bombButton.gameObject.SetActive(isActive);
    }

    public void OnClickBomb()
    {
        if(GameManager.instance.targetCharacter != null && GameManager.instance.targetCharacter.tag == "Bomb")
        {
            GameManager.instance.isSelectBombTarget = true;
            OnSelectDirectTarget(true);
        }
    }

    public void SetBombAlert(bool isAlert)
    {
        if (isAlert)
        {
            StartCoroutine(BombAlert());
        }
        else
        {
            StopCoroutine(BombAlert());
            bombButton.color = Color.white;
        }
    }

    public IEnumerator BombAlert()
    {
        var col = 1f;
        bool isDown = true;
        while (true)
        {
            bombButton.color = new Color(1, col, col);
            yield return null;
            col += 0.1f * (isDown ? -1 : 1);
            if (col <= 0) isDown = true;
            if (col >= 1) isDown = false;
        }
    }

    public void OnSelectDirectTarget(bool isActive)
    {
        foreach (var key in userslots.Keys)
        {
            if (!userslots[key].isDeath)
                userslots[key].SetSelectVisible(isActive);
        }
    }

    public void SetDeath(string id)
    {
        userslots[id].SetDeath();
    }
}