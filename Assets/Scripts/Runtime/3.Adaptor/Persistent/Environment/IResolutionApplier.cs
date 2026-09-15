using System.Collections.Generic;

namespace KillChord.Runtime.Adaptor.Persistent.Environment
{
    /// <summary>
    ///     解像度・画面モードをデバイスへ適用する共通インターフェース。
    /// </summary>
    public interface IResolutionApplier
    {
        /// <summary>
        ///     選択可能な解像度の一覧を取得する。
        /// </summary>
        IReadOnlyList<ResolutionOption> GetAvailableResolutions();

        /// <summary>
        ///     解像度と画面モードを適用する。
        /// </summary>
        void Apply(int width, int height, bool isFullScreen);
    }
}
