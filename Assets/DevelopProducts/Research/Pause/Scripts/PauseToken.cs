using System;

namespace DevelopProducts.Pause
{
    /// <summary>
    ///     ポーズの解除ハンドル
    ///     TimeScaleHandlerクラスでDispose()を呼びポーズを解除する
    /// </summary>
    public class PauseToken : IDisposable
    {
        /// <summary>
        ///     コンストラクタ
        /// </summary>
        /// <param name="onDispose"></param>
        public PauseToken(Action onDispose)
        {
            _onDispose = onDispose ?? throw new ArgumentNullException(nameof(onDispose));
        }
        /// <summary>
        ///     ポーズ解除
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _onDispose.Invoke();
        }

        private readonly Action _onDispose;
        private bool _disposed = false;
    }
}
