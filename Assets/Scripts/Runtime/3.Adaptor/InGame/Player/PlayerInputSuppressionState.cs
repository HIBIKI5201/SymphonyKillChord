using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Player
{
    /// <summary>
    ///     プレイヤー入力を一定時間だけ無効化する状態を管理するクラス。
    ///     説明ポップアップ表示直後など、入力を読ませたい間だけ操作を無効化する用途に使う。
    /// </summary>
    public class PlayerInputSuppressionState
    {
        /// <summary> 現在入力が抑制されているかどうかを取得します。 </summary>
        public bool IsSuppressed => Time.unscaledTime < _suppressedUntil;

        /// <summary>
        ///     指定した秒数だけ入力を抑制します。既存の抑制の残り時間より短い場合は延長しません。
        /// </summary>
        /// <param name="duration"> 抑制する秒数です。 </param>
        public void Suppress(float duration)
        {
            float until = Time.unscaledTime + duration;
            if (until > _suppressedUntil)
            {
                _suppressedUntil = until;
            }
        }

        private float _suppressedUntil;
    }
}
