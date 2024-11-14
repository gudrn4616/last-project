using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ironcow
{
#if USE_AUTO_CACHING
    public class MonoSingleton<T> : MonoAutoCaching where T : MonoSingleton<T>
#else
    public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
#endif
    {
        private static T _instance;
        public static T instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                    if (_instance != null)
                    {
                        _instance = new GameObject(typeof(T).Name).AddComponent<T>();
                    }
                }
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }

        public static bool isInstance { get => _instance != null; }

        [SerializeField] private bool isDontDestroy;

        protected virtual void Awake()
        {
            instance = (T)this;
            if (isDontDestroy)
                DontDestroyOnLoad(this);
        }
    }
}