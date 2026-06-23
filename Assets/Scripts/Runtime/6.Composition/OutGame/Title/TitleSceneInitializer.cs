using KillChord.Runtime.Adaptor.OutGame.Title;
using KillChord.Runtime.Adaptor.Persistent.SceneManagement;
using KillChord.Runtime.View.OutGame.Title;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.Composition.OutGame.Title
{
    /// <summary>
    ///     タイトルシーンの初期化を行うクラス。
    /// </summary>
    public class TitleSceneInitializer : MonoBehaviour
    {
        [SerializeField, Tooltip("UI Document")]
        private UIDocument _uiDocument;

        [SerializeField, Tooltip("タイトルシーンの View")]
        private TitleSceneView _titleSceneView;

        private void Start()
        {
            if (_uiDocument == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: UI Documentが設定されていません。");
#endif
                return;
            }

            if (_titleSceneView == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: TitleSceneViewが設定されていません。");
#endif
                return;
            }

            if (!ServiceLocator.TryGetInstance(out SceneTransitionController sceneTransitionController))
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: SceneTransitionControllerがServiceLocatorに登録されていません。");
#endif
                return;
            }

            var root = _uiDocument.rootVisualElement;
            if (root == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"{nameof(TitleSceneInitializer)}: Root VisualElementがnullです。");
#endif
                return;
            }

            TitleStartController titleStartController = new TitleStartController(sceneTransitionController);

            _titleSceneView.Initialize(root, titleStartController);
        }
    }
}
