using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.Composition.Bootstrap
{
    /// <summary>
    ///     初期化モジュールをフェーズ順に実行するCoordinatorの共通実装です。
    /// </summary>
    /// <typeparam name="TModule"> 実行する初期化モジュール型です。 </typeparam>
    public abstract class InitializationCoordinator<TModule>
        where TModule : IInitializationModule
    {
        /// <summary> ログに表示するCoordinator名です。 </summary>
        protected abstract string CoordinatorName { get; }

        /// <summary>
        ///     モジュール一覧をInit→ResourceLoadAsync→Build→Readyの順で実行します。
        /// </summary>
        /// <param name="modules"> 実行対象モジュールです。 </param>
        /// <param name="progress"> 進捗通知先です。 </param>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        public async Awaitable<bool> InitializeAsync(
            IReadOnlyList<TModule> modules,
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

            if (!await RunResourceLoadPhaseAsync(
                    modules,
                    progress,
                    totalStepCount,
                    completedStepCount,
                    cancellationToken))
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

        private const int PhaseCount = 4;

        /// <summary>
        ///     Initフェーズを実行します。
        /// </summary>
        /// <param name="modules"> 実行対象モジュールです。 </param>
        /// <param name="progress"> 進捗通知先です。 </param>
        /// <param name="totalStepCount"> 総ステップ数です。 </param>
        /// <param name="completedStepCount"> 完了ステップ数です。 </param>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        private bool RunInitPhase(
            IReadOnlyList<TModule> modules,
            IProgress<float> progress,
            int totalStepCount,
            ref int completedStepCount,
            CancellationToken cancellationToken)
        {
            for (int i = 0; i < modules.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                TModule module = modules[i];
                if (!RunSynchronousPhase(module, InitializationPhase.Init, module.Init))
                {
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
        /// <param name="modules"> 実行対象モジュールです。 </param>
        /// <param name="progress"> 進捗通知先です。 </param>
        /// <param name="totalStepCount"> 総ステップ数です。 </param>
        /// <param name="completedStepCount"> 完了済みステップ数です。 </param>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        private async Awaitable<bool> RunResourceLoadPhaseAsync(
            IReadOnlyList<TModule> modules,
            IProgress<float> progress,
            int totalStepCount,
            int completedStepCount,
            CancellationToken cancellationToken)
        {
            for (int i = 0; i < modules.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                TModule module = modules[i];
                bool success;
                try
                {
                    success = await module.ResourceLoadAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception)
                {
                    LogPhaseFailure(module, InitializationPhase.ResourceLoadAsync);
                    throw;
                }

                if (!success)
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
        /// <param name="modules"> 実行対象モジュールです。 </param>
        /// <param name="progress"> 進捗通知先です。 </param>
        /// <param name="totalStepCount"> 総ステップ数です。 </param>
        /// <param name="completedStepCount"> 完了ステップ数です。 </param>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        private bool RunBuildPhase(
            IReadOnlyList<TModule> modules,
            IProgress<float> progress,
            int totalStepCount,
            ref int completedStepCount,
            CancellationToken cancellationToken)
        {
            for (int i = 0; i < modules.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                TModule module = modules[i];
                if (!RunSynchronousPhase(module, InitializationPhase.Build, module.Build))
                {
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
        /// <param name="modules"> 実行対象モジュールです。 </param>
        /// <param name="progress"> 進捗通知先です。 </param>
        /// <param name="totalStepCount"> 総ステップ数です。 </param>
        /// <param name="completedStepCount"> 完了ステップ数です。 </param>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        private bool RunReadyPhase(
            IReadOnlyList<TModule> modules,
            IProgress<float> progress,
            int totalStepCount,
            ref int completedStepCount,
            CancellationToken cancellationToken)
        {
            for (int i = 0; i < modules.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                TModule module = modules[i];
                if (!RunSynchronousPhase(module, InitializationPhase.Ready, module.Ready))
                {
                    return false;
                }

                completedStepCount++;
                ReportProgress(progress, totalStepCount, completedStepCount);
            }

            return true;
        }

        /// <summary>
        ///     同期フェーズを実行し、falseと例外のどちらでも失敗箇所を記録します。
        /// </summary>
        /// <param name="module"> 実行対象のモジュールです。 </param>
        /// <param name="phase"> 実行するフェーズです。 </param>
        /// <param name="operation"> フェーズの処理です。 </param>
        /// <returns> 成功した場合はtrueです。 </returns>
        private bool RunSynchronousPhase(TModule module, InitializationPhase phase, Func<bool> operation)
        {
            try
            {
                bool success = operation();
                if (!success)
                {
                    LogPhaseFailure(module, phase);
                }

                return success;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                LogPhaseFailure(module, phase);
                throw;
            }
        }

        /// <summary>
        ///     フェーズ失敗ログを出力します。
        /// </summary>
        /// <param name="module"> 失敗したモジュールです。 </param>
        /// <param name="phase"> 失敗したフェーズです。 </param>
        private void LogPhaseFailure(TModule module, InitializationPhase phase)
        {
            Debug.LogError(
                $"[{CoordinatorName}] {module.ModuleName} の {phase} フェーズに失敗しました。");
        }

        /// <summary>
        ///     現在進捗を通知します。
        /// </summary>
        /// <param name="progress"> 進捗通知先です。 </param>
        /// <param name="totalStepCount"> 総ステップ数です。 </param>
        /// <param name="completedStepCount"> 完了ステップ数です。 </param>
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

        private enum InitializationPhase
        {
            Init = 0,
            ResourceLoadAsync = 1,
            Build = 2,
            Ready = 3,
        }
    }
}
