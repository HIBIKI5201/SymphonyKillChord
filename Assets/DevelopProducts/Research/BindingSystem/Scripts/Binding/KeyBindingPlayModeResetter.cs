#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.InputSystem;

namespace DevelopProducts.BindingSystem
{
    /// <summary>
    ///     キー再割り当てのオーバーライドはInputActionAsset本体に書き込まれるため、
    ///     再生終了後もエディタ上に残り続けてしまう。
    ///     開発中は再生開始時に毎回オーバーライドをリセットし、常に初期キー配置で始まるようにする。
    /// </summary>
    [InitializeOnLoad]
    public static class KeyBindingPlayModeResetter
    {
        static KeyBindingPlayModeResetter()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode) return;

            foreach (var guid in AssetDatabase.FindAssets("t:InputActionAsset"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
                asset.RemoveAllBindingOverrides();
            }
        }
    }
}
#endif
