using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using System.Threading.Tasks;
#if USE_CLOUD_CODE
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.CloudCode.GeneratedBindings;
#endif

namespace Ironcow
{
    public class NetworkManager : MonoSingleton<NetworkManager>
    {
        public bool isInit = false;
#if USE_CLOUD_CODE
        IroncowModuleBindings module;
#endif
        #region URL
        [SerializeField] string API_BASE_URL = "http://218.38.65.83";
        [SerializeField] string API_BASE_PORT = "";
        [SerializeField] string DOWNLOAD_BASE_URL = "http://ironcow.synology.me/resources/";
#if NEXON_API
        [SerializeField] string NEXON_API_KEY;
#endif
        public string GetApiBaseUrl => string.Format("{0}{1}/", API_BASE_URL, string.IsNullOrEmpty(API_BASE_PORT) ? "" : string.Format(":{0}", API_BASE_PORT));

        #endregion

        #region Nexon Api
#if NEXON_API
        /// <summary>GET/DELETE 요청 비동기로 실행</summary>
        public async Task<Response<T>> requestAPIAsync<T>(string api, string param, eRequestType requestType)
        {
            string api_url = API_BASE_URL + (API_BASE_PORT.Length == 0 ? "" : ":" + API_BASE_PORT);
            string url = api_url + api + param;
            //Debug.Log(url);

            UnityWebRequest response = UnityWebRequest.Get(url);
            if (requestType == eRequestType.DELETE)
            {
                response = UnityWebRequest.Delete(url);
                response.downloadHandler = new DownloadHandlerBuffer();
            }
            if (!api.ToLower().Contains("jwt") && !api.ToLower().Contains("meta"))
                response.SetRequestHeader("X-Token", "Bearer " + StorageManager.JWT);
            if (!string.IsNullOrEmpty(NEXON_API_KEY))
                response.SetRequestHeader("x-nxopen-api-key", NEXON_API_KEY);

            UIManager.ShowIndicator();

            await response.SendWebRequest();

            UIManager.HideIndicator();
            try
            {

                ICLogger.Log("Request API : " + api + " Request Type : " + requestType + "\nResponse : " + response.downloadHandler.text);
                var data = JsonUtility.FromJson<Response<T>>(response.downloadHandler.text);
                if (data.error.message == null)
                {
                    data.data = JsonUtility.FromJson<T>(response.downloadHandler.text);
                    response.Dispose();
                    return data;
                }
                else
                {
                    Debug.LogError(data.error.name + "\n" + data.error.message);
                }
            }
            catch (Exception e)
            {
                Debug.Log(response.error);
            }
            return null;
        }

        /// <summary>
        /// GET / DELETE 요청하기 (request GET/DELETE)
        /// </summary>
        /// <typeparam name="T">response로 받을 타입 (type to response)</typeparam>
        /// <param name="api">api url</param>
        /// <param name="param">쿼리로 붙일 패러미터 (parameter for attach to query)</param>
        /// <param name="requestType">요청할 타입 (ex. GET / DELETE)</param>
        /// <param name="callback">요청 후 실행할 콜백 (callback function for executing after request complete)</param>
        /// <param name="errorCallback">에러 발생 시 실행할 콜백 (callback function for executing when error occured)</param>
        public void requestAPI<T>(string api, string param, eRequestType requestType, UnityAction<Response<T>> callback, UnityAction<string> errorCallback = null)
        {
            StartCoroutine(requestGet<T>(api, param, requestType, callback, errorCallback));
        }

        /// <summary>GET/DELETE 요청 실행</summary>
        protected IEnumerator requestGet<T>(string api, string param, eRequestType requestType, UnityAction<Response<T>> callback, UnityAction<string> errorCallback = null)
        {
            string api_url = API_BASE_URL + (API_BASE_PORT.Length == 0 ? "" : ":" + API_BASE_PORT);
            string url = api_url + api + param;
            Debug.Log(string.Format("totalUrl = {3} \napi_url : {0} , api : {1}, param = {2}", api_url, api, param, url));

            var response = UnityWebRequest.Get(url);

            //DELETE 요청일 경우 처리
            if (requestType == eRequestType.DELETE)
            {
                response = UnityWebRequest.Delete(url);
                response.downloadHandler = new DownloadHandlerBuffer();
            }
            if (!api_url.ToLower().Contains("jwt") && !api_url.ToLower().Contains("meta"))
                response.SetRequestHeader("X-Token", "Bearer " + StorageManager.JWT);
            if (!string.IsNullOrEmpty(NEXON_API_KEY))
                response.SetRequestHeader("x-nxopen-api-key", NEXON_API_KEY);

            UIManager.ShowIndicator();
            yield return response.SendWebRequest();
            UIManager.HideIndicator();

            try
            {
                var data = JsonUtility.FromJson<Response<T>>(response.downloadHandler.text);
                if (data.error.message == null)
                {
                    data.data = JsonUtility.FromJson<T>(response.downloadHandler.text);
                    data.text = response.downloadHandler.text;
                    response.Dispose();
                    callback.Invoke(data);
                }
                else
                {
                    Debug.LogError(data.error.name + "\n" + data.error.message);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e + "\n" + response.error);
            }
        }

#endif
#endregion

        #region Ironcow
#if !NEXON_API

        /// <summary>
        /// GET / DELETE 요청하기 (request GET/DELETE)
        /// </summary>
        /// <typeparam name="T">response로 받을 타입 (type to response)</typeparam>
        /// <param name="api">api url</param>
        /// <param name="param">쿼리로 붙일 패러미터 (parameter for attach to query)</param>
        /// <param name="requestType">요청할 타입 (ex. GET / DELETE)</param>
        /// <param name="callback">요청 후 실행할 콜백 (callback function for executing after request complete)</param>
        /// <param name="errorCallback">에러 발생 시 실행할 콜백 (callback function for executing when error occured)</param>
        public void requestAPI<T>(string api, string param, eRequestType requestType, UnityAction<Response<T>> callback, UnityAction<string> errorCallback = null)
        {
            StartCoroutine(requestGet<T>(api, param, requestType, callback, errorCallback));
        }

        /// <summary>GET/DELETE 요청 비동기로 실행</summary>
        public async Task<Response<T>> requestAPIAsync<T>(string api, string param, eRequestType requestType)
        {
            string api_url = API_BASE_URL + (API_BASE_PORT.Length == 0 ? "" : ":" + API_BASE_PORT);
            string url = api_url + api + param;
            //Debug.Log(url);

            UnityWebRequest response = UnityWebRequest.Get(url);
            if (requestType == eRequestType.DELETE)
            {
                response = UnityWebRequest.Delete(url);
                response.downloadHandler = new DownloadHandlerBuffer();
            }
            if (!api.ToLower().Contains("jwt") && !api.ToLower().Contains("meta"))
                response.SetRequestHeader("X-Token", "Bearer " + StorageManager.JWT);

            //비동기는 보통 백그라운드 처리라 로딩창 안띄우는 게 맞는듯
            UIManager.ShowIndicator();

            var asyncOperation = response.SendWebRequest();
            
            await response.SendWebRequest();

            UIManager.HideIndicator();
            try
            {
                ICLogger.Log("Request API : " + api + " Request Type : " + requestType + "\nResponse : " + response.downloadHandler.text);
                var data = JsonUtility.FromJson<Response<T>>(response.downloadHandler.text);
                if (data.error == null)
                {
                    response.Dispose();
                    return data;
                }
                else
                {
                    Debug.LogError(data.error);
                }
            }
            catch (Exception e)
            {
                Debug.Log(response.error);
            }
            return null;
        }

        /// <summary>GET/DELETE 요청 실행</summary>
        protected IEnumerator requestGet<T>(string api, string param, eRequestType requestType, UnityAction<Response<T>> callback, UnityAction<string> errorCallback = null)
        {
            string api_url = API_BASE_URL + (API_BASE_PORT.Length == 0 ? "" : ":" + API_BASE_PORT);
            string url = api_url + api + param;
            Debug.Log(string.Format("totalUrl = {3} \napi_url : {0} , api : {1}, param = {2}", api_url, api, param, url));

            UnityWebRequest response = UnityWebRequest.Get(url);

            //DELETE 요청일 경우 처리
            if (requestType == eRequestType.DELETE)
            {
                response = UnityWebRequest.Delete(url);
                response.downloadHandler = new DownloadHandlerBuffer();
            }
            if (!api_url.ToLower().Contains("jwt") && !api_url.ToLower().Contains("meta"))
                response.SetRequestHeader("X-Token", "Bearer " + StorageManager.JWT);

            UIManager.ShowIndicator();
            yield return response.SendWebRequest();
            UIManager.HideIndicator();

            try
            {
                ICLogger.Log("Request API : " + api + " Request Type : " + requestType + "\nResponse : " + response.downloadHandler.text);
                var data = JsonUtility.FromJson<Response<T>>(response.downloadHandler.text);
                if (data.error == null)
                {
                    response.Dispose();
                    callback.Invoke(data);
                }
                else
                {
                    Debug.LogError(data.error);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e + "\n" + response.error);
            }
        }

#endif
#endregion

        #region Post
        
        /// <summary>POST 요청 비동기로 실행</summary>
        public async Task<Response<T>> requestAPIAsync<T>(string api, WWWForm form, eRequestType requestType)
        {
            string api_url = API_BASE_URL + (API_BASE_PORT.Length == 0 ? "" : ":" + API_BASE_PORT) + api;
            UnityWebRequest response = UnityWebRequest.Post(api_url, form);
            response.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            if (!api_url.ToLower().Contains("jwt") && !api_url.ToLower().Contains("meta"))
                response.SetRequestHeader("X-Token", "Bearer " + StorageManager.JWT);
#if NEXON_API
            if (!string.IsNullOrEmpty(NEXON_API_KEY))
                response.SetRequestHeader("x-nxopen-api-key", NEXON_API_KEY);
#endif

            //비동기는 보통 백그라운드 처리라 로딩창 안띄우는 게 맞는듯
            UIManager.ShowIndicator();

            await response.SendWebRequest();

            UIManager.HideIndicator();
            try
            {
                ICLogger.Log("Request API : " + api + ", params : " + UTF8Encoding.UTF8.GetString(form.data) + "\nResponse : " + response.downloadHandler.text);
                var data = JsonUtility.FromJson<Response<T>>(response.downloadHandler.text);
                if (data.error.Equals("E0000"))
                {
                    response.Dispose();
                    return data;
                }
                else
                {
                    Debug.LogError(data.error);
                }
            }
            catch (Exception e)
            {
                Debug.Log(response.error);
            }
            return null;
        }

        /// <summary>
        /// POST 요청하기 (request POST)
        /// </summary>
        /// <typeparam name="T">response로 받을 타입 (type to response)</typeparam>
        /// <param name="api">api url</param>
        /// <param name="form"></param>
        /// <param name="callback">요청 후 실행할 콜백 (callback function for executing after request complete)</param>
        /// <param name="errorCallback">에러 발생 시 실행할 콜백 (callback function for executing when error occured)</param>
        /// <param name="isRawLink">사용중인 호스트 주소가 아닌 다른 주소라던지, 규칙에 예외 발생시 주소 전체가 필요할 때 사용. (ex. http://www.naver.com/api/1")
        /// (if you need to use other url rule (like host url is not match), use this parameter)</param>
        public void requestAPI<T>(string api, WWWForm form, UnityAction<Response<T>> callback, UnityAction<string> errorCallback = null, bool isRawLink = false)
        {
            StartCoroutine(requestPost<T>(api, form, callback, errorCallback, isRawLink));
        }

        public IEnumerator requestPost<T>(string api, WWWForm form, UnityAction<Response<T>> callback, UnityAction<string> errorCallback = null, bool isRawLink = false)
        {
            string api_url = isRawLink ? api : API_BASE_URL + (API_BASE_PORT.Length == 0 ? "" : ":" + API_BASE_PORT) + api;
            Debug.Log(api_url + ", params : " + UTF8Encoding.UTF8.GetString(form.data));
            using (UnityWebRequest response = UnityWebRequest.Post(api_url, form))
            {
                response.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
                if (!api_url.ToLower().Contains("jwt") && !api_url.ToLower().Contains("meta"))
                    response.SetRequestHeader("X-Token", "Bearer " + StorageManager.JWT);
#if NEXON_API
                if (!string.IsNullOrEmpty(NEXON_API_KEY))
                    response.SetRequestHeader("x-nxopen-api-key", NEXON_API_KEY);
#endif

                UIManager.ShowIndicator();
                yield return response.SendWebRequest();
                UIManager.HideIndicator();

                try
                {
                    var data = Parse<Response<T>>(response.downloadHandler);
                    //data.ParseJson(response.downloadHandler);
                    if (data != null && data.error == null)
                    {
                        Debug.Log(response.downloadHandler.text);
                        callback.Invoke(data);
                    }
                    else if (data != null)
                    {
                        if (errorCallback != null)
                            errorCallback.Invoke(data.error);
                        Debug.LogError(data.error);
                    }
                    else
                    {
                        if (errorCallback != null)
                            errorCallback.Invoke(response.error);
                        Debug.LogError(response.error);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(response.error + "\n" + e);
                }

            }
        }

        public async Task<Texture> GetTexture(string url)
        {
            var response = UnityWebRequestTexture.GetTexture(url);
#if NEXON_API
            if (!string.IsNullOrEmpty(NEXON_API_KEY))
                response.SetRequestHeader("x-nxopen-api-key", NEXON_API_KEY);
#endif
            UIManager.ShowIndicator();

            var result = await response.SendWebRequest();

            UIManager.HideIndicator();
            try
            {
                if (result == UnityWebRequest.Result.Success)
                {
                    var tex = ((DownloadHandlerTexture)response.downloadHandler).texture;
                    response.Dispose();
                    return tex;
                }
                else
                {
                    Debug.Log(response.error);
                }
            }
            catch (Exception e)
            {
                Debug.Log(response.error);
            }
            return null;
        }

        public async Task<AudioClip> GetAudioCip(string url, AudioType audioType = AudioType.MPEG)
        {
            var response = UnityWebRequestMultimedia.GetAudioClip(url, audioType);

            UIManager.ShowIndicator();

            var result = await response.SendWebRequest();

            UIManager.HideIndicator();
            try
            {
                if (result == UnityWebRequest.Result.Success)
                {
                    var clip = ((DownloadHandlerAudioClip)response.downloadHandler).audioClip;
                    response.Dispose();
                    return clip;
                }
                else
                {
                    Debug.Log(response.error);
                }
            }
            catch (Exception e)
            {
                Debug.Log(response.error);
            }
            return null;
        }

        /// <summary>받은 json string 데이터를 파싱하여 클래스로 변환 (convert from json string to class)</summary>
        public T Parse<T>(DownloadHandler handler)
        {
            var data = JsonUtility.FromJson<T>(handler.text);
            return data;
        }
        #endregion

        public void Init(string ip, string port)
        {
            API_BASE_URL = ip;
            API_BASE_PORT = port;
        }

        public async void Init()
        {
#if USE_CLOUD_CODE
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            module = new IroncowModuleBindings(CloudCodeService.Instance);
            try
            {
                // Call the function within the module and provide the parameters we defined in there
                var result = await module.Signin("test");
                //var result = await module.SayHello("www");
                Debug.Log(result);
            }
            catch (CloudCodeException exception)
            {
                Debug.LogException(exception);
                var result = module.Signup("test");
                
            }
#endif
            isInit = true;
        }
    }
}