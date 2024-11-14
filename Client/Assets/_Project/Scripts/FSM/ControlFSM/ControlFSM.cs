using Ironcow;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlFSM : FSM<ControlState>
{
    public ControlFSM(ControlState state) : base(state)
    {

    }
}
