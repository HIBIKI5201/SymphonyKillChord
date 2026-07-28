using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Reticle
{
    /// <summary>
    ///     ワールド座標をスクリーン座標へ投影する処理を抽象化するインターフェース。
    ///     カメラ実装への依存をViewModelから切り離し、テスト可能にする。
    /// </summary>
    public interface IScreenProjector
    {
        /// <summary>
        ///     ワールド座標をスクリーン座標へ投影する。
        /// </summary>
        /// <param name="worldPosition"> 投影元のワールド座標。 </param>
        /// <param name="screenPosition"> 投影されたスクリーン座標。投影不可の場合は <see cref="Vector2.zero"/>。 </param>
        /// <returns> 画面内へ投影できた場合は true。カメラ背後や画面外の場合は false。 </returns>
        bool TryWorldToScreen(in Vector3 worldPosition, out Vector2 screenPosition);
    }
}
