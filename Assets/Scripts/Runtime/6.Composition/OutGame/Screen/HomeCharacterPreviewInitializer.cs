using KillChord.Runtime.Composition.OutGame.Bootstrap;
using KillChord.Runtime.View.OutGame.Screen;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.OutGame.Screen
{
    /// <summary>
    ///     ホーム画面に表示する3Dキャラクタープレビューの依存を解決するクラス。
    ///     シーンに既に存在するキャラクターを専用カメラでレンダーテクスチャへ描画し、HomeScreenViewへ反映します。
    /// </summary>
    public sealed class HomeCharacterPreviewInitializer : OutGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(HomeCharacterPreviewInitializer);

        /// <summary> 実行順です。ScreenInitializerでHomeScreenViewが登録された後に実行します。 </summary>
        public override int Order => 110;

        /// <summary>
        ///     プレビューカメラ・レンダーテクスチャを生成し、シーン上のキャラクターに追従させます。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Build()
        {
            if (_sceneCharacterTransform == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(HomeCharacterPreviewInitializer)}] シーン上のキャラクターが設定されていません。", this);
#endif
                return false;
            }

            // 既存のシーンオブジェクト(Timeline等で制御される)を子として追従させるため、
            // レイヤーやAnimatorには一切手を加えない。
            GameObject cameraObject = new(PREVIEW_CAMERA_NAME);
            cameraObject.transform.SetParent(_sceneCharacterTransform, false);
            cameraObject.transform.localPosition = _cameraLocalPosition;
            cameraObject.transform.localEulerAngles = _cameraLocalEulerAngles;

            // Inspectorで未設定(0)の場合は、キャラクター自身のレイヤーを自動的に使用する。
            int cullingMask = _cullingMask.value != 0
                ? _cullingMask.value
                : 1 << _sceneCharacterTransform.gameObject.layer;

            _previewCamera = cameraObject.AddComponent<Camera>();
            _previewCamera.clearFlags = CameraClearFlags.SolidColor;
            _previewCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            _previewCamera.cullingMask = cullingMask;
            _previewCamera.fieldOfView = _fieldOfView;
            _previewCamera.nearClipPlane = 0.05f;
            _previewCamera.farClipPlane = 20f;

            _renderTexture = new RenderTexture(_renderTextureWidth, _renderTextureHeight, 16, RenderTextureFormat.ARGB32)
            {
                name = "HomeCharacterPreviewRT"
            };
            _renderTexture.Create();
            _previewCamera.targetTexture = _renderTexture;

            return true;
        }

        /// <summary>
        ///     生成したレンダーテクスチャをHomeScreenViewへ反映します。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Ready()
        {
            if (!ServiceLocator.TryGetInstance(out HomeScreenView homeScreenView))
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(HomeCharacterPreviewInitializer)}] HomeScreenView が取得できませんでした。", this);
#endif
                return false;
            }

            homeScreenView.SetCharacterTexture(_renderTexture);
            return true;
        }

        /// <summary>
        ///     生成したカメラ・レンダーテクスチャを破棄します。シーン上のキャラクターは破棄しません。
        /// </summary>
        public override void Shutdown()
        {
            if (_previewCamera != null)
            {
                _previewCamera.targetTexture = null;
                Destroy(_previewCamera.gameObject);
                _previewCamera = null;
            }

            if (_renderTexture != null)
            {
                _renderTexture.Release();
                Destroy(_renderTexture);
                _renderTexture = null;
            }
        }

        private const string PREVIEW_CAMERA_NAME = "HomeCharacterPreviewCamera";

        [SerializeField, Tooltip("ホーム画面に表示する、シーンに既に配置されているキャラクターです。Timeline等で制御される既存オブジェクトを指定します。")]
        private Transform _sceneCharacterTransform;
        [SerializeField, Tooltip("プレビューカメラが描画するレイヤーです。シーンのキャラクターが属するレイヤーを指定します。")]
        private LayerMask _cullingMask;
        [SerializeField, Tooltip("プレビューカメラのキャラクターからの相対位置です。")]
        private Vector3 _cameraLocalPosition = new(1.1f, 1.1f, 1.9052559f);
        [SerializeField, Tooltip("プレビューカメラの回転です。キャラクターから見て右30度の位置から見る向きです。")]
        private Vector3 _cameraLocalEulerAngles = new(0f, 210f, 0f);
        [SerializeField, Tooltip("プレビューカメラの画角です。")]
        private float _fieldOfView = 30f;
        [SerializeField, Tooltip("レンダーテクスチャの幅です。")]
        private int _renderTextureWidth = 1024;
        [SerializeField, Tooltip("レンダーテクスチャの高さです。")]
        private int _renderTextureHeight = 2048;

        private Camera _previewCamera;
        private RenderTexture _renderTexture;
    }
}
