using Ironcow;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterWalkState : CharacterState
{
    float syncFrame = 0;
    public override void OnStateEnter()
    {
        if (anim != null)
        {
            anim.ChangeAnimation("walk");
        }
    }

    public override void OnStateExit()
    {
        anim.ChangeAnimation("idle");
    }

    public override void OnStateUpdate()
    {
        if(anim != null && !anim.IsAnim("walk"))
        {
            anim.ChangeAnimation("walk");
        }
        rigidbody.linearVelocity = character.dir * character.Speed;
        if (SocketManager.instance.isConnected && character.dir != Vector2.zero)
        {
            syncFrame++;
            if (syncFrame > 3)
            {
                GamePacket packet = new GamePacket();
                packet.PositionUpdateRequest = new C2SPositionUpdateRequest() { X = character.transform.position.x, Y = character.transform.position.y };
                SocketManager.instance.Send(packet);
                syncFrame = 0;
            }
        }
    }
}
