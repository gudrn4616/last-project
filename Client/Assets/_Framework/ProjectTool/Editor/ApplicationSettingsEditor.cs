using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Ironcow
{
	[CustomEditor(typeof(ApplicationSettings))]
	public class ApplicationSettingsEditor : UnityEditor.Editor
	{
		static bool DrawToolbar(ref int selected, string[] texts)
		{
			bool dirty = false;

			int value = GUILayout.Toolbar(selected, texts, EditorStyles.toolbarButton);
			if (value != selected)
			{
				selected = value;
				dirty = true;
			}

			return dirty;
		}

		private ApplicationSettings applicationSettings = null;
		private List<string> toolbarTexts = null;
		private int selected = 0;

		private void OnEnable()
		{
			this.applicationSettings = base.target as ApplicationSettings;

			this.toolbarTexts = new List<string>();
			this.toolbarTexts.Add("Property");
		}

		public override void OnInspectorGUI()
		{
			if (this.applicationSettings == null)
				return;

			EditorGUI.BeginChangeCheck();

			if (DrawToolbar(ref this.selected, this.toolbarTexts.ToArray()))
			{

			}

			GUILayout.Space(10f);

			EditorGUILayout.LabelField("Default", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			{
				this.applicationSettings.LogEnabled = EditorGUILayout.Toggle("Log Enabled", this.applicationSettings.LogEnabled);
				this.applicationSettings.Development = EditorGUILayout.Toggle("Development", this.applicationSettings.Development);
			}
			EditorGUI.indentLevel--;
			GUILayout.Space(10f);

			EditorGUILayout.LabelField("Facebook", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			{
				//EditorGUILayout.LabelField( "SDK Version", Facebook.Unity.FacebookSdkVersion.Build );
				this.applicationSettings.FacebookAppName = EditorGUILayout.TextField("App Name", this.applicationSettings.FacebookAppName);
				this.applicationSettings.FacebookAppId = EditorGUILayout.TextField("App Id", this.applicationSettings.FacebookAppId);

				if (GUILayout.Button("Edit"))
				{
					//Facebook.Unity.Editor.FacebookSettingsEditor.Edit();
				}
			}
			EditorGUI.indentLevel--;

			if (EditorGUI.EndChangeCheck())
			{
				if (EditorApplication.isPlaying)
					Debug.unityLogger.logEnabled = this.applicationSettings.LogEnabled;

				EditorUtility.SetDirty(this.applicationSettings);
			}
		}
	}
}