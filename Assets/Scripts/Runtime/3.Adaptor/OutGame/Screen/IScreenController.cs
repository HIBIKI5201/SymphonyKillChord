using System.Threading;
using System.Threading.Tasks;

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

        /// <summary> 現在画面を即座に閉じます。 </summary>
        void CloseCurrentImmediately();

        /// <summary> ホーム画面を表示します。 </summary>
        Task ShowHome(CancellationToken token);

        /// <summary> 作戦画面を表示します。 </summary>
        Task ShowStageSelect(CancellationToken token);

        /// <summary> 研究画面を表示します。 </summary>
        Task ShowSkillTree(CancellationToken token);

        /// <summary> 改造画面を表示します。 </summary>
        Task ShowSkillBuild(CancellationToken token);

        /// <summary> 戦闘準備画面を表示します。 </summary>
        Task ShowBattlePreparation(string targetSceneName, CancellationToken token);

        /// <summary> 設定画面を表示します。 </summary>
        Task ShowSetting(CancellationToken token);

        /// <summary> 現在画面を閉じます。 </summary>
        Task CloseCurrent(CancellationToken token);
    }
}
