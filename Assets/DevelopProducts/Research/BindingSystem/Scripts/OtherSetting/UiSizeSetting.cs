using System;
using UnityEngine;
using UnityEngine.UI;

namespace DevelopProducts.BindingSystem
{
    /// <summary>
    ///     UIサイズ設定（小・中・大）。
    /// </summary>
    [Serializable]
    public class UiSizeSetting : OtherSettingBase
    {
        private static readonly string[] Labels = { "小", "中", "大" };
        private static readonly float[] Scales = { 0.8f, 1f, 1.2f };

        [SerializeField] private CanvasScaler[] _targetScalers;

        protected override Array GetItems() => Labels;

        protected override string GetLabel(int index) => Labels[index];

        protected override void ApplyValue(int index)
        {
            foreach (var scaler in _targetScalers)
            {
                scaler.scaleFactor = Scales[index];
            }
        }
    }
}
