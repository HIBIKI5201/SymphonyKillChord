using KillChord.Runtime.Composition.Persistent.Bootstrap;
using KillChord.Runtime.View.Persistent.Environment;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.Persistent.Environment
{
    /// <summary>
    ///     解像度・画質・明るさ・表示言語をデバイスへ適用するAdaptorの初期化をする。
    /// </summary>
    public sealed class EnvironmentAppliersInitializer : PersistentInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(EnvironmentAppliersInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 32;

        /// <summary>
        ///     各適用先を構築して登録する。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Build()
        {
            _resolutionApplier = new ResolutionApplier();
            _qualityApplier = new QualityApplier();
            _brightnessApplier = new BrightnessApplier(transform);
            _languageApplier = new LanguageApplier();

            if (!ServiceLocator.RegisterInstance(_resolutionApplier)
                || !ServiceLocator.RegisterInstance(_qualityApplier)
                || !ServiceLocator.RegisterInstance(_brightnessApplier)
                || !ServiceLocator.RegisterInstance(_languageApplier))
            {
                Debug.LogError(
                    $"[{nameof(EnvironmentAppliersInitializer)}] 環境設定の適用先を登録できませんでした。",
                    this);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     登録済みの適用先を解除する。
        /// </summary>
        public override void Shutdown()
        {
            if (ServiceLocator.TryGetInstance(out ResolutionApplier registeredResolutionApplier)
                && ReferenceEquals(registeredResolutionApplier, _resolutionApplier))
            {
                ServiceLocator.UnregisterInstance<ResolutionApplier>();
            }

            if (ServiceLocator.TryGetInstance(out QualityApplier registeredQualityApplier)
                && ReferenceEquals(registeredQualityApplier, _qualityApplier))
            {
                ServiceLocator.UnregisterInstance<QualityApplier>();
            }

            if (ServiceLocator.TryGetInstance(out BrightnessApplier registeredBrightnessApplier)
                && ReferenceEquals(registeredBrightnessApplier, _brightnessApplier))
            {
                ServiceLocator.UnregisterInstance<BrightnessApplier>();
            }

            if (ServiceLocator.TryGetInstance(out LanguageApplier registeredLanguageApplier)
                && ReferenceEquals(registeredLanguageApplier, _languageApplier))
            {
                ServiceLocator.UnregisterInstance<LanguageApplier>();
            }

            _resolutionApplier = null;
            _qualityApplier = null;
            _brightnessApplier = null;
            _languageApplier = null;
        }

        private ResolutionApplier _resolutionApplier;
        private QualityApplier _qualityApplier;
        private BrightnessApplier _brightnessApplier;
        private LanguageApplier _languageApplier;
    }
}
