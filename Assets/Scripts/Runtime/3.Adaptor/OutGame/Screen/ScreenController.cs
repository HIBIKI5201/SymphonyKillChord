using KillChord.Runtime.Application.OutGame.Screen;
using KillChord.Runtime.Domain.OutGame.Screen;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.Screen
{
    /// <summary>
    ///     画面操作をユースケースへ伝達する Controller。
    /// </summary>
    public sealed class ScreenController : IScreenController
    {
        /// <summary>
        ///     Controller を初期化します。
        /// </summary>
        public ScreenController(
            ShowScreenUseCase showScreenUseCase,
            CloseCurrentScreenUseCase closeCurrentScreenUseCase,
            ResetToHomeScreenUseCase resetToHomeScreenUseCase)
        {
            _showScreenUseCase = showScreenUseCase;
            _closeCurrentScreenUseCase = closeCurrentScreenUseCase;
            _resetToHomeScreenUseCase = resetToHomeScreenUseCase;
        }

        /// <summary>
        ///    タイトル画面を表示します。
        /// </summary>
        public void ShowTitle()
        {
            _showScreenUseCase.Execute(new ShowScreenCommand(ScreenId.Title));
        }

        /// <summary>
        ///    メニュー画面を表示します。
        /// </summary>
        public void ShowMenu()
        {
            _showScreenUseCase.Execute(new ShowScreenCommand(ScreenId.Menu));
        }

        /// <summary>
        ///     オプション画面を表示します。
        /// </summary>
        public void ShowOptions()
        {
            _showScreenUseCase.Execute(new ShowScreenCommand(ScreenId.Options));
        }

        /// <summary>
        ///    クレジット画面を表示します。
        /// </summary>
        public void ShowCredit()
        {
            _showScreenUseCase.Execute(new ShowScreenCommand(ScreenId.Credit));
        }

        /// <summary>
        ///    現在画面を即座に閉じます。
        /// </summary>
        public void CloseCurrentImmediately()
        {
            _closeCurrentScreenUseCase.Execute();
        }

        /// <summary>
        ///     ホーム画面を表示します。
        /// </summary>
        public async Task ShowHome(CancellationToken token)
        {
            await _resetToHomeScreenUseCase.Execute(token);
        }

        /// <summary>
        ///     作戦画面を表示します。
        /// </summary>
        public async Task ShowStageSelect(CancellationToken token)
        {
            await _showScreenUseCase.Execute(new ShowScreenCommand(ScreenId.StageSelect), token);
        }

        /// <summary>
        ///     研究画面を表示します。
        /// </summary>
        public async Task ShowSkillTree(CancellationToken token)
        {
            await _showScreenUseCase.Execute(new ShowScreenCommand(ScreenId.SkillTree), token);
        }

        /// <summary>
        ///     改造画面を表示します。
        /// </summary>
        public async Task ShowSkillBuild(CancellationToken token)
        {
            await _showScreenUseCase.Execute(new ShowScreenCommand(ScreenId.SkillBuild), token);
        }

        /// <summary>
        ///     戦闘準備画面を表示します。
        /// </summary>
        public async Task ShowBattlePreparation(string targetSceneName, CancellationToken token)
        {
            await _showScreenUseCase.Execute(
                new ShowScreenCommand(ScreenId.BattlePreparation, targetSceneName), token);
        }

        /// <summary>
        ///     設定画面を表示します。
        /// </summary>
        public async Task ShowSetting(CancellationToken token)
        {
            await _showScreenUseCase.Execute(new ShowScreenCommand(ScreenId.Setting), token);
        }

        /// <summary>
        ///     現在画面を閉じます。
        /// </summary>
        public async Task CloseCurrent(CancellationToken token)
        {
            await _closeCurrentScreenUseCase.Execute(token);
        }

        private readonly CloseCurrentScreenUseCase _closeCurrentScreenUseCase;
        private readonly ResetToHomeScreenUseCase _resetToHomeScreenUseCase;
        private readonly ShowScreenUseCase _showScreenUseCase;
    }
}
