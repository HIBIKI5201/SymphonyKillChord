using KillChord.Runtime.Adaptor.InGame.Sequence;
using KillChord.Runtime.Composition.Persistent.Input;
using KillChord.Runtime.View.Persistent.Input;
using KillChord.Runtime.View.Persistent.Music;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Sequence
{
    /// <summary>
    ///     戦闘をポーズするモジュール。
    /// </summary>
    public class BattlePauseModule : IBattlePauseModule
    {
        public BattlePauseModule(MusicPlayer musicPlayer)
        {
            InputComposition inputComposition = ServiceLocator.GetInstance<InputComposition>();
            if (inputComposition == null || inputComposition.GetInputMapController == null)
            {
                Debug.LogError("[BattlePauseModule] InputComposition の取得に失敗しました。");
            }
            _inputMapController = inputComposition.GetInputMapController;
            _musicPlayer = musicPlayer;
        }

        /// <inheritdoc/>
        public bool TryPause()
        {
            if (_isPaused)
            {
                return false;
            }
            // UIと既存Optionを残し、決定・取消と同じボタンの攻撃・回避入力を停止する。
            _inputMapController.EnableOnly(InputMapNames.Common);
            _musicPlayer.PauseBGM();
            Time.timeScale = 0f;
            _isPaused = true;
            return true;
        }

        /// <inheritdoc/>
        public void Resume()
        {
            if (!_isPaused)
            {
                return;
            }
            _musicPlayer.ResumeBGM();
            // シナリオ終了経路もExitScenarioInputModeの後にここへ到達する。
            _inputMapController.EnableCommonWith(InputMapNames.InGame);
            Time.timeScale = 1f;
            _isPaused = false;
        }

        private bool _isPaused;
        private readonly UnityInputMapController _inputMapController;

        private MusicPlayer _musicPlayer;
    }
}
