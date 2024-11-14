using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System;
using System.Text;
using UnityEngine.Networking;
using Google.Protobuf.Collections;

namespace Ironcow
{
    public static class ExtensionMethods
    {
#if !UNITY_6000_0_OR_NEWER
        public static TaskAwaiter<bool> GetAwaiter(this AsyncOperation reqOp)
        {
            TaskCompletionSource<bool> tsc = new TaskCompletionSource<bool>();
            reqOp.completed += asyncOp => tsc.TrySetResult(reqOp.isDone);

            if (reqOp.isDone)
                tsc.TrySetResult(reqOp.isDone);

            return tsc.Task.GetAwaiter();
        }
#endif

        public static TaskAwaiter<UnityEngine.Object> GetAwaiter(this ResourceRequest reqOp)
        {
            TaskCompletionSource<UnityEngine.Object> tsc = new TaskCompletionSource<UnityEngine.Object>();
            reqOp.completed += asyncOp => tsc.TrySetResult(reqOp.asset);

            if (reqOp.isDone)
                tsc.TrySetResult(reqOp.asset);

            return tsc.Task.GetAwaiter();
        }

        public static List<T> ToList<T>(this T[] array)
        {
            return new List<T>(array);
        }

        public static T Last<T>(this T[] array)
        {
            var list = array.ToList<T>();
            return list.Last();
        }

        public static List<T> Clone<T>(this List<T> list)
        {
            var retList = new List<T>();
            foreach (var item in list)
            {
                retList.Add(item);
            }
            return retList;
        }


        public static T RemovePullLast<T>(this List<T> list)
        {
            var data = list.Last();
            list.Remove(data);
            return data;
        }

        public static void RemoveLast<T>(this List<T> list)
        {
            var idx = list.Count - 1;
            list.RemoveAt(idx);
        }

        public static T RandomValue<T>(this List<T> list)
        {
            var rand = Util.Random(0, list.Count);
            return list[rand];
        }

        public static T RandomValue<T>(this List<T> list, int max)
        {
            var rand = Util.Random(0, max);
            return list[rand];
        }

        public static T RandomValue<T>(this List<T> list, int min, int max)
        {
            var rand = Util.Random(min, max);
            return list[rand];
        }

        public static T RandomValue<T>(this T[] array)
        {
            return array.ToList().RandomValue();
        }

        public static T RandomPeek<T>(this List<T> list)
        {
            var rand = Util.Random(0, list.Count);
            var t = list[rand];
            list.RemoveAt(rand);
            return t;
        }

        public static RectTransform rectTransform(this MonoBehaviour mono)
        {
            return mono.transform as RectTransform;
        }

        public static Sprite ToSprite(this Texture2D tex)
        {
            Rect rect = new Rect(0, 0, tex.width, tex.height);
            return Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f));
        }

        public static Sprite ToSprite(this Texture tex)
        {
            return ToSprite((Texture2D)tex);
        }
        public static Texture2D ToTexture2D(this RenderTexture rTex)
        {
            Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.ARGB32, false, true);
            var old_rt = RenderTexture.active;
            RenderTexture.active = rTex;

            tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            tex.Apply();

            RenderTexture.active = old_rt;
            return tex;
        }

        public static Texture2D ToTexture2D(this RenderTexture rTex, Vector2 size)
        {
            Texture2D tex = new Texture2D((int)size.x, (int)size.y, TextureFormat.ARGB32, false, true);
            var old_rt = RenderTexture.active;
            RenderTexture.active = rTex;

            int destX = rTex.width / 2 - (int)size.x / 2;
            int destY = rTex.height / 2 - (int)size.y / 2;
            tex.ReadPixels(new Rect(destX, destY, size.x, size.y), 0, 0);
            tex.Apply();

            RenderTexture.active = old_rt;
            return tex;
        }


        public static Texture2D ToTexture2D(this RenderTexture rTex, Rect rect)
        {
            Texture2D tex = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, false, true);
            var old_rt = RenderTexture.active;
            RenderTexture.active = rTex;

            tex.ReadPixels(rect, 0, 0);
            tex.Apply();

            RenderTexture.active = old_rt;
            return tex;
        }

        public static Texture2D DeCompress(this Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                        source.width,
                        source.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }

        public static T Clone<T>(this T data) where T : BaseDataSO
        {
            return (T)data.clone;
        }

        public static T First<V, T>(this Dictionary<V, T> dic)
        {
            var keys = new List<V>(dic.Keys);
            return dic[keys[0]];
        }

        public static string FileName(this string path)
        {
            return path.Split('/').Last().Split('.')[0];
        }

        public static string Extension(this string path)
        {
            return path.Split('/').Last().Split('.').Last().ToLower();
        }

        public static string AssetType(this string path)
        {
            return path.Split('/')[3];
        }

        public static T First<T>(this List<T> list)
        {
            return list[0];
        }


        public static TaskAwaiter<AssetBundle> GetAwaiter(this AssetBundleCreateRequest instruction)
        {
            TaskCompletionSource<AssetBundle> tsc = new TaskCompletionSource<AssetBundle>();
            instruction.completed += asyncOp => tsc.TrySetResult(instruction.assetBundle);

            if (instruction.isDone)
                tsc.TrySetResult(instruction.assetBundle);

            return tsc.Task.GetAwaiter();
        }
        public static async Task ForEachAsync<T>(this List<T> list, Func<T, Task> func)
        {
            foreach (var value in list)
            {
                await func(value);
            }
        }

        public static RectTransform RectTransform(this MonoBehaviour mono)
        {
            return mono.transform as RectTransform;
        }

        public static RectTransform RectTransform(this GameObject gameObject)
        {
            return gameObject.transform as RectTransform;
        }

        public static RectTransform RectTransform(this Transform transform)
        {
            return transform as RectTransform;
        }

        public static RectTransform RectTransform(this Animator animator)
        {
            return animator.transform as RectTransform;
        }

        public static RectTransform RectTransform(this Canvas canvas)
        {
            return canvas.transform as RectTransform;
        }

        public static RectTransform RectTransform(this Collider2D collider)
        {
            return collider.transform as RectTransform;
        }

        public static string ToComma(this int val)
        {
            return string.Format("{0:#,##0}", val);
        }

        public static string ToComma(this long val)
        {
            return string.Format("{0:#,##0}", val);
        }

        public static string ToTime(this int val)
        {
            var second = val % 60;
            var minute = val / 60 % 60;
            var hour = val / 360;
            return hour > 0 ? string.Format("{0:00}:{1:00}:{2:00}", hour, minute, second) : string.Format("{0:00}:{1:00}", minute, second);
        }

        public static string ToTime(this float val)
        {
            return ((int)val).ToTime();
        }

        public static int BitToInt(this int val)
        {
            int count = 0;
            int bit = val;
            while(bit != 0)
            {
                bit = bit >> 1;
                count++;
            }
            return count;
        }
        
        public static List<int> BitToIntList(this int val)
        {
            int count = 0;
            int bit = val;
            List<int> list = new List<int>();

            while (bit != 0)
            {
                if ((1 & bit) == 1)
                {
                    list.Add(count);
                }
                bit = bit >> 1;
                count++;
            }
            return list;
        }

        public static float NormalToAngle(this Vector3 normal)
        {
            return (Mathf.Atan2(normal.y, normal.x) * 180 / Mathf.PI) + 90;
        }

        public static List<string> FindAllKey(this List<string> list, List<string> keys)
        {
            var retList = new List<string>(list);
            keys.ForEach(obj => retList.RemoveAll(str => !str.ToLower().Contains(obj.ToLower())));
            return retList;
        }
#if USE_ASYNC || USE_COROUTINE
        public static List<string> FindKey(this IResourceLocator locator, List<string> keys)
        {
            var list = locator.Keys.ToList().ConvertObjectListToStringList();
            return list.FindAllKey(keys);
        }
#endif

        public static List<string> ConvertObjectListToStringList(this List<object> list)
        {
            List<string> retList = new List<string>();
            foreach(var key in list)
            {
                retList.Add(key.ToString());
            }
            return retList;
        }

        public static int GetColumnCount(this string str)
        {
            return str.Split('{').Length - 1;
        }

        public static string ToDataString(this List<string> list)
        {
            string ret = "";
            list.ForEach(obj => ret += obj + "|");
            
            if (ret.Length > 0)
            {
                return ret.Substring(0, ret.Length - 1);
            }
            else
            {
                return ret;
            }
        }

        public static string ToDataString(this List<int> list)
        {
            string ret = "";
            list.ForEach(obj => ret += obj + "|");
            if (ret.Length > 0)
            {
                return ret.Substring(0, ret.Length - 1);
            }
            else
            {
                return ret;
            }
        }

        public static string ToDataString(this List<float> list)
        {
            string ret = "";
            list.ForEach(obj => ret += obj + "|");
            if (ret.Length > 0)
            {
                return ret.Substring(0, ret.Length - 1);
            }
            else
            {
                return ret;
            }
        }

        public static List<T> ToList<T>(this List<Collider2D> colliders)
        {
            List<T> list = new List<T>();

            foreach (Collider2D col in colliders)
            {
                if (col.TryGetComponent(out T obj))
                {
                    list.Add(obj);
                }
            }

            return list;
        }

        public static void ForEachChild(this Transform parent, Action<GameObject> action)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                action.Invoke(parent.GetChild(i).gameObject);
            }
        }

        public static T AddOrGetComponent<T>(this Component component) where T : Component
        {
            var t = component.GetComponent<T>();
            if (t == null) t = component.gameObject.AddComponent<T>();
            return t;
        }

        public static void AddRange<T>(this Dictionary<string, BaseDataSO> dic, List<T> datas) where T : BaseDataSO
        {
            foreach (var data in datas)
            {
                if(!dic.ContainsKey(data.rcode)) if (!dic.ContainsKey(data.rcode))
                        dic.Add(data.rcode, data);
            }
        }

        public static byte[] ToDataArray(this string str)
        {
            return UTF8Encoding.UTF8.GetBytes(str);
        }

        public static Vector3 ToVector3(this CharacterPositionData position)
        {
            return new Vector3((float)position.X, (float)position.Y);
        }

        public static List<T> Shuffle<T>(this List<T> values)
        {
            System.Random rnd = new System.Random();
            var shuffled = values.OrderBy(_ => rnd.Next()).ToList();

            return shuffled;
        }

        public static UserInfo ToUserInfo(this UserData userData)
        {
            return new UserInfo(userData);
        }

        public static CardDataSO GetCardData(this CardData cardData)
        {
            return DataManager.instance.GetData<CardDataSO>(cardData.GetRcode());
        }

        public static string GetCardRcode(this CardType idx)
        {
            return string.Format("CAD{0:00000}", (int)idx);
        }

        public static string GetCharaRcode(this CharacterType idx)
        {
            return string.Format("CHA{0:00000}", (int)idx);
        }

        public static string GetRcode(this CharacterData data)
        {
            return string.Format("CHA{0:00000}", (int)data.CharacterType);
        }

        public static string GetRcode(this CardData data)
        {
            return string.Format("CAD{0:00000}", (int)data.Type);
        }

        public static CardDataSO GetCardData(this CardType idx)
        {
            return DataManager.instance.GetData<CardDataSO>(string.Format("CAD{0:00000}", (int)idx));
        }

        public static List<UserInfo> UpdateUserData(this List<UserInfo> users, RepeatedField<UserData> userdatas)
        {
            for (int i = 0; i < userdatas.Count; i++)
            {
                var user = users.Find(obj => obj.id == userdatas[i].Id);
                user.UpdateUserInfo(userdatas[i]);
            }
            return users;
        }

        public static UserInfo UpdateUserData(this List<UserInfo> users, UserData userdata)
        {
            var user = users.Find(obj => obj.id == userdata.Id);
            user.UpdateUserInfo(userdata);
            return user;
        }
    }
}