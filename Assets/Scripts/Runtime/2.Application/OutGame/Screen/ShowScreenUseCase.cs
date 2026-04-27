using KillChord.Runtime.Domain.OutGame.Screen;

namespace KillChord.Runtime.Application.OutGame.Screen
{
    /// <summary>
    ///     指定された画面を表示するユースケース。
    /// </summary>
    public sealed class ShowScreenUseCase
    {
        /// <summary>
        ///     ユースケースを初期化します。
        /// </summary>
        public ShowScreenUseCase(
            IScreenStateRepository screenStateRepository,
            IScreenRuleRepository screenRuleRepository,
            IScreenPresenter screenPresenter)
        {
            _screenStateRepository = screenStateRepository;
            _screenRuleRepository = screenRuleRepository;
            _screenPresenter = screenPresenter;
        }

        /// <summary>
        ///     指定された画面を表示します。
        /// </summary>
        public void Execute(ShowScreenCommand command)
        {
            ScreenTransitionState transitionState = _screenStateRepository.TransitionState;
            ScreenId? previousScreenId = transitionState.CurrentScreenId;
            ScreenTransitionRule rule = _screenRuleRepository.GetRule(command.TargetScreenId);

            if (rule.TransitionType == ScreenTransitionType.Reset)
            {
                transitionState.Reset(command.TargetScreenId);
            }
            else
            {
                transitionState.MoveTo(command.TargetScreenId, rule.KeepInHistory);
            }

            bool hidePreviousScreen = rule.TransitionType == ScreenTransitionType.Replace ||
                rule.TransitionType == ScreenTransitionType.Reset;

            ScreenTransitionResult result = new(
                hidePreviousScreen ? previousScreenId : null,
                command.TargetScreenId,
                rule.TransitionType == ScreenTransitionType.Reset);

            _screenPresenter.Present(result);
        }

        private readonly IScreenPresenter _screenPresenter;
        private readonly IScreenRuleRepository _screenRuleRepository;
        private readonly IScreenStateRepository _screenStateRepository;
    }
}
