using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Ironcow
{
    /// <summary>
    /// 기본 reponse 모델 클래스 (base response model class)
    /// </summary>
    /// <typeparam name="T">모델로 사용할 클래스 (class for using to model)</typeparam>
    [System.Serializable]
    public class Response<T>
    {
        [System.Serializable]
        public class JWTPayload
        {
            public int midx;
            public int iap;
        }
        //public string error;
#if NEXON_API
        public NexonErrorCheck error;
        public T data;
#else
        public List<T> data = new List<T>();
        public T Data { get => data[0]; }
        public string error;
#endif
        public bool isDone;
        public string callApi;
        public int code;
        public string status;
        public JWTPayload jwt_payload;
        public string text;

        /// <summary>json string을 클래스로 변환 (convert json string to class)</summary>
        public T ParseJson(DownloadHandler handler)
        {
            return JsonUtility.FromJson<T>(handler.text);
        }

        /// <summary>디버그 등을 위해 json형 string으로 변환 (convert response class to json string)</summary>
        public string ToJson() => JsonUtility.ToJson(this);

        /// <summary>
        /// data[0]번째 (기본적으로 배열로 데이터가 넘어오기 때문에, 여러 개를 사용하지 않을 경우 이것을 사용) ( 0th data (data are sent to array basically, so if you don't use array, use this))
        /// </summary>

        [System.Obsolete]
        public virtual void SetDataToUserInfo()
        {

        }
    }

    [System.Serializable]
    public class ResponseJWT
    {
        [System.Serializable]
        public class JWT_Payload
        {
            public int midx;
            public string iat;
            public string exp;
        }

        public string code;
        public string status;
        public string message;
        public int midx;
        public string jwt;
        public JWT_Payload jwt_payload;
    }

    [System.Serializable]
    public class ErrorCheck
    {
        public string error;
        public string message;
    }

    [System.Serializable]
    public class JWTCheck
    {
        public string code;
    }

    [System.Serializable]
    public class NexonErrorCheck
    {
        public string name;
        public string message;
    }
}