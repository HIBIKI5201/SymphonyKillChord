using KillChord.Runtime.Adaptor.OutGame.Scenario;
using KillChord.Runtime.Adaptor.OutGame.Sortie;
using KillChord.Runtime.Adaptor.OutGame.StageSelect;
using KillChord.Runtime.Adaptor.Persistent.SceneManagement;
using KillChord.Runtime.Application.OutGame.Sortie;
using KillChord.Runtime.Application.Persistent.Load;
using KillChord.Runtime.Application.Persistent.SceneManagement;
using KillChord.Runtime.Composition.OutGame.Bootstrap;
using KillChord.Runtime.Composition.OutGame.StageSelect;
using KillChord.Runtime.Composition.Persistent.Input;
using KillChord.Runtime.Composition.Persistent.SceneManagement;
using KillChord.Runtime.View.OutGame.Screen;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.OutGame.Sortie
{
    /// <summary>
    ///     アウトゲームの出撃機能の初期化を行うクラス。
    /// </summary>
    public sealed class OutGameSortieInitializer : OutGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(OutGameSortieInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 20;

        /// <summary>
        ///     出撃機能の依存を解決して登録します。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Build()
        {
            if (!ServiceLocator.TryGetInstance(out OutGameUIEvent outGameUIEvent))
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(OutGameSortieInitializer)}] OutGameUIEvent が取得できませんでした。", this);
#endif
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out InputComposition inputComposition))
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(OutGameSortieInitializer)}] InputComposition が取得できませんでした。", this);
#endif
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out SceneTransitionUsecase sceneTransitionUseCase))
            {
#if UNITY_EDITOR
                Debug.LogError(
                    $"[{nameof(OutGameSortieInitializer)}] " +
                    $"{nameof(SceneTransitionUsecase)}が取得できませんでした。",
                    this);
#endif
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out SceneTransitionController transition)
                || !ServiceLocator.TryGetInstance<ILoadingOperationExecutor>(out var executor)
                || !ServiceLocator.TryGetInstance(out SceneTransitionInitializer transitionInitializer))
            {
                Debug.LogError($"[{nameof(OutGameSortieInitializer)}] 常駐出撃サービスを取得できませんでした。", this);
                return false;
            }

            _outputPort = new OutGameSortieOutputPort(
                outGameUIEvent, inputComposition, transition, executor, transitionInitializer);
            IOutGameSortieOutputPort outputPort = _outputPort;
            OutGameSortieUseCase useCase =
                new OutGameSortieUseCase(
                    sceneTransitionUseCase,
                    outputPort,
                    destroyCancellationToken);

            OutGameSortieController controller = new OutGameSortieController(useCase);

            // ステージ詳細画面の出撃ボタンを押した時の処理を外部で呼び出せるようにするため、ServiceLocatorに登録しておく。
            return ServiceLocator.RegisterInstance(controller);
        }

        /// <summary>
        ///     全Build終了後にStageSelectが公開したStateと選択準備を接続します。
        /// </summary>
        public override bool Ready()
        {
            if (_outputPort == null
                || !ServiceLocator.TryGetInstance(out PendingNodeTransitionState pendingState)
                || !ServiceLocator.TryGetInstance(out SelectedScenarioState selectedState))
            {
                Debug.LogError($"[{nameof(OutGameSortieInitializer)}] 専用出撃のStateを取得できませんでした。", this);
                return false;
            }
            _outputPort.ConnectScenarioBattleSortie(pendingState, selectedState, BattleSortieSelectionStateResolver.CreateSelectionService());
            return true;
        }

        /// <summary>
        ///     登録したコントローラーをServiceLocatorから登録解除します。
        /// </summary>
        public override void Shutdown()
        {
            ServiceLocator.UnregisterInstance<OutGameSortieController>();
            _outputPort = null;
        }

        private OutGameSortieOutputPort _outputPort;
    }
}
