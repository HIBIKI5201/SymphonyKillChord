using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.UI
{
    /// <summary>
    ///     プラットフォームによってUIの表示/非表示を切り替えるクラス。
    /// </summary>
    public class PlatformSpecificUI : MonoBehaviour
    {
        [Header("スマホ用のUIをEditorで表示するか")]
        [SerializeField] private bool _showInEditor = false;

        private void Awake()
        {
#if UNITY_EDITOR
            this.gameObject.SetActive(_showInEditor);
#elif UNITY_ANDROID || UNITY_IOS
            this.gameObject.SetActive(true);
#else
            this.gameObject.SetActive(false);
#endif
        }
    }
}
