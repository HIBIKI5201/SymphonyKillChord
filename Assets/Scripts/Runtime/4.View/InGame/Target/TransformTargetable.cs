using KillChord.Runtime.Adaptor.InGame.Target;
using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Target
{
    /// <summary>
    ///     Transform をターゲット選択用の対象として扱うクラス。
    /// </summary>
    public sealed class TransformTargetable : ITargetable, ITargetBoundsViewModel, IDisposable
    {
        /// <summary>
        ///     追跡対象の Transform を受け取って初期化する。
        /// </summary>
        /// <param name="targetId"> 追跡対象のID。</param>
        /// <param name="targetTransform"> 追跡対象の Transform。</param>
        /// <param name="targetCollider"> 追跡対象の表示範囲に使用するCollider。</param>
        /// <param name="currentHealthProvider"> 追跡対象の現在HPを返す関数。HPを持たない対象ではnull。</param>
        public TransformTargetable(
            Guid targetId,
            Transform targetTransform,
            Collider targetCollider = null,
            Func<float> currentHealthProvider = null)
        {
            _targetId = targetId;
            _targetTransform = targetTransform;
            _targetCollider = targetCollider;
            _currentHealthProvider = currentHealthProvider;
        }

        /// <summary> 対象の一意なID。 </summary>
        public Guid TargetId => _targetId;

        /// <summary> 追跡対象のTransform。無効な場合はnull。 </summary>
        public Transform TargetTransform => IsAlive ? _targetTransform : null;

        /// <summary> 対象の現在位置。無効な場合は <see cref="Vector3.zero"/> を返す。 </summary>
        public Vector3 Position => IsAlive ? _targetTransform.position : Vector3.zero;

        /// <summary> 対象が有効である場合は true。 </summary>
        public bool IsAlive => !_isDisposed && _targetTransform != null;

        /// <summary> 対象の現在HP。HPを持たない、または無効な場合は <see cref="float.MaxValue"/> を返す。 </summary>
        public float CurrentHealth => IsAlive && _currentHealthProvider != null
            ? _currentHealthProvider.Invoke()
            : float.MaxValue;

        /// <summary> 対象のワールド空間Bounds。Colliderがない場合は対象位置の一点を返す。 </summary>
        public Bounds WorldBounds => _targetCollider != null
            ? _targetCollider.bounds
            : new Bounds(Position, Vector3.zero);

        /// <summary>
        ///     対象参照を破棄する。
        /// </summary>
        public void Dispose()
        {
            _targetTransform = null;
            _targetCollider = null;
            _currentHealthProvider = null;
            _isDisposed = true;
        }

        private readonly Guid _targetId;
        private Transform _targetTransform;
        private Collider _targetCollider;
        private Func<float> _currentHealthProvider;
        private bool _isDisposed;
    }
}
