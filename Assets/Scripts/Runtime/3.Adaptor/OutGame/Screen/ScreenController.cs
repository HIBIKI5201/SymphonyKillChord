using KillChord.Runtime.Application.OutGame.Screen;
using KillChord.Runtime.Domain.OutGame.Screen;

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
        public void ShowHome()
        {
            _resetToHomeScreenUseCase.Execute();
        }

        /// <summary>
        ///     作戦画面を表示します。
        /// </summary>
        public void ShowStageSelect()
        {
            _showScreenUseCase.Execute(new ShowScreenCommand(ScreenId.StageSelect));
        }

        /// <summary>
        ///     研究画面を表示します。
        /// </summary>
        public void ShowSkillTree()
        {
            _showScreenUseCase.Execute(new ShowScreenCommand(ScreenId.SkillTree));
        }

        /// <summary>
        ///     改造画面を表示します。
        /// </summary>
        public void ShowSkillBuild()
        {
            _showScreenUseCase.Execute(new ShowScreenCommand(ScreenId.SkillBuild));
        }

        /// <summary>
        ///     設定画面を表示します。
        /// </summary>
        public void ShowSetting()
        {
            _showScreenUseCase.Execute(new ShowScreenCommand(ScreenId.Setting));
        }

        /// <summary>
        ///     現在画面を閉じます。
        /// </summary>
        public void CloseCurrent()
        {
            _closeCurrentScreenUseCase.Execute();
        }

        private readonly CloseCurrentScreenUseCase _closeCurrentScreenUseCase;
        private readonly ResetToHomeScreenUseCase _resetToHomeScreenUseCase;
        private readonly ShowScreenUseCase _showScreenUseCase;
    }
}
