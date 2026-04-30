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
