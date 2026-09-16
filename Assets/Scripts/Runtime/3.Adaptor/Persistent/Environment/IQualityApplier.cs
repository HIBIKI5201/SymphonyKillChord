using System.Collections.Generic;

namespace KillChord.Runtime.Adaptor.Persistent.Environment
{
    /// <summary>
    ///     画質プリセットをデバイスへ適用する共通インターフェース。
    /// </summary>
    public interface IQualityApplier
    {
        /// <summary>
        ///     選択可能な画質プリセットの一覧を取得する。
        /// </summary>
        IReadOnlyList<QualityLevelOption> GetAvailableQualityLevels();

        /// <summary>
        ///     画質プリセットを適用する。
        /// </summary>
        void Apply(int qualityLevelIndex);

        /// <summary>
        ///     指定した画質プリセットの表示名を取得する。
        /// </summary>
        string GetQualityLevelName(int qualityLevelIndex);
    }
}
