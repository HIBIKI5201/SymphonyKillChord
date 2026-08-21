using System;

namespace KillChord.Runtime.View.OutGame.Screen
{
    /// <summary>
    ///     アウトゲーム UI のイベントを管理する Signal です。
    /// </summary>
    public sealed class OutGameUIEvent
    {
        /// <summary> タイトル画面を表示するイベント。 </summary>
        public Action OnShowTitleScreen;

        /// <summary> メニュー画面を表示するイベント。 </summary>
        public Action OnShowMenuScreen;

        /// <summary> オプション画面を表示するイベント。 </summary>
        public Action OnShowOptionsScreen;

        /// <summary> クレジット画面を表示するイベント。 </summary>
        public Action OnShowCreditScreen;

        /// <summary> データリセットボタンが押されたときのイベント。 </summary>
        public Action OnDataResetButtonClicked;

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

        /// <summary> 戦闘準備画面を表示するイベントです。 </summary>
        public Action OnShownBattlePreparationScreen;

        /// <summary> 設定画面を表示するイベント。 </summary>
        public Action OnShownSettingScreen;

        /// <summary> 画面を閉じるイベント。 </summary>
        public Action OnScreenClosed;

        /// <summary> ステージノードが選択されたときのイベントです。 </summary>
        public Action<int> OnStageNodeSelected;

        /// <summary> ステージ詳細画面を閉じるイベント。 </summary>
        public Action OnStageDetailClosed;

        /// <summary> スキルノードが選択された時のイベント。 </summary>
        public Action<string> OnSkillNodeSelected;

        /// <summary> スキルノードが解放された時のイベント。 </summary>
        public Action OnSkillUnlocked;

        /// <summary> スキルツリーのリセット確認表示を要求するイベント。 </summary>
        public Action OnSkillTreeResetRequested;

        /// <summary> スキルツリーのリセットを確定するイベント。 </summary>
        public Action OnSkillTreeResetConfirmed;

        /// <summary> スキルツリーのリセットをキャンセルするイベント。 </summary>
        public Action OnSkillTreeResetCancelled;

        /// <summary> 入手済みスキル一覧が更新された時のイベント。 </summary>
        public Action OnOwnedSkillChanged;

        /// <summary> スキル詳細画面を閉じるイベント。 </summary>
        public Action<int> OnSkillDetailClosed;

        /// <summary> スキルプレビュー動画ボタンをクリックした時のイベント。 </summary>
        public Action OnSkillPreviewButtonClicked;

        /// <summary> スキルプレビュー動画の閉じるボタンをクリックした時のイベント。 </summary>
        public Action OnSkillPreviewCloseButtonClicked;

        /// <summary> ステージクリアを通知するイベント。 </summary>
        public Action<int> OnStageCleared;

        /// <summary> 出撃ボタンが押されたことを通知するイベント。 </summary>
        public Action OnSortieRequested;

        /// <summary> 作戦画面の表示アニメーションが完了したことを通知するイベント。 </summary>
        public Action OnStageSelectScreenCompleted;

        /// <summary> スキルレベルアップが行われたときのイベント。 </summary>
        public Action OnSkillLevelUp;

        /// <summary> 選択中のバトルステージへ遷移するイベントです。 </summary>
        public Action OnStartGame;
    }
}
