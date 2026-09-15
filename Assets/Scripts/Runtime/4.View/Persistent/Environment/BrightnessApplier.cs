using KillChord.Runtime.Adaptor.Persistent.Environment;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace KillChord.Runtime.View.Persistent.Environment
{
    /// <summary>
    ///     画面の明るさをURPのGlobal Volume（Color Adjustments）で適用するクラス。
    /// </summary>
    public sealed class BrightnessApplier : IBrightnessApplier
    {
        /// <summary>
        ///     明るさ適用用のVolumeを生成する。
        /// </summary>
        /// <param name="parent"> 生成するVolumeオブジェクトの親。ライフサイクルを親に合わせるために使用する。 </param>
        public BrightnessApplier(Transform parent)
        {
            GameObject volumeObject = new GameObject(nameof(BrightnessApplier));
            if (parent != null)
            {
                volumeObject.transform.SetParent(parent, false);
            }

            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            _colorAdjustments = profile.Add<ColorAdjustments>(true);
            _colorAdjustments.postExposure.overrideState = true;

            Volume volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = VOLUME_PRIORITY;
            volume.sharedProfile = profile;

            SetBrightness(DEFAULT_BRIGHTNESS);
        }

        /// <summary>
        ///     明るさを設定する。
        /// </summary>
        /// <param name="brightness"> 0～1に正規化した明るさ。 </param>
        public void SetBrightness(float brightness)
        {
            _brightness = Mathf.Clamp01(brightness);
            _colorAdjustments.postExposure.value = Mathf.Lerp(MIN_POST_EXPOSURE, MAX_POST_EXPOSURE, _brightness);
        }

        /// <summary>
        ///     明るさを取得する。
        /// </summary>
        public float GetBrightness()
        {
            return _brightness;
        }

        private const float DEFAULT_BRIGHTNESS = 0.5f;
        private const float MIN_POST_EXPOSURE = -3f;
        private const float MAX_POST_EXPOSURE = 3f;
        private const int VOLUME_PRIORITY = 50;

        private readonly ColorAdjustments _colorAdjustments;
        private float _brightness;
    }
}
