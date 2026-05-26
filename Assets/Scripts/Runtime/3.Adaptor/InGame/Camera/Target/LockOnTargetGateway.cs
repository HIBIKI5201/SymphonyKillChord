using KillChord.Runtime.Domain.InGame.Camera.Target;
using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Camera.Target
{
    /// <summary>
    ///     Unity の Transform をロックオン対象インターフェースへ変換するゲートウェイクラス。
    ///     IDisposable を実装し、破棄後は無効状態として扱う。
    /// </summary>
    public sealed class LockOnTargetGateway : ILockOnTarget, IDisposable
    {
        /// <summary>
        ///     ロックオン対象の Transform を受け取り、ゲートウェイを初期化するコンストラクタ。
        /// </summary>
        /// <param name="fromTarget"> ロックオン対象の Transform。</param>
        public LockOnTargetGateway(Transform fromTarget)
        {
            _cache = fromTarget;
            _isDisposed = false;
        }

        /// <summary> ロックオン対象のワールド座標。破棄済みの場合は Vector3.zero を返す。 </summary>
        public Vector3 Position => IsAlive ? _cache.position : Vector3.zero;

        /// <summary> ロックオン対象が有効であるかを示す。未破棄かつ Transform が null でない場合にtrueを返す。 </summary>
        public bool IsAlive => !_isDisposed && _cache != null;

        /// <summary>
        ///     ゲートウェイを破棄し、対象への参照を解放する。
        ///     二重破棄時は ObjectDisposedException をスローする。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(LockOnTargetGateway));
            }
            _cache = null;
            _isDisposed = true;
        }

        private Transform _cache;
        private bool _isDisposed;
    }
}
