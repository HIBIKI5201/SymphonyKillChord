using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Adaptor.InGame.Stage;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Composition.InGame.Bootstrap;
using KillChord.Runtime.Composition.InGame.Enemy;
using KillChord.Runtime.Composition.InGame.Music;
using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Stage;
using KillChord.Runtime.View.InGame.Stage;
using SymphonyFrameWork.System.ServiceLocate;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Stage
{
    /// <summary>
    ///     Wave開始通知を音楽同期したステージ演出へ接続します。
    /// </summary>
    public sealed class StageEffectInitializer : InGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(StageEffectInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 800;

        /// <summary>
        ///     ステージ演出ViewとPresenterを構築します。
        /// </summary>
        /// <returns> 構築に成功した場合はtrueです。 </returns>
        public override bool Build()
        {
            _viewModel = new StageEffectViewModel();
            _presenter = new StageEffectPresenter(_viewModel);
            _views = FindObjectsByType<StageEffectView>(FindObjectsSortMode.None);

            for (int i = 0; i < _views.Length; i++)
            {
                _views[i]?.Initialize(_viewModel);
            }

            return true;
        }

        /// <summary>
        ///     Wave開始通知と音楽同期サービスを結合します。
        /// </summary>
        /// <returns> 結合に成功した場合はtrueです。 </returns>
        public override bool Ready()
        {
            EnemyModuleContainer enemyContainer =
                ServiceLocator.GetInstance<EnemyModuleContainer>();
            MusicSyncModuleContainer musicContainer =
                ServiceLocator.GetInstance<MusicSyncModuleContainer>();
            if (enemyContainer?.EnemyWaveSpawnerState == null
                || musicContainer?.MusicSyncService == null
                || musicContainer.MusicSyncState == null)
            {
                Debug.LogError(
                    $"[{nameof(StageEffectInitializer)}] 必要なContainerを取得できませんでした。",
                    this);
                return false;
            }

            _waveSpawnerState = enemyContainer.EnemyWaveSpawnerState;
            _musicScheduler = new MusicSchedulerAdaptor(
                musicContainer.MusicSyncState,
                musicContainer.MusicSyncService);
            _waveSpawnerState.OnWaveStarted += WaveStartedHandler;
            return true;
        }

        /// <summary>
        ///     イベント購読とViewModel接続を解除します。
        /// </summary>
        public override void Shutdown()
        {
            if (_waveSpawnerState != null)
            {
                _waveSpawnerState.OnWaveStarted -= WaveStartedHandler;
                _waveSpawnerState = null;
            }

            if (_views != null)
            {
                for (int i = 0; i < _views.Length; i++)
                {
                    _views[i]?.Shutdown();
                }
            }

            _views = null;
            _musicScheduler = null;
            _presenter = null;
            _viewModel = null;
        }

        private StageEffectViewModel _viewModel;
        private StageEffectPresenter _presenter;
        private StageEffectView[] _views;
        private EnemyWaveSpawnerState _waveSpawnerState;
        private IMusicActionScheduler _musicScheduler;

        /// <summary>
        ///     Wave定義に登録されたステージ演出を音楽同期で予約します。
        /// </summary>
        /// <param name="waveIndex"> 開始したWaveインデックスです。 </param>
        /// <param name="waveDefinition"> 開始したWave定義です。 </param>
        private void WaveStartedHandler(
            int waveIndex,
            EnemyWaveDefinition waveDefinition)
        {
            IReadOnlyList<IStageEffectDefinition> stageEffects =
                waveDefinition.StageEffects;
            for (int i = 0; i < stageEffects.Count; i++)
            {
                IStageEffectDefinition stageEffect = stageEffects[i];
                if (stageEffect == null)
                {
                    continue;
                }

                IStageEffectDefinition scheduledEffect = stageEffect;
                _musicScheduler.Schedule(
                    scheduledEffect.MusicSpec,
                    () => _presenter.Present(scheduledEffect),
                    destroyCancellationToken);
            }
        }
    }
}
