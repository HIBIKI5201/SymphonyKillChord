using KillChord.Runtime.Adaptor.InGame.Mission;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Mission
{
    /// <summary>
    ///     ミッションの定期更新を処理するビュークラス。
    /// </summary>
    public class MissionLoopView : MonoBehaviour
    {
        /// <summary>
        ///     初期化処理を行います。
        /// </summary>
        /// <param name="missionEventController">ミッションイベントコントローラー。</param>
        public void Initialize(MissionEventController missionEventController)
        {
            _missionEventController = missionEventController;
        }

        /// <summary>
        ///     UnityのUpdateメソッド。
        /// </summary>
        private void Update()
        {
            _missionEventController?.Tick(Time.deltaTime);
        }

        /// <summary> ミッションイベントコントローラー。 </summary>
        private MissionEventController _missionEventController;
    }
}
