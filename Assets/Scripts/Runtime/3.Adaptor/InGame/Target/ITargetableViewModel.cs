using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Target
{
    /// <summary>
    ///     ターゲットシステムが扱う対象の共通インターフェース。
    /// </summary>
    public interface ITargetableViewModel
    {
        /// <summary> 対象の一意なID。 </summary>
        public Guid TargetId { get; }

        /// <summary> 対象の現在位置。 </summary>
        public Vector3 Position { get; }

        /// <summary> 対象が有効である場合は true。 </summary>
        public bool IsAlive { get; }
    }
}
