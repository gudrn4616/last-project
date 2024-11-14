using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ironcow
{
    public abstract class BaseState
    {
        public abstract void OnStateEnter();
        public abstract void OnStateUpdate();
        public abstract void OnStateExit();

    }
}