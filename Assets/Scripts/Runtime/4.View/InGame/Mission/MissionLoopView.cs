using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.View.InGame.Sequence;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Mission
{
    /// <summary>
    ///     ミッションの定期更新を処理するビュークラス。
    /// </summary>
    public class MissionLoopView : MonoBehaviour, IGameplayControllable
    {
        /// <summary>
        ///     初期化処理を行います。
        /// </summary>
        /// <param name="missionEventController">ミッションイベントコントローラー。</param>
        public void Initialize(MissionEventController missionEventController)
        {
            _missionEventController = missionEventController;
        }

        /// <summary> ゲームプレイを開始します。 </summary>
        public void StartGameplay() => _isPlaying = true;

        /// <summary> ゲームプレイを停止します。 </summary>
        public void StopGameplay() => _isPlaying = false;

        /// <summary>
        ///     UnityのUpdateメソッド。
        /// </summary>
        private void Update()
        {
            if (!_isPlaying)
            {
                return;
            }

            _missionEventController?.Tick(Time.deltaTime);
        }

        /// <summary> ミッションイベントコントローラー。 </summary>
        private MissionEventController _missionEventController;
        private bool _isPlaying;
    }
}
