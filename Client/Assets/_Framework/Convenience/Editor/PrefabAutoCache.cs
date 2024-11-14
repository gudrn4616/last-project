using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ironcow
{
    public static class PrefabAutoCache
    {
#if USE_AUTO_CACHING
        [MenuItem("Ironcow/Tool/Selection Prefabs Auto Cache #&s")]
#endif
        public static void AutoCache()
        {
            var prefab = Selection.activeGameObject;
            var fields = prefab.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (!field.FieldType.IsSubclassOf(typeof(Component))) continue;
                var value = field.GetValue(prefab);
                var isNull = value == null;
                if (!isNull) isNull = value.Equals(null);
                if (!isNull) continue;
                var components = prefab.GetComponentsInChildren(field.FieldType);
                foreach (var component in components)
                {
                    if (component.name == field.Name) field.SetValue(prefab, component);
                }
                if (field.GetValue(prefab) == null)
                {
                    field.SetValue(prefab, prefab.GetComponent(field.FieldType));
                }
            }
        }

#if USE_AUTO_CACHING
        [MenuItem("Ironcow/Tool/All Prefabs Auto Cache #&a")]
#endif
        public static void AutoCacheAll()
        {
            var folders = EditorDataSO.SharedInstance.prefabFolders;
            foreach (var folder in folders)
            {
                var path = AssetDatabase.GetAssetPath(folder);
                var lists = AssetDatabase.FindAssets("", new string[] { path });
                foreach (var itemGuid in lists)
                {
                    var itemPath = AssetDatabase.GUIDToAssetPath(itemGuid);
                    var item = AssetDatabase.LoadAssetAtPath<GameObject>(itemPath);
                    if (item == null) continue;
                    var fields = item.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    foreach (var field in fields)
                    {
                        if (!field.FieldType.IsSubclassOf(typeof(Component))) continue;
                        var value = field.GetValue(item.GetType());
                        var isNull = value == null;
                        if (!isNull) isNull = value.Equals(null);
                        if (!isNull) continue;
                        var components = item.GetComponentsInChildren(field.FieldType);
                        foreach (var component in components)
                        {
                            if (component.name == field.Name) field.SetValue(item.GetType(), component);
                        }
                        if (field.GetValue(item) == null)
                        {
                            field.SetValue(item, item.GetComponent(field.FieldType));
                        }
                    }
                }
            }
        }
    }
}