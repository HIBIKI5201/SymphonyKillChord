using KillChord.Runtime.Adaptor.Persistent.Environment;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.View.Persistent.Environment
{
    /// <summary>
    ///     解像度・画面モードをデバイスへ適用するクラス。
    /// </summary>
    public sealed class ResolutionApplier : IResolutionApplier
    {
        /// <summary>
        ///     選択可能な解像度の一覧を取得する。
        ///     ディスプレイがサポートする解像度からリフレッシュレート違いの重複を除いて返す。
        /// </summary>
        public IReadOnlyList<ResolutionOption> GetAvailableResolutions()
        {
            if (_availableResolutions != null)
            {
                return _availableResolutions;
            }

            Resolution[] deviceResolutions = Screen.resolutions;
            List<ResolutionOption> options = new(deviceResolutions.Length);
            HashSet<(int Width, int Height)> addedSizes = new();

            foreach (Resolution resolution in deviceResolutions)
            {
                (int Width, int Height) size = (resolution.width, resolution.height);
                if (!addedSizes.Add(size))
                {
                    continue;
                }

                options.Add(new ResolutionOption(resolution.width, resolution.height));
            }

            if (options.Count == 0)
            {
                options.Add(new ResolutionOption(Screen.currentResolution.width, Screen.currentResolution.height));
            }

            _availableResolutions = options;
            return _availableResolutions;
        }

        /// <summary>
        ///     解像度と画面モードを適用する。
        /// </summary>
        public void Apply(int width, int height, bool isFullScreen)
        {
            Screen.SetResolution(width, height, isFullScreen);
        }

        private List<ResolutionOption> _availableResolutions;
    }
}
