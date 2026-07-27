using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Reticle
{
    /// <summary>
    ///     1体分のレティクル表示情報を保持する。
    /// </summary>
    public readonly struct ReticleMarker
    {
        /// <summary>
        ///     レティクル表示情報を生成する。
        /// </summary>
        /// <param name="targetId"> 対象のターゲットID。 </param>
        /// <param name="screenPosition"> 表示するスクリーン座標。 </param>
        public ReticleMarker(Guid targetId, in Vector2 screenPosition)
        {
            TargetId = targetId;
            ScreenPosition = screenPosition;
        }

        /// <summary> 対象のターゲットID。 </summary>
        public readonly Guid TargetId;

        /// <summary> 表示するスクリーン座標。 </summary>
        public readonly Vector2 ScreenPosition;
    }
}
