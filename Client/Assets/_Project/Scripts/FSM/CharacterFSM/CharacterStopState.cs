using Ironcow;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStopState : CharacterState
{
    public override void OnStateEnter()
    {
        character.stop.SetActive(true);
    }

    public override void OnStateExit()
    {
        character.stop.SetActive(false);
    }

    public override void OnStateUpdate()
    {
        
    }
}
