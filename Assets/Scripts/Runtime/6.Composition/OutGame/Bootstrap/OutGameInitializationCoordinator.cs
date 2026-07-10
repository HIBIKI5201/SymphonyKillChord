using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.Composition.OutGame.Bootstrap
{
    /// <summary>
    ///     アウトゲーム初期化モジュールをフェーズ順に実行するCoordinatorです。
    /// </summary>
    public sealed class OutGameInitializationCoordinator
    {
        /// <summary>
        ///     モジュール一覧をInit→ResourceLoadAsync→Build→Readyの順で実行します。
        /// </summary>
        /// <param name="modules"> 実行対象モジュールです。 </param>
        /// <param name="progress"> 進捗通知先です。 </param>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        public async Awaitable<bool> InitializeAsync(
            IReadOnlyList<IOutGameInitializationModule> modules,
            IProgress<float> progress,
            CancellationToken cancellationToken)
        {
            progress?.Report(0f);

            if (modules == null || modules.Count == 0)
            {
                progress?.Report(1f);
                return true;
            }

            int completedStepCount = 0;
            int totalStepCount = modules.Count * PhaseCount;

            if (!RunInitPhase(modules, progress, totalStepCount, ref completedStepCount, cancellationToken))
            {
                return false;
            }

            if (!await RunResourceLoadPhaseAsync(modules, progress, totalStepCount, completedStepCount, cancellationToken))
            {
                return false;
            }
            completedStepCount += modules.Count;

            if (!RunBuildPhase(modules, progress, totalStepCount, ref completedStepCount, cancellationToken))
            {
                return false;
            }

            if (!RunReadyPhase(modules, progress, totalStepCount, ref completedStepCount, cancellationToken))
            {
                return false;
            }

            progress?.Report(1f);
            return true;
        }

        /// <summary>
        ///     Initフェーズを実行します。
        /// </summary>
        private bool RunInitPhase(
            IReadOnlyList<IOutGameInitializationModule> modules,
            IProgress<float> progress,
            int totalStepCount,
            ref int completedStepCount,
            CancellationToken cancellationToken)
        {
            for (int i = 0; i < modules.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                IOutGameInitializationModule module = modules[i];
                if (!module.Init())
                {
                    LogPhaseFailure(module, InitializationPhase.Init);
                    return false;
                }

                completedStepCount++;
                ReportProgress(progress, totalStepCount, completedStepCount);
            }

            return true;
        }

        /// <summary>
        ///     ResourceLoadAsyncフェーズを実行します。
        /// </summary>
        private async Awaitable<bool> RunResourceLoadPhaseAsync(
            IReadOnlyList<IOutGameInitializationModule> modules,
            IProgress<float> progress,
            int totalStepCount,
            int completedStepCount,
            CancellationToken cancellationToken)
        {
            for (int i = 0; i < modules.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                IOutGameInitializationModule module = modules[i];
                if (!await module.ResourceLoadAsync(cancellationToken))
                {
                    LogPhaseFailure(module, InitializationPhase.ResourceLoadAsync);
                    return false;
                }

                ReportProgress(progress, totalStepCount, completedStepCount + i + 1);
            }

            return true;
        }

        /// <summary>
        ///     Buildフェーズを実行します。
        /// </summary>
        private bool RunBuildPhase(
            IReadOnlyList<IOutGameInitializationModule> modules,
            IProgress<float> progress,
            int totalStepCount,
            ref int completedStepCount,
            CancellationToken cancellationToken)
        {
            for (int i = 0; i < modules.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                IOutGameInitializationModule module = modules[i];
                if (!module.Build())
                {
                    LogPhaseFailure(module, InitializationPhase.Build);
                    return false;
                }

                completedStepCount++;
                ReportProgress(progress, totalStepCount, completedStepCount);
            }

            return true;
        }

        /// <summary>
        ///     Readyフェーズを実行します。
        /// </summary>
        private bool RunReadyPhase(
            IReadOnlyList<IOutGameInitializationModule> modules,
            IProgress<float> progress,
            int totalStepCount,
            ref int completedStepCount,
            CancellationToken cancellationToken)
        {
            for (int i = 0; i < modules.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                IOutGameInitializationModule module = modules[i];
                if (!module.Ready())
                {
                    LogPhaseFailure(module, InitializationPhase.Ready);
                    return false;
                }

                completedStepCount++;
                ReportProgress(progress, totalStepCount, completedStepCount);
            }

            return true;
        }

        /// <summary>
        ///     フェーズ失敗ログを出力します。
        /// </summary>
        private static void LogPhaseFailure(
            IOutGameInitializationModule module,
            InitializationPhase phase)
        {
            Debug.LogError(
                $"[{nameof(OutGameInitializationCoordinator)}] " +
                $"{module.ModuleName} の {phase} フェーズに失敗しました。");
        }

        /// <summary>
        ///     現在進捗を通知します。
        /// </summary>
        private static void ReportProgress(
            IProgress<float> progress,
            int totalStepCount,
            int completedStepCount)
        {
            if (totalStepCount <= 0)
            {
                progress?.Report(1f);
                return;
            }

            progress?.Report((float)completedStepCount / totalStepCount);
        }

        private const int PhaseCount = 4;

        /// <summary>
        ///     初期化フェーズ種別です。
        /// </summary>
        private enum InitializationPhase
        {
            Init = 0,
            ResourceLoadAsync = 1,
            Build = 2,
            Ready = 3,
        }
    }
}
