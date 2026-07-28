using KillChord.Runtime.Adaptor.InGame.Reticle;
using KillChord.Runtime.Composition.InGame.Bootstrap;
using KillChord.Runtime.Composition.InGame.Target;
using KillChord.Runtime.View.InGame.Reticle;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Reticle
{
    using Camera = UnityEngine.Camera;

    /// <summary>
    ///     全敵レティクル表示に関するクラスの生成と依存解決を行う初期化クラス。
    ///     注目/候補の強調表示を担う HUDEnemyHealthInitializer(Order 650) の後に実行する。
    /// </summary>
    public sealed class ReticleHudInitializer : InGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(ReticleHudInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 660;

        /// <summary>
        ///     ターゲットモジュールへ結合してレティクル表示を構築する。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Ready()
        {
            if (_view == null)
            {
                Debug.LogError($"[{nameof(ReticleHudInitializer)}] {nameof(_view)} がアサインされていません。", this);
                return false;
            }

            TargetSystemModuleContainer targetSystemModuleContainer = ServiceLocator.GetInstance<TargetSystemModuleContainer>();
            if (targetSystemModuleContainer == null)
            {
                Debug.LogError($"[{nameof(ReticleHudInitializer)}] {nameof(TargetSystemModuleContainer)} が見つかりません。", this);
                return false;
            }

            Camera camera = Camera.main;
            if (camera == null)
            {
                Debug.LogError($"[{nameof(ReticleHudInitializer)}] メインカメラが見つかりません。", this);
                return false;
            }

            IScreenProjector projector = new CameraScreenProjector(camera);
            ReticleHudPresenter presenter = new ReticleHudPresenter(
                targetSystemModuleContainer.TargetSystemViewModel,
                projector);
            _view.Initialize(presenter);
            return true;
        }

        [SerializeField, Tooltip("全敵レティクルを表示する View コンポーネント。")]
        private ReticleHudView _view;
    }
}
