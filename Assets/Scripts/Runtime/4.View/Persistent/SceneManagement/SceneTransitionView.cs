using KillChord.Runtime.Adaptor.Persistent.SceneManagement;
using SymphonyFrameWork.Attribute;
using UnityEngine;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     デバッグ用のボタンなどにつけるシーン遷移のView。
    /// </summary>
    public class SceneTransitionView : MonoBehaviour
    {
        /// <summary>
        ///     シーン遷移Controllerを初期化する。
        /// </summary>
        /// <param name="controller"> シーン遷移Controller。 </param>
        public void Initialize(SceneTransitionController controller)
        {
            _controller = controller;
        }

        /// <summary>
        ///     シーン遷移を実行する。
        /// </summary>
        public async void ChangeScene()
        {
            bool success = await _controller.ChangeSceneAsync(
                _fromSceneName,
                _toSceneName,
                default);

            if (!success)
            {
                Debug.LogError($"シーン遷移失敗: {_fromSceneName} -> {_toSceneName}");
            }
            else
            {
                Debug.Log($"シーン遷移成功: {_fromSceneName} -> {_toSceneName}");
            }
        }

        [SerializeField, SceneNameSelector] private string _fromSceneName;
        [SerializeField, SceneNameSelector] private string _toSceneName;

        private SceneTransitionController _controller;
    }
}
