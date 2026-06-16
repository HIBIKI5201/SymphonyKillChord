using KillChord.Runtime.View.OutGame.SkillBuild;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.Composition.OutGame.SkillBuild
{
    /// <summary>
    ///    スキルビルド画面の初期化を担当する MonoBehaviour クラス。
    /// </summary>
    [DefaultExecutionOrder(1)]
    public class SkillBuildInitializer : MonoBehaviour
    {
        [SerializeField, Tooltip("アウトゲームの UIDocument")] 
        private UIDocument _uiDocument;

        private void Awake()
        {
            if (_uiDocument == null)
            {
#if UNITY_EDITOR
                Debug.LogError("UIDocument がアサインされていません。Inspector で UIDocument をアサインしてください。");
#endif
                return;
            }

            SkillElementDragAndDropSetup dragAndDropSetup = new(_uiDocument);
        }
    }
}
