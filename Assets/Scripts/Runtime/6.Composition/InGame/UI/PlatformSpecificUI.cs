using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KillChord.Runtime.Composition.InGame.UI
{
    /// <summary>
    /// プラットフォームによってUIの表示/非表示を切り替えるクラス。
    /// </summary>
    public class PlatformSpecificUI : MonoBehaviour
    {
        [Header("スマホ用のUIをEditorで表示するか")] [SerializeField, Tooltip("エディタ上でスマホ用の UI を表示するか。")]
        private bool _showInEditor = false;

        /// <summary>
        ///     実行中のプラットフォームに応じて UI の表示を切り替える。
        /// </summary>
        private void Awake()
        {
#if UNITY_EDITOR
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                case BuildTarget.iOS:
                    gameObject.SetActive(_showInEditor);
                    break;

                default:
                    gameObject.SetActive(false);
                    break;
            }
#else
#if UNITY_ANDROID || UNITY_IOS
            gameObject.SetActive(true);
#else
            gameObject.SetActive(false);
#endif
#endif
        }
    }
}