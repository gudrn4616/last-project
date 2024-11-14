
using UnityEditor;
using UnityEngine;


namespace Ironcow
{
    internal class AutoBoneSetting : Editor
    {
        [MenuItem("Ironcow/Tool/Set bones from SkinnedMeshResderers #&v")]
        private static void Setting()
        {
            Setting(Selection.activeTransform);
        }

        [MenuItem("CONTEXT/Transform/Set bones from SkinnedMeshResderers")]
        private static void Setting(MenuCommand command)
        {
            var transform = ((Transform)command.context);
            Setting(transform);
        }

        private static void Setting(Transform transform)
        {
            var skins = transform.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var skin in skins)
            {
                var bones = skin.bones.ToList();
                var rootBone = FindChild(skin.rootBone.name, transform);
                if (skin.rootBone == rootBone) continue;
                var newBones = new Transform[skin.bones.Length];
                for (int i = 0; i < bones.Count; i++)
                {
                    var target = FindChild(bones[i].name, rootBone);
                    newBones[i] = target;
                }
                skin.bones = newBones;
                skin.rootBone = rootBone;
            }
        }

        public static Transform FindChild(string name, Transform target)
        {
            if (target.name == name) return target;
            else
            {
                for (int i = 0; i < target.childCount; i++)
                {
                    var child = target.GetChild(i);
                    var newTarget = FindChild(name, child);
                    if (newTarget != null) return newTarget;
                }
            }
            return null;
        }
    }
}