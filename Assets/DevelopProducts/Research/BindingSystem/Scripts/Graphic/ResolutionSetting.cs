using System;
using UnityEngine;

namespace DevelopProducts.BindingSystem
{
    /// <summary>
    ///     画面解像度設定。
    /// </summary>
    [Serializable]
    public class ResolutionSetting : GraphicSelectSettingBase
    {
        private Resolution[] _resolutions;

        private Resolution[] GetResolutions()
        {
            if (_resolutions == null)
            {
                _resolutions = Screen.resolutions;
            }
            return _resolutions;
        }

        protected override Array GetItems() => GetResolutions();

        /// <summary>
        ///     現在の画面解像度に対応するインデックスを取得する。
        /// </summary>
        protected override int GetCurrentIndex()
        {
            var resolutions = GetResolutions();
            var currentResolution = Screen.currentResolution;
            for (int i = 0; i < resolutions.Length; i++)
            {
                if (resolutions[i].width == currentResolution.width && resolutions[i].height == currentResolution.height)
                {
                    return i;
                }
            }
            return _defaultIndex;
        }

        /// <summary>
        ///    解像度のラベルを取得する。例: "1920 x 1080 (16:9)"
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override string GetLabel(int index)
        {
            var resolution = GetResolutions()[index];
            var aspectRatio = GetClosestAspectRatio(resolution.width, resolution.height);
            return $"{resolution.width} x {resolution.height} ({aspectRatio.Width}:{aspectRatio.Height})";
        }

        /// <summary>
        ///     代表的なアスペクト比の一覧。
        /// </summary>
        private static readonly (int Width, int Height)[] CommonAspectRatios =
        {
            (16, 9),
            (16, 10),
            (4, 3),
            (21, 9),
            (5, 4),
            (3, 2),
        };

        /// <summary>
        ///     解像度に最も近い代表的なアスペクト比を求める。
        /// </summary>
        private static (int Width, int Height) GetClosestAspectRatio(int width, int height)
        {
            float ratio = (float)width / height;

            var closest = CommonAspectRatios[0];
            float closestDiff = Mathf.Abs(ratio - (float)closest.Width / closest.Height);

            for (int i = 1; i < CommonAspectRatios.Length; i++)
            {
                var candidate = CommonAspectRatios[i];
                float diff = Mathf.Abs(ratio - (float)candidate.Width / candidate.Height);
                if (diff < closestDiff)
                {
                    closestDiff = diff;
                    closest = candidate;
                }
            }

            return closest;
        }

        /// <summary>
        ///   選択された解像度を排他的フルスクリーンで適用する。
        /// </summary>
        /// <param name="index"></param>
        protected override void ApplyValue(int index)
        {
            var resolution = GetResolutions()[index];
            var currentScreenMode = Screen.fullScreenMode;
            Screen.SetResolution(resolution.width, resolution.height, currentScreenMode);
        }
    }
}