using SymphonyFrameWork.System.ServiceLocate;
using System;
using UnityEngine;

namespace KillChord.Runtime.View.OutGame.Screen
{
    /// <summary>
    ///     アウトゲーム UI のイベントを管理するクラス。
    ///     イベント管理クラスのため、他のクラスよりも早く初期化されるように
    ///     DefaultExecutionOrder を -100 に設定しています。
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class OutGameUIEvent : MonoBehaviour
    {
        /// <summary> ホーム画面を表示するイベント。 </summary>
        public Action OnShownHomeScreen;

        /// <summary> 作戦画面を表示するイベント。 </summary>
        public Action OnShownStageSelectionScreen;

        /// <summary> 研究画面を表示するイベント。 </summary>
        public Action OnShownSkillTreeScreen;

        /// <summary> 改造画面を表示するイベント。 </summary>
        public Action OnShownSkillBuildScreen;

        /// <summary> 戦闘準備画面を表示するイベント。 </summary>
        public Action OnShownBattlePreparationScreen;

        /// <summary> 設定画面を表示するイベント。 </summary>
        public Action OnShownSettingScreen;

        /// <summary> 画面を閉じるイベント。 </summary>
        public Action OnScreenClosed;

        /// <summary> ステージノードが選択されたときのイベント。選択されたステージのIDを整数で通知します。 </summary>
        public Action<int> OnStageNodeSelected;

        /// <summary> ステージ詳細画面を閉じるイベント。 </summary>
        public Action OnStageDetailClosed;

        /// <summary> ステージクリアを通知するイベント。クリアしたステージのIDを整数で通知します。 </summary>
        public Action<int> OnStageCleared;

        /// <summary> 
        ///     インゲームへ遷移するイベント。
        ///     TODO : 遷移先のステージを指定できるようにする（OnStageNodeSelected で選択されたステージのIDを引数で受け取るなど）。
        /// </summary>
        public Action OnStartGame;

        /// <summary>
        ///     アウトゲームのUIイベントを ServiceLocator に登録します。
        /// </summary>
        public void RegisterOutGameUIEvent()
        {
            if (_isRegistered) { return; }
            ServiceLocator.RegisterInstance(this);
            _isRegistered = true;
        }

        /// <summary>
        ///     アウトゲームのUIイベントを ServiceLocator から登録解除します。
        /// </summary>
        public void UnregisterOutGameUIEvent()
        {
            if (!_isRegistered) { return; }
            ServiceLocator.UnregisterInstance(this);
            _isRegistered = false;
        }

        /// <summary>
        ///     ServiceLocator にインスタンスを登録する。
        /// </summary>
        private void Awake()
        {
            RegisterOutGameUIEvent();
        }

        /// <summary>
        ///     ServiceLocator からインスタンスを登録解除する。
        ///     自身で登録解除を呼び出すとエラーが起きるため、コメントアウトしています。
        /// </summary>
        private void OnDestroy()
        {
            //UnregisterOutGameUIEvent();
        }

        private bool _isRegistered;
    }
}
