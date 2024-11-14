using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ironcow
{
    public class FSM<T> where T : BaseState
    {
        protected T state;
        public T currentState { get => state; }

        public FSM(T state)
        {
            this.state = state;
        }

        public void ChangeState(T nextState)
        {
            if (nextState == state)
                return;

            if (state != null)
                state.OnStateExit();

            state = nextState;
            state.OnStateEnter();
        }

        public void UpdateState()
        {
            if (state != null)
            {
                state.OnStateUpdate();
            }
        }

        public Type GetState()
        {
            return state.GetType();
        }

        public bool IsState<T>()
        {
            return state.GetType() == typeof(T);
        }
    }
}