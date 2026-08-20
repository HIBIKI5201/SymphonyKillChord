using Unity.Entities;

namespace KillChord.Runtime.View.InGame.Stage.Barrage
{
    /// <summary>
    ///     ルーティング辞書へ登録済みであることを示します。
    /// </summary>
    /// <remarks>
    ///     Cleanupコンポーネントのため、タレットEntityの破棄後もこのコンポーネントだけが残ります。
    ///     それを検出して辞書から登録を除去します。
    /// </remarks>
    public struct TurretRegistered : ICleanupComponentData
    {
        /// <summary> 登録時に使用した数値IDです。破棄検出時の辞書キーになります。 </summary>
        public int Id;
    }
}
