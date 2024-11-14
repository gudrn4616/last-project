using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Ironcow
{
    public static class Util
    {
        public static string dataPath
        {
            get
            {
#if !UNITY_EDITOR
            return Application.persistentDataPath;
#else
                return Application.streamingAssetsPath + "~";
#endif
            }
        }

        public static Color ParseHexToColor(string hex)
        {
            Color color;
            hex = hex.Contains("#") ? hex : "#" + hex;
            ColorUtility.TryParseHtmlString(hex, out color);
            return color;
        }

        public static string ParseColorToHex(Color color)
        {
            return ColorUtility.ToHtmlStringRGB(color);
        }

        public static void SetLayer(GameObject target, int layer)
        {
            target.layer = layer;
            for (int i = 0; i < target.transform.childCount; i++)
            {
                SetLayer(target.transform.GetChild(i).gameObject, layer);
            }
        }

        public static Transform FindChild(Transform target, string name)
        {
            var ret = target.Find(name);
            if (ret == null)
            {
                for (int i = 0; i < target.childCount; i++)
                {
                    ret = FindChild(target.GetChild(i), name);
                    if (ret != null) break;
                }
            }
            return ret;
        }

        public static string ParseComma(int value)
        {
            return string.Format("{0:#,##0}", value);
        }

        public static string ParseComma(float value)
        {
            return string.Format("{0:#,##0}", value);
        }

        public static int Next(int num, int min, int max, bool isEqual = true)
        {
            num++;
            if (max <= num + (isEqual ? -1 : 0))
            {
                num = min;
            }
            return num;
        }

        public static int Prev(int num, int min, int max, bool isEqual = true)
        {
            num--;
            if (min >= num + (isEqual ? -1 : 0))
            {
                num = max;
            }
            return num;
        }

        //끝값 제외
        public static int Random(int min, int max)
        {
            return UnityEngine.Random.Range(min, max);
        }

        public static float Random(float min, float max)
        {
            return UnityEngine.Random.Range(min, max);
        }

#if UNITY_EDITOR
        public static string ConvertTargetPlatformToString(UnityEditor.BuildTargetGroup target)
        {
            return target == UnityEditor.BuildTargetGroup.Android ? "Android" : target == UnityEditor.BuildTargetGroup.iOS ? "IOS" : "StandaloneWindows";
        }
#endif

        public static string GetPlatformToString()
        {
            /*#if UNITY_EDITOR
                    if (Application.platform == RuntimePlatform.WindowsEditor)
                    { 
                        return EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android ? "Android" : EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS ? "IOS" : "StandaloneWindows";
                    }
                    else
            #endif*/
            {
                return Application.platform == RuntimePlatform.Android ? "Android" : Application.platform == RuntimePlatform.IPhonePlayer ? "IOS" : "StandaloneWindows";
            }
        }

        public static string GetBuildTargetToString()
        {
#if UNITY_EDITOR
            return EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android ? "Android" : EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS ? "IOS" : "StandaloneWindows";
#endif
            return Application.platform == RuntimePlatform.Android ? "Android" : Application.platform == RuntimePlatform.IPhonePlayer ? "IOS" : "StandaloneWindows";
        }

        public static string GetLanguage()
        {
#if UNITY_EDITOR
            //return "English";
            return Application.systemLanguage.ToString();
#else
        return Application.systemLanguage.ToString();
#endif
        }
        /*
        public static Rect WebviewMargin(RectTransform rectTrans)
        {
            var camera = CameraManager.instance.UICamera;
            var corners = new Vector3[4];

            rectTrans.GetWorldCorners(corners);
            var screenCorner1 = RectTransformUtility.WorldToScreenPoint(camera, corners[1]);
            var screenCorner3 = RectTransformUtility.WorldToScreenPoint(camera, corners[3]);

            var screenRect = new Rect();
            screenRect.x = screenCorner1.x;
            screenRect.width = screenCorner3.x - screenRect.x;
            screenRect.y = screenCorner3.y;
            screenRect.height = screenCorner1.y - screenRect.y;
#if false
        var margin = new Vector4( screenCorner1.x, Screen.height - screenCorner1.y, Screen.width - screenCorner3.x, screenCorner3.y );
        Debug.Log("margin x:" + margin.x + "y:" + margin.y + "z:" + margin.z + "w:" + margin.w );
        _WebView.SetMargins( (int)margin.x, (int)margin.y, (int)margin.z, (int)margin.w ); 
#endif
            return screenRect;
        }*/

        public static void TextEllipsis(Text text)
        {
            if (text.preferredWidth > text.rectTransform.sizeDelta.x)
            {
                for (int i = text.text.Length; i > 0; i--)
                {
                    text.text = text.text.Substring(0, i) + "...";
                    if (text.preferredWidth < text.rectTransform.sizeDelta.x) break;
                }
            }
        }

        public static void TextHypen(Text text, int munite, string seconde)
        {
            if (text.preferredWidth > text.rectTransform.sizeDelta.x)
            {
                for (int i = text.text.Length - 9; i > 0; i--)
                {
                    text.text = text.text.Substring(0, i) + "..." + $" * {munite}:{seconde}";
                    if (text.preferredWidth < text.rectTransform.sizeDelta.x) break;
                }
            }
        }

        public static T GetAPIData<T>(string s) where T : new()
        {
            T data = new T();
            data = JsonUtility.FromJson<T>(s);

            return data;
        }

        public static async Task<string> GetMyIpAddress()
        {
            var req = UnityWebRequest.Get("http://checkip.dyndns.org");
            if (req == null) return "";
            await req.SendWebRequest();
            var ip = req.downloadHandler.text;
            ip = ip.Substring(ip.IndexOf(":") + 1);
            ip = ip.Substring(0, ip.IndexOf("<"));
            ip = ip.Trim();
            return ip;
        }

        public static bool IsLocalHost(IPEndPoint ep)
        {
            return ep.Address.ToString().Equals(GetMyIpAddress());
        }

        public static string ChangeNaming(string enumName)
        {
            char[] arr = enumName.ToLower().ToCharArray();
            for (int i = 0; i < arr.Length; i++)
            {
                if (i == 0)
                    arr[i] = char.ToUpper(arr[i]);
                if (arr[i] == '_')
                    arr[i + 1] = char.ToUpper(arr[i + 1]);
            }
            return new string(arr);
        }

        /// <summary>
        /// 특수문자 및 띄어쓰기 체크
        /// </summary>
        /// <param name="str"></param>
        /// <param name="isSpace"></param>
        /// <returns></returns>
        public static bool CheckString(string str, bool isSpace = false)
        {
            if (isSpace) // 특수문자 X 띄어쓰기 O
            {
                if (Regex.IsMatch(str, @"[^0-9a-zA-Zㄱ-ㅎ가-힣ㅏ-ㅣ\s]"))
                {
                    return true;
                }
            }
            else // 특수문자 및 띄어쓰기 X
            {
                if (Regex.IsMatch(str, @"[^0-9a-zA-Zㄱ-ㅎ가-힣ㅏ-ㅣ]"))
                {
                    return true;
                }
            }

            return false;
        }

        public static string CheckStrLength(UnityEngine.UI.Text text, string sourceStr)
        {
            text.text = sourceStr;

            if (text.preferredWidth > text.rectTransform.sizeDelta.x)
            {
                for (int i = text.text.Length; i > 0; i--)
                {
                    text.text = text.text.Substring(0, i) + "...";
                    if (text.preferredWidth < text.rectTransform.sizeDelta.x) break;
                }
            }

            return text.text;
        }

        public static float CalculateAngle(Vector3 from, Vector3 to)
        {
            return Quaternion.FromToRotation(Vector3.up, to - from).eulerAngles.z;
        }

        public static List<int> GetTargets(int target)
        {
            List<int> retList = new List<int>();
            var startTarget = 16;
            for(int i = 4; i >= 0; i--)
            {
                if(i >= startTarget)
                {
                    retList.Add(i); 
                    target -= startTarget;
                    startTarget = startTarget >> 1;
                }
            }
            retList.Reverse();
            return retList;
        }

        public static float GetRandomBoundsAngle(RectTransform border)
        {
            var angleY = border.pivot.x == 0.5f ? 0 : 1;
            var angleX = border.pivot.y == 0.5f ? 0 : 1;

            var angle = angleY * Random(border.pivot.x * 180 + 90, border.pivot.x * 180 + 270) + angleX * Random(border.pivot.y * 180, border.pivot.y * 180 + 180);
            return (float)angle % 360;
        }
        
        public static int GetSkillAttrToOptionAttr(int skillAttr)
        {
            int optionAttr = 0;
            switch(skillAttr)
            {
                case 0:     // 무속성
                    {
                        optionAttr = 21;
                    }
                    break;
                case 1:     // 화염속성
                    {
                        optionAttr = 17;
                    }
                    break;
                case 2:     // 빛속성
                    {
                        optionAttr = 24;
                    }
                    break;
                case 3:     // 독속성
                    {
                        optionAttr = 23;
                    }
                    break;
                case 4:     // 성속성
                    {
                        optionAttr = 20;
                    }
                    break;
                case 5:     // 암흑속성
                    {
                        optionAttr = 22;
                    }
                    break;
                case 6:     // 얼음속성
                    {
                        optionAttr = 18;
                    }
                    break;
                case 7:     // 전기속성
                    {
                        optionAttr = 19;
                    }
                    break;
                case 8:     // 화염 + 얼음 속성
                    {
                        optionAttr = 17;
                    }
                    break;
            }
            return optionAttr;
        }

        public static bool IsPointerOverUI()
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, results);

            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].gameObject.layer == LayerMask.NameToLayer("UI"))
                    return true;
            }

            return false;
        }

        public static int GetDistance(int idx, int targetIdx, int max)
        {
            var sec = idx + max;
            return Mathf.Min(Mathf.Abs(idx - targetIdx), Mathf.Abs(sec - targetIdx));
        }
    }
}