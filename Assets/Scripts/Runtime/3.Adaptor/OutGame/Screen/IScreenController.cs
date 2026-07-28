namespace KillChord.Runtime.Adaptor.OutGame.Screen
{
    /// <summary>
    ///     画面操作をユースケースへ伝達するためのインターフェース。
    /// </summary>
    /// <remarks>
    ///     各メソッドは遷移要求を受け付けた場合に true を返します。
    ///     トランジション再生中の連打は false で弾かれるため、
    ///     遷移完了を前提とした後続処理は戻り値を確認してから実行してください。
    /// </remarks>
    public interface IScreenController
    {

        /// <summary> タイトル画面を表示します。 </summary>
        bool ShowTitle();

        /// <summary> メニュー画面を表示します。 </summary>
        bool ShowMenu();

        /// <summary> オプション画面を表示します。 </summary>
        bool ShowOptions();

        /// <summary> クレジット画面を表示します。 </summary>
        bool ShowCredit();

        /// <summary> ホーム画面を表示します。 </summary>
        bool ShowHome();

        /// <summary> 作戦画面を表示します。 </summary>
        bool ShowStageSelect();

        /// <summary> 研究画面を表示します。 </summary>
        bool ShowSkillTree();

        /// <summary> 改造画面を表示します。 </summary>
        bool ShowSkillBuild();

        /// <summary> 戦闘準備画面を表示します。 </summary>
        bool ShowBattlePreparation(string targetSceneName);

        /// <summary> 設定画面を表示します。 </summary>
        bool ShowSetting();

        /// <summary> 現在画面を閉じます。 </summary>
        bool CloseCurrent();
    }
}
