using KillChord.Runtime.View.Persistent.PostEffect;
using KillChord.Runtime.Composition.Persistent.Bootstrap;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.Persistent.Camera
{
    /// <summary>
    ///     カメラの初期化を担当するクラス。
    ///     ServiceLocator に <see cref="ICameraTransform"/> を登録する。
    /// </summary>
    public sealed class CameraInitializer : PersistentInitializationModuleBase, ICameraTransform
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(CameraInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 40;

        /// <summary>
        ///     ICameraTransform の Transform プロパティ。
        ///     MonoBehaviour の transform を返す。  
        /// </summary>
        public Transform Transform => transform;

        /// <summary>
        ///     起動時にServiceLocatorへインスタンスを登録する。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Build()
        {
            ServiceLocator.RegisterInstance<ICameraTransform>(this, LocateTypeEnum.Locator);

            // ポストプロセスの適用は、同じカメラ上のコンポーネントが担う。
            _postEffectOverlayCamera = GetComponent<PostEffectOverlayCamera>();
            if (_postEffectOverlayCamera != null)
            {
                ServiceLocator.RegisterInstance<IPostEffectOverlayPlayer>(
                    _postEffectOverlayCamera, LocateTypeEnum.Locator);
            }

            return true;
        }

        /// <summary>
        ///     登録済みカメラTransformを解除する。
        /// </summary>
        public override void Shutdown()
        {
            if (ServiceLocator.TryGetInstance<ICameraTransform>(out var registeredCameraTransform)
                && ReferenceEquals(registeredCameraTransform, this))
            {
                ServiceLocator.UnregisterInstance<ICameraTransform>();
            }

            if (_postEffectOverlayCamera != null)
            {
                ServiceLocator.UnregisterInstance<IPostEffectOverlayPlayer>();
                _postEffectOverlayCamera = null;
            }
        }

        private PostEffectOverlayCamera _postEffectOverlayCamera;
    }
}
