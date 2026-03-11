using DevelopProducts.ToonShader.Utility;
using UnityEditor;
using UnityEngine;

namespace DevelopProducts.ToonShader.Editor
{
    public class OutLineInjectorWindow : EditorWindow
    {
        [MenuItem(Const.WINDOW_PATH + nameof(OutLineInjectorWindow))]
        public static void Open()
        {
            OutLineInjectorWindow window = GetWindow<OutLineInjectorWindow>();
            window.titleContent = new GUIContent("Outline Injector");
            window.Show();
        }

        private Material _outline;
        private GameObject _targetObject;

        private void OnGUI()
        {
            _outline = (Material)EditorGUILayout.ObjectField(
                "Outline Material",
                _outline,
                typeof(Material),
                false
                );

            _targetObject = (GameObject)EditorGUILayout.ObjectField(
                "Target GameObject",
                _targetObject,
                typeof(GameObject),
                true
                );

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("選択マテリアル: ", _outline != null ? _outline.name : "null");
            EditorGUILayout.LabelField("選択オブジェクト: ", _targetObject != null ? _targetObject.name : "null");

            if (_targetObject == null || _outline == null)
            {
                EditorGUILayout.HelpBox("アウトラインマテリアルと対象のゲームオブジェクトを指定してください。", MessageType.Warning);
                return;
            }

            if (GUILayout.Button("ベイクする"))
            {
                Bake(_targetObject, _outline);
            }
        }

        private static void Bake(GameObject obj, Material outline)
        {
            SkinnedMeshRenderer[] renderers = obj.GetComponentsInChildren<SkinnedMeshRenderer>();

            foreach (var renderer in renderers)
            {
                TangentBaker.BakeMesh(renderer.sharedMesh);
                var materials = renderer.materials;

                bool hasOutline = false;
                foreach (var item in materials)
                {
                    if (item == outline)
                    {
                        hasOutline = true;
                        break;
                    }
                }
                if (!hasOutline)
                    continue;
                var newMaterials = new Material[materials.Length + 1];
                for (int i = 0; i < materials.Length; i++)
                {
                    newMaterials[i] = materials[i];
                }
                newMaterials[^1] = outline;
            }
        }
    }
}