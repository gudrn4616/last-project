using Ironcow;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterFSM : FSM<CharacterState>
{
    public CharacterFSM(CharacterState state) : base(state)
    {

    }

}
