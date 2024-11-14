using System;
using System.Collections.Generic;
using System.Reflection;

namespace Ironcow
{
    public abstract class FSMController<S, F, D> : WorldBase<D> where S : BaseState where F : FSM<S> where D : BaseDataSO
    {
        protected F fsm;
        public Dictionary<string, S> states = new Dictionary<string, S>();
        public S currentState { get => fsm.currentState; }

        protected virtual T ChangeState<T>() where T : S
        {
            if (states.ContainsKey(typeof(T).ToString()))
            {
                fsm.ChangeState((T)states[typeof(T).ToString()]);
            }
            else
            {
                fsm.ChangeState(CreateState<T>());
            }
            return (T)states[typeof(T).ToString()];
        }

        protected T CreateState<T>() where T : S
        {
            var state = Activator.CreateInstance(typeof(T));
            var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.FieldType == GetType() || field.FieldType == GetType().BaseType) field.SetValue(state, this);
            }
            return AddState((T)state);
        }

        protected T AddState<T>(T state) where T : S
        {
            states.Add(typeof(T).ToString(), state);
            return (T)states[typeof(T).ToString()];
        }
    }

}