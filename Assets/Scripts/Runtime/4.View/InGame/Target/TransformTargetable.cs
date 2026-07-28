using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Target
{
    /// <summary>
    ///     Transform をターゲット選択用の対象として扱うクラス。
    /// </summary>
    public sealed class TransformTargetable : ITargetable, IDisposable
    {
        /// <summary>
        ///     追跡対象の Transform を受け取って初期化する。
        /// </summary>
        /// <param name="targetId"> 追跡対象のID。</param>
        /// <param name="targetTransform"> 追跡対象の Transform。</param>
        public TransformTargetable(Guid targetId, Transform targetTransform)
        {
            _targetId = targetId;
            _targetTransform = targetTransform;
        }

        /// <summary> 対象の一意なID。 </summary>
        public Guid TargetId => _targetId;

        /// <summary> 対象の現在位置。無効な場合は <see cref="Vector3.zero"/> を返す。 </summary>
        public Vector3 Position => IsAlive ? _targetTransform.position : Vector3.zero;

        /// <summary> 対象が有効である場合は true。 </summary>
        public bool IsAlive => !_isDisposed && _targetTransform != null;

        /// <summary>
        ///     対象参照を破棄する。
        /// </summary>
        public void Dispose()
        {
            _targetTransform = null;
            _isDisposed = true;
        }

        private readonly Guid _targetId;
        private Transform _targetTransform;
        private bool _isDisposed;
    }
}
