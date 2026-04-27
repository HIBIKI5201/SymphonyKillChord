namespace KillChord.Runtime.Adaptor.OutGame.Screen
{
    /// <summary>
    ///     画面操作をユースケースへ伝達するためのインターフェース。
    /// </summary>
    public interface IScreenController
    {
        /// <summary> ホーム画面を表示します。 </summary>
        void ShowHome();

        /// <summary> ステージ選択画面を表示します。 </summary>
        void ShowStageSelect();

        /// <summary> スキルツリー画面を表示します。 </summary>
        void ShowSkillTree();

        /// <summary> スキル選択画面を表示します。 </summary>
        void ShowSkillBuild();

        /// <summary> 設定画面を表示します。 </summary>
        void ShowSetting();

        /// <summary> 現在画面を閉じます。 </summary>
        void CloseCurrent();
    }
}
