using System;
using UnityEngine;

namespace DevelopProducts.BindingSystem
{
    /// <summary>
    ///     視点操作（カメラ）の感度設定。
    /// </summary>
    [Serializable]
    public class LookSensitivitySetting : SensitivitySettingBase
    {
        [SerializeField, Tooltip("感度倍率を保持するデータアセット")]
        private SensitivityData _sensitivityData;

        protected override void ApplyValue(float value)
        {
            // TODO: ここで、実際のカメラコントローラーに感度を適用する処理を実装する。
        }
    }
}
