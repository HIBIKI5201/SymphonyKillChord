using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.Application.Persistent.SceneManagement
{
    /// <summary>
    ///     シーン名ごとの初期化結果を保持するレジストリです。
    /// </summary>
    public sealed class SceneInitializationReadinessRegistry : ISceneInitializationReadiness
    {
        /// <summary>
        ///     最大待機フレーム数を指定して生成します。
        /// </summary>
        /// <param name="maxWaitFrameCount"> 最大待機フレーム数です。 </param>
        public SceneInitializationReadinessRegistry(
            int maxWaitFrameCount = DEFAULT_MAX_WAIT_FRAME_COUNT)
        {
            if (maxWaitFrameCount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxWaitFrameCount),
                    maxWaitFrameCount,
                    "最大待機フレーム数は1以上で指定してください。");
            }

            _maxWaitFrameCount = maxWaitFrameCount;
        }

        /// <summary>
        ///     対象シーンの初期化追跡を開始します。
        /// </summary>
        /// <param name="sceneName"> 追跡するシーン名です。 </param>
        public void BeginTracking(string sceneName)
        {
            ValidateSceneName(sceneName);

            if (_states.TryGetValue(sceneName, out SceneInitializationState state)
                && (!state.IsCompleted || state.IsSuccess))
            {
                return;
            }

            _states[sceneName] = new SceneInitializationState();
        }

        /// <summary>
        ///     対象シーンの初期化結果を通知します。
        /// </summary>
        /// <param name="sceneName"> 完了したシーン名です。 </param>
        /// <param name="isSuccess"> 初期化に成功した場合はtrueです。 </param>
        public void Complete(string sceneName, bool isSuccess)
        {
            ValidateSceneName(sceneName);

            if (!_states.TryGetValue(sceneName, out SceneInitializationState state))
            {
                state = new SceneInitializationState();
                _states.Add(sceneName, state);
            }

            state.Complete(isSuccess);
        }

        /// <summary>
        ///     対象シーンの追跡状態を解除します。
        /// </summary>
        /// <param name="sceneName"> 追跡を解除するシーン名です。 </param>
        public void Clear(string sceneName)
        {
            ValidateSceneName(sceneName);
            _states.Remove(sceneName);
        }

        /// <summary>
        ///     対象シーンの初期化完了を待機します。
        /// </summary>
        /// <param name="sceneName"> 待機するシーン名です。 </param>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 初期化に成功した場合はtrueです。 </returns>
        public async Awaitable<bool> WaitForReadyAsync(
            string sceneName,
            CancellationToken cancellationToken)
        {
            ValidateSceneName(sceneName);

            for (int waitFrameCount = 0;
                waitFrameCount < _maxWaitFrameCount;
                waitFrameCount++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (_states.TryGetValue(sceneName, out SceneInitializationState state)
                    && state.IsCompleted)
                {
                    return state.IsSuccess;
                }

                await Awaitable.NextFrameAsync(cancellationToken);
            }

            return false;
        }

        private const int DEFAULT_MAX_WAIT_FRAME_COUNT = 3600;
        private readonly Dictionary<string, SceneInitializationState> _states = new(StringComparer.Ordinal);
        private readonly int _maxWaitFrameCount;

        /// <summary>
        ///     シーン名が有効か検証します。
        /// </summary>
        /// <param name="sceneName"> 検証するシーン名です。 </param>
        private static void ValidateSceneName(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                throw new ArgumentException(
                    "シーン名を指定してください。",
                    nameof(sceneName));
            }
        }

        private sealed class SceneInitializationState
        {
            /// <summary> 初期化通知を受信済みの場合はtrueです。 </summary>
            public bool IsCompleted { get; private set; }

            /// <summary> 初期化に成功した場合はtrueです。 </summary>
            public bool IsSuccess { get; private set; }

            /// <summary>
            ///     初期化結果を確定します。
            /// </summary>
            /// <param name="isSuccess"> 初期化に成功した場合はtrueです。 </param>
            public void Complete(bool isSuccess)
            {
                IsSuccess = isSuccess;
                IsCompleted = true;
            }
        }
    }
}
