using Ironcow;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ControlState : BaseState
{

    public abstract void OnClickScreen(RaycastHit2D hit);

}
