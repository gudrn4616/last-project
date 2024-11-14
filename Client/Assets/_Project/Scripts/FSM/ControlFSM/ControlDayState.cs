using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlDayState : ControlState
{
    public override void OnClickScreen(RaycastHit2D hit)
    {
        if (hit.collider.TryGetComponent<Character>(out var character))
        {
            if (character == GameManager.instance.userCharacter)
                character.OnVisibleRange();
            else if (Vector3.Distance(character.transform.position, GameManager.instance.userCharacter.transform.position) < 4.5f)
            {
                if (character.characterType != eCharacterType.npc && character.IsState<CharacterStopState>()) return;
                switch(character.tag)
                {/*
                    case "Bank":
                        {
                            if(GameManager.instance.SelectedCard.rcode == "CAD00011")
                            {
                                UserInfo.myInfo.handCards.Remove(GameManager.instance.SelectedCard);
                                GameManager.instance.SendSocketUseCard(null, UserInfo.myInfo, GameManager.instance.SelectedCard.rcode);
                            }
                        }
                        break;
                    case "Lottery":
                        {
                            if (GameManager.instance.SelectedCard.rcode == "CAD00012")
                            {
                                UserInfo.myInfo.handCards.Remove(GameManager.instance.SelectedCard);
                                GameManager.instance.SendSocketUseCard(null, UserInfo.myInfo, GameManager.instance.SelectedCard.rcode);
                            }
                        }
                        break;
                    case "Mission":
                        {
                            
                        }
                        break;*/
                    case "Bomb":
                        {
                            if (UserInfo.myInfo.debuffs.Find(obj => obj.rcode == "CAD00023") != null)
                            {
                                GameManager.instance.OnTargetSelect(character);
                                //UserInfo.myInfo.handCards.Remove(GameManager.instance.SelectedCard);
                                //GameManager.instance.isSelectBombTarget = true;
                                //GameManager.instance.SendSocketUseCard(null, UserInfo.myInfo, GameManager.instance.SelectedCard.rcode);
                            }
                        }
                        break;
                    default:
                        {
                            GameManager.instance.OnTargetSelect(character);
                        }
                        break;
                }
            }
        }
    }

    public override void OnStateEnter()
    {
        
    }

    public override void OnStateExit()
    {
        
    }

    public override void OnStateUpdate()
    {
        
    }
}
