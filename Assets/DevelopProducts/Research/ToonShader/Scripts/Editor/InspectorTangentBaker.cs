#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace DevelopProducts.ToonShader.EditorExtension
{
    [InitializeOnLoad]
    public class InspectorTangentBaker
    {
        static InspectorTangentBaker()
        {
            Editor.finishedDefaultHeaderGUI += OnPostHeaderGUI;
        }
        private static void OnPostHeaderGUI(Editor editor)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (editor.target is GameObject gameObject)
                if (GUILayout.Button("BakeSmoothNormals", GUILayout.Width(200)))
                {
                    var array = gameObject.GetComponentsInChildren<MeshFilter>(true);
                    foreach (var item in array)
                    {
                        TangentBaker.BakeMesh(item.sharedMesh);
                    }

                    var array2 = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                    foreach (var item in array2)
                    {
                        TangentBaker.BakeMesh(item.sharedMesh);
                    }

                }
            GUILayout.EndHorizontal();
        }
    }
}
#endif