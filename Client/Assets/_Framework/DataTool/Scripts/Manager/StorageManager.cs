using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Ironcow
{
    public class StorageManager : MonoSingleton<StorageManager>
    {
        #region keys
        private static UserInfo _userInfo;
        public static UserInfo userInfo
        {
            get
            {
                if (_userInfo == null)
                {
                    var data = PlayerPrefs.GetString("userInfo", JsonUtility.ToJson(new UserInfo()));
                    _userInfo = new UserInfo();// JsonUtility.FromJson<UserInfo>(data);
                }
                return _userInfo;
            }
            set
            {
                PlayerPrefs.SetString("userInfo", JsonUtility.ToJson(_userInfo));
            }
        }

        public static DateTime HEART_TIME { get => DateTime.Parse(PlayerPrefs.GetString("heart_time", DateTime.Now.ToString())); set => PlayerPrefs.SetString("heart_time", value.ToString()); }

        public static string LOGIN_INFO { get => PlayerPrefs.GetString("loginInfo", ""); set => PlayerPrefs.SetString("loginInfo", value); }
        public static string EDITOR_NICKNAME { get => PlayerPrefs.GetString("editorNickname", "뉴토_"); set => PlayerPrefs.SetString("editorNickname", value); }
        public const string EDITOR_NICKNAME_KEY = "editorNickname";
        public static string USER_ID { get => PlayerPrefs.GetString("USER_ID", null); }
        /// <summary>자동 로그인 시 유저의 ID(KEY)</summary>
        public static string USER_ID_AUTO { get => PlayerPrefs.GetString("USER_ID_AUTO", null); set => PlayerPrefs.SetString("USER_ID_AUTO", value); }

        public const string USER_ID_AUTO_KEY = "USER_ID_AUTO";

        /// <summary>로그인 하는 플랫폼(Google/Facebook/Kakao/Apple)</summary>
        public static string LOGIN_PLATFORM { get => PlayerPrefs.GetString("LOGIN_PLATFORM", null); set => PlayerPrefs.SetString("LOGIN_PLATFORM", value); }

        public const string LOGIN_PLATFORM_KEY = "LOGIN_PLATFORM";

        public static int USER_MIDX { get => PlayerPrefs.GetInt("USER_MIDX", 0); }
        public const string USER_MIDX_KEY = "USER_MIDX";

        public static string PROFILE_INTRODUCTION { get => PlayerPrefs.GetString("PROFILE_INTRODUCTION", "저는 호기심이 많아 다양한 취미 활동에 도전하지만\n주로 활동적인 일에 더 적극적입니다!"); set => PlayerPrefs.SetString("PROFILE_INTRODUCTION", value); }

        public static string JWT { get => PlayerPrefs.GetString("JWT", ""); set => PlayerPrefs.SetString("JWT", value); }
        public static bool isJWT { get => JWT.Length > 0; }

        public static string INSIDE_VIEW { get => PlayerPrefs.GetString("InsideView", "view1"); set => PlayerPrefs.SetString("InsideView", value); }
        #endregion

        public void SetEditorNickname(string nickname)
        {
            PlayerPrefs.SetString(EDITOR_NICKNAME_KEY, nickname);
        }

        public string GetEditorNickname()
        {
            return EDITOR_NICKNAME;
        }

        public static void OnSaveUserInfo()
        {
            userInfo = userInfo;
        }
    }
}