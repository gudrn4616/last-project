using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Ironcow
{
#if UNITY_EDITOR
	[InitializeOnLoad]
#endif
	public class ApplicationSettings : ScriptableObject
	{
		static private ApplicationSettings instance = null;
		static public ApplicationSettings SharedInstance
		{
			get
			{
				if (instance == null)
				{
					instance = Resources.Load("ApplicationSettings") as ApplicationSettings;
					if (instance == null)
					{
						instance = CreateInstance<ApplicationSettings>();

#if UNITY_EDITOR
						string directory = System.IO.Path.Combine(Application.dataPath, "Settings/Resources");
						if (!System.IO.Directory.Exists(directory))
							AssetDatabase.CreateFolder("Assets/Settings", "Resources");

						string fullPath = System.IO.Path.Combine(System.IO.Path.Combine("Assets", "Settings/Resources"), "ApplicationSettings.asset");
						AssetDatabase.CreateAsset(instance, fullPath);
#endif
					}
				}

				return instance;
			}
		}

#if UNITY_EDITOR
		[MenuItem("Ironcow/Data/Application Settings")]
		private static void Edit()
		{
			Selection.activeObject = SharedInstance;
		}
#endif

		[HideInInspector]
		public List<BuildSettings> BuildSettingsList = new List<BuildSettings>();

		[HideInInspector]
		public bool LogEnabled = true;

		[HideInInspector]
		public BuildSettings CurrentBuildSettings = null;

		[HideInInspector]
		public string FacebookAppName = "";

		[HideInInspector]
		public string FacebookAppId = "";

		[HideInInspector]
		public bool Development = false;

		[HideInInspector]
		public bool TestMode = false;

		[HideInInspector]
		public bool FullBuild = false;
	}
}