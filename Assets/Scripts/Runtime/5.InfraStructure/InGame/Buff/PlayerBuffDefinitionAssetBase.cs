using KillChord.Runtime.Domain.InGame.Buff;
using System;

namespace KillChord.Runtime.InfraStructure.InGame.Buff
{
    /// <summary>
    ///     プレイヤーへ付与するバフ・デバフ定義アセットの基底クラス。
    /// </summary>
    [Serializable]
    public abstract class PlayerBuffDefinitionAssetBase
    {
        /// <summary>
        ///     バフを生成します。
        /// </summary>
        /// <returns>バフ。</returns>
        public abstract IBuff Create();
    }
}
