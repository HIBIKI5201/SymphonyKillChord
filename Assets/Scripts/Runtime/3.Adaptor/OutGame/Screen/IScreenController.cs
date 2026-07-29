namespace KillChord.Runtime.Adaptor.OutGame.Screen
{
    /// <summary>
    ///     画面操作をユースケースへ伝達するためのインターフェース。
    /// </summary>
    public interface IScreenController
    {

        /// <summary> タイトル画面を表示します。 </summary>
        void ShowTitle();

        /// <summary> メニュー画面を表示します。 </summary>
        void ShowMenu();

        /// <summary> オプション画面を表示します。 </summary>
        void ShowOptions();

        /// <summary> クレジット画面を表示します。 </summary>
        void ShowCredit();

        /// <summary> ホーム画面を表示します。 </summary>
        void ShowHome();

        /// <summary> 作戦画面を表示します。 </summary>
        void ShowStageSelect();

        /// <summary> 研究画面を表示します。 </summary>
        void ShowSkillTree();

        /// <summary> 改造画面を表示します。 </summary>
        void ShowSkillBuild();

        /// <summary> 戦闘準備画面を表示します。 </summary>
        void ShowBattlePreparation(string targetSceneName);

        /// <summary> 設定画面を表示します。 </summary>
        void ShowSetting();

        /// <summary> 現在画面を閉じます。 </summary>
        void CloseCurrent();
    }
}
