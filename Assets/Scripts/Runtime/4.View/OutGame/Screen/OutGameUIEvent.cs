using SymphonyFrameWork.System.ServiceLocate;
using System;
using UnityEngine;

namespace KillChord.Runtime.View.OutGame.Screen
{
    /// <summary>
    ///     アウトゲーム UI のイベントを管理するクラス。
    ///     イベント管理クラスのため、他のクラスよりも早く初期化されるように
    ///     DefaultExecutionOrder を -200 に設定しています。
    /// </summary>
    [DefaultExecutionOrder(-200)]
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

        /// <summary> OutGame UIの表示状態を切り替えるイベント。 </summary>
        public Action<bool> OnOutGameUiVisibilityChanged;

        /// <summary> 
        ///     戦闘準備画面を表示するイベント。
        ///     引数は遷移先のシーン名。
        /// </summary>
        public Action<string> OnShownBattlePreparationScreen;

        /// <summary> 設定画面を表示するイベント。 </summary>
        public Action OnShownSettingScreen;

        /// <summary> 画面を閉じるイベント。 </summary>
        public Action OnScreenClosed;

        /// <summary> ステージノードが選択されたときのイベント。選択されたステージのIDを整数で通知します。 </summary>
        public Action<int> OnStageNodeSelected;

        /// <summary> ステージ詳細画面を閉じるイベント。 </summary>
        public Action OnStageDetailClosed;

        /// <summary> スキルノードが選択された時のイベント。 </summary>
        public Action<string> OnSkillNodeSelected;

        /// <summary> スキルノードが選択された時のイベント。 </summary>
        public Action OnSkillUnlocked;

        /// <summary> スキル詳細画面を閉じるイベント。 </summary>
        public Action<int> OnSkillDetailClosed;

        /// <summary> スキルプレビュー動画ボタンをクリックした時のイベント。 </summary>
        public Action OnSkillPreviewButtonClicked;

        /// <summary> スキルプレビュー動画の閉じるボタンをクリックした時のイベント。 </summary>
        public Action OnSkillPreviewCloseButtonClicked;

        /// <summary> ステージクリアを通知するイベント。クリアしたステージのIDを整数で通知します。 </summary>
        public Action<int> OnStageCleared;

        /// <summary> 出撃ボタンが押されたことを通知するイベント。 </summary>
        public Action OnSortieRequested;

        /// <summary> 作戦画面の表示アニメーションが完了したことを通知するイベント。 </summary>
        public Action OnStageSelectScreenCompleted;

        /// <summary> スキル編成が保存されたときのイベント。 </summary>
        public Action OnSkillBuildSaved;

        /// <summary> スキルレベルアップが行われたときのイベント。 </summary>
        public Action OnSkillLevelUp;

        /// <summary> 
        ///     インゲームへ遷移するイベント。
        ///     引数は遷移先のシーン名。
        /// </summary>
        public Action<string> OnStartGame;

        /// <summary>
        ///     アウトゲームのUIイベントを ServiceLocator に登録します。
        /// </summary>
        public void RegisterOutGameUIEvent()
        {
            if (_isRegistered) { return; }
            ServiceLocator.RegisterInstance(this, LocateType.Locator);
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
