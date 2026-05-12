using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.Screen
{
    /// <summary>
    ///     画面操作をユースケースへ伝達するためのインターフェース。
    /// </summary>
    public interface IScreenController
    {
        /// <summary> ホーム画面を表示します。 </summary>
        Task ShowHome(CancellationToken token);

        /// <summary> 作戦画面を表示します。 </summary>
        Task ShowStageSelect(CancellationToken token);

        /// <summary> 研究画面を表示します。 </summary>
        Task ShowSkillTree(CancellationToken token);

        /// <summary> 改造画面を表示します。 </summary>
        Task ShowSkillBuild(CancellationToken token);

        /// <summary> 設定画面を表示します。 </summary>
        Task ShowSetting(CancellationToken token);

        /// <summary> 現在画面を閉じます。 </summary>
        Task CloseCurrent(CancellationToken token);
    }
}
