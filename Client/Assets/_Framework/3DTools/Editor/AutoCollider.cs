using UnityEngine;
using UnityEditor;

namespace Ironcow
{
    public static class AutoCollider
    {
        [MenuItem("Ironcow/Tool/AddBoxCollider #&c")]
        private static void AdjustBounds()
        {
            var selection = Selection.activeGameObject;
            Renderer mr = selection.GetComponentInChildren<MeshRenderer>();                 //MeshRenderer 가져오기
            if (mr == null) mr = selection.GetComponentInChildren<SkinnedMeshRenderer>();   //없으면 SkinnedMeshRenderer로 가져오기
            if (mr == null) mr = selection.GetComponentInChildren<Renderer>();              //그래도 없으면 랜더러라도 찾아보기
            if (mr == null) return;                                                         //그래도 없으면 아예 없는거니 종료
            selection.transform.GetChild(0).position = new Vector3(0, -GetBottom(selection), 0);
            var box = selection.GetComponent<BoxCollider>();
            if (box == null) box = selection.AddComponent<BoxCollider>();
            box.center = mr.bounds.center;
            box.size = mr.bounds.size;
        }

        private static float GetBottom(GameObject gameObject)
        {
            var meshes = gameObject.GetComponentsInChildren<Renderer>();
            float min = 0;
            foreach (var mesh in meshes)
            {
                var bounds = mesh.bounds;
                var bottom = bounds.center.y - bounds.size.y / 2;
                min = Mathf.Min(min, bottom);
            }
            return min;
        }
    }
}