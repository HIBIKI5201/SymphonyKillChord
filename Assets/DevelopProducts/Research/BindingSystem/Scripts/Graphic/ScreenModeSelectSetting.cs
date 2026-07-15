using System;
using UnityEngine;

namespace DevelopProducts.BindingSystem
{
    [Serializable]
    public class ScreenModeSelectSetting : GraphicSelectSettingBase
    {
        protected override void ApplyValue(int index)
        {
            // indexをFullScreenModeにキャストして適用
            var mode = (FullScreenMode)index;
            // 現在の解像度を維持してフルスクリーンモードのみ変更する
            var currentResolution = Screen.currentResolution;
            // フルスクリーンモードを変更
            Screen.SetResolution(currentResolution.width, currentResolution.height, mode);
        }

        protected override Array GetItems()
        {
            return Enum.GetValues(typeof(FullScreenMode));
        }

        /// <summary>
        ///     現在のフルスクリーンモードに対応するインデックスを取得する。
        ///     起動時にPlayer Settingsの初期モードを上書きしないようにするため、
        ///     _defaultIndexではなく現在のフルスクリーンモードを優先する。
        /// </summary>
        protected override int GetCurrentIndex()
        {
            return (int)Screen.fullScreenMode;
        }

        protected override string GetLabel(int index)
        {
            var mode = (FullScreenMode)index;
            return mode.ToString();
        }
    }
}
