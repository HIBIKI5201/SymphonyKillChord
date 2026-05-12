using KillChord.Runtime.Adaptor.InGame.Mission;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Mission
{
    /// <summary>
    ///     アウトゲームでミッションを選択するボタンのビュークラス。
    /// </summary>
    public class OutGameMissionButtonView : MonoBehaviour
    {
        /// <summary>
        ///     初期化処理を行います。
        /// </summary>
        /// <param name="controller">ミッション選択コントローラー。</param>
        public void Initialize(OutGameMissionSelectController controller)
        {
            _controller = controller;
        }

        /// <summary>
        ///     ボタンクリック時の処理を行います。
        /// </summary>
        public void OnClick()
        {
            if (_controller == null)
            {
                Debug.LogError("OutGameMissionSelectController が初期化されていません。");
                return;
            }

            if (string.IsNullOrWhiteSpace(_missionId))
            {
                Debug.LogError("MissionId が未設定です。");
                return;
            }

            _controller.Select(_missionId);
        }

        [SerializeField, Tooltip("このボタンで選択するミッションのID。")] private string _missionId;
        /// <summary> ミッション選択コントローラー。 </summary>
        private OutGameMissionSelectController _controller;
    }
}
