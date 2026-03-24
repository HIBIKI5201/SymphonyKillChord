using DevelopProducts.Design.GameMode.Adaptor;
using DevelopProducts.Design.GameMode.Application;
using UnityEngine;

namespace DevelopProducts.Design.GameMode.View
{
    /// <summary>
    ///     ステージのメインループを管理するViewクラス。
    ///     UnityのUpdateメソッドを利用して、ゲームの進行に合わせて時間の更新やHUDの表示を行う役割を持つ。
    /// </summary>
    public class StageLoopView : MonoBehaviour
    {
        public void Initialize(
            AdvanceTimeUsecase advanceTimeUsecase,
            GameModeRuntime gameModeRuntime,
            StageHudPresenter hudPresenter,
            StageHudViewModel hudViewModel,
            InGameHudView inGameHudView)
        {
            _advanceTimeUsecase = advanceTimeUsecase;
            _gameModeRuntime = gameModeRuntime;
            _hudPresenter = hudPresenter;
            _hudViewModel = hudViewModel;
            _inGameHudView = inGameHudView;
        }

        private AdvanceTimeUsecase _advanceTimeUsecase;
        private GameModeRuntime _gameModeRuntime;
        private StageHudPresenter _hudPresenter;
        private StageHudViewModel _hudViewModel;
        private InGameHudView _inGameHudView;

        private void Update()
        {
            if (!_gameModeRuntime.IsFinished)
            {
                _advanceTimeUsecase.Execute(Time.deltaTime);
                _gameModeRuntime.Tick();
                _hudPresenter.Present();
            }

            _inGameHudView.View(_hudViewModel);
        }
    }
}
