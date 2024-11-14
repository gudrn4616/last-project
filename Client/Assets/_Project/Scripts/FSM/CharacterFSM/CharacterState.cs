using Ironcow;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterState : BaseState
{
    protected SpriteAnimation anim;
    protected Rigidbody2D rigidbody;
    protected Character character;

    public CharacterState SetElement(SpriteAnimation anim, Rigidbody2D rigidbody, Character character)
    {
        this.anim = anim;
        this.rigidbody = rigidbody;
        this.character = character;
        return this;
    }
}
