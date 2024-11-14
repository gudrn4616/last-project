using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ironcow
{
    public class ICLogger
    {
        public static bool isLog = true;

        public static void Log(object log)
        {
            if (isLog)
                Debug.Log(log);
        }

        public static void LogError(object error)
        {
            if (isLog)
                Debug.LogError(error);
        }

        public static void LogError(object error, UnityEngine.Object obj)
        {
            if (isLog)
                Debug.LogError(error, obj);
        }

        public static void LogException(Exception error)
        {
            if (isLog)
                Debug.LogException(error);
        }

        public static void LogWarning(object warning)
        {
            if (isLog)
                Debug.LogWarning(warning);
        }
    }
}