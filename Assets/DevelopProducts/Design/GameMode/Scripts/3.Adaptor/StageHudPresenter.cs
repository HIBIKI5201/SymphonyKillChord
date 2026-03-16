using DevelopProducts.Design.GameMode.Application;
using DevelopProducts.Design.GameMode.Domain;

namespace DevelopProducts.Design.GameMode.Adaptor
{
    /// <summary>
    ///     ステージのHUDを更新するクラス。
    ///     ステージの状態を監視し、プレイヤーのHPや経過時間、ゲームの結果などをHUDに反映させる役割を持つ。
    /// </summary>
    public class StageHudPresenter
    {
        public StageHudPresenter(StageRuntimeContext context,
            GameModeRuntime gameModeRuntime,
            StageHudViewModel viewModel)
        {
            _runtimeContext = context;
            _gameModeRuntime = gameModeRuntime;
            _viewModel = viewModel;
        }

        public void Present()
        {
            _viewModel.CurrentPlayerHp = _runtimeContext.PlayerRuntimeState.CurrentHp;
            _viewModel.MaxPlayerHp = _runtimeContext.PlayerRuntimeState.MaxHp;
            _viewModel.ElapsedTime = _runtimeContext.StageTimeState.ElapsedTime;
            _viewModel.ResultText = _gameModeRuntime.EndReason.ToString();

            if(_gameModeRuntime.IsFinished && _gameModeRuntime.EndReason == StageEndReason.Clear)
            {
                EvaluationResult result = _gameModeRuntime.BuildEvaluationResult();
                _viewModel.EvaluationCount = result.AchivedCount;
            }
            else
            {
                _viewModel.EvaluationCount = 0;
            }
        }

        private readonly StageRuntimeContext _runtimeContext;
        private readonly GameModeRuntime _gameModeRuntime;
        private readonly StageHudViewModel _viewModel;
    }
}
