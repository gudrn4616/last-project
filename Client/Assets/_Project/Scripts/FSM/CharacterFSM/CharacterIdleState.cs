using Ironcow;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterIdleState : CharacterState
{
    public override void OnStateEnter()
    {
        rigidbody.linearVelocity = Vector3.zero;
    }

    public override void OnStateExit()
    {
        
    }

    public override void OnStateUpdate()
    {
        
    }
}
