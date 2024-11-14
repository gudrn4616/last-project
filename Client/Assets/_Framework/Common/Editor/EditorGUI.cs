using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Ironcow
{
	static public class GUI
	{
		static public bool DrawToolbar( ref int selected, string[] texts )
		{
			bool dirty = false;

			int value = GUILayout.Toolbar( selected, texts, EditorStyles.toolbarButton );
			if( value != selected )
			{
				selected = value;
				dirty = true;
			}

			return dirty;
		}

		static public bool DrawSearchField( ref string searchFilter )
		{
			bool dirty = false;

			GUILayout.BeginHorizontal();
			{
				string after = EditorGUILayout.TextField( string.Empty, searchFilter, "SearchTextField" );

				if( GUILayout.Button( string.Empty, "SearchCancelButton", GUILayout.Width( 18f ) ) )
				{
					after = "";
					GUIUtility.keyboardControl = 0;
				}

				if( searchFilter != after )
				{
					searchFilter = after;
					dirty = true;
				}
			}
			GUILayout.EndHorizontal();

			return dirty;
		}
	}
}