using KillChord.Runtime.Composition.OutGame.Bootstrap;
using KillChord.Runtime.View.OutGame.Screen;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.OutGame.Screen
{
    /// <summary>
    ///     ホーム画面に表示する3Dキャラクタープレビューの依存を解決するクラス。
    ///     専用カメラでキャラクターをレンダーテクスチャへ描画し、HomeScreenViewへ反映します。
    /// </summary>
    public sealed class HomeCharacterPreviewInitializer : OutGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(HomeCharacterPreviewInitializer);

        /// <summary> 実行順です。ScreenInitializerでHomeScreenViewが登録された後に実行します。 </summary>
        public override int Order => 110;

        /// <summary>
        ///     キャラクターインスタンス・プレビューカメラ・レンダーテクスチャを生成します。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Build()
        {
            if (_characterPrefab == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(HomeCharacterPreviewInitializer)}] キャラクタープレハブが設定されていません。", this);
#endif
                return false;
            }

            int previewLayer = LayerMask.NameToLayer(PREVIEW_LAYER_NAME);
            if (previewLayer < 0)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(HomeCharacterPreviewInitializer)}] レイヤー {PREVIEW_LAYER_NAME} が見つかりません。", this);
#endif
                return false;
            }

            _characterInstance = Instantiate(_characterPrefab, _spawnPosition, Quaternion.Euler(_spawnEulerAngles));
            SetLayerRecursively(_characterInstance, previewLayer);

            Animator animator = _characterInstance.GetComponentInChildren<Animator>(true);
            if (animator != null)
            {
                if (_playIdlePose && _idleController != null)
                {
                    animator.runtimeAnimatorController = _idleController;
                }
                else
                {
                    // ポーズを適用せず、モデル本来のバインドポーズ(Tポーズ)のまま表示する。
                    animator.enabled = false;
                }
            }

            GameObject cameraObject = new(PREVIEW_CAMERA_NAME);
            cameraObject.transform.SetParent(_characterInstance.transform, false);
            cameraObject.transform.localPosition = _cameraLocalPosition;
            cameraObject.transform.localEulerAngles = _cameraLocalEulerAngles;
            cameraObject.layer = previewLayer;

            _previewCamera = cameraObject.AddComponent<Camera>();
            _previewCamera.clearFlags = CameraClearFlags.SolidColor;
            _previewCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            _previewCamera.cullingMask = 1 << previewLayer;
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
        ///     生成したオブジェクト・リソースを破棄します。
        /// </summary>
        public override void Shutdown()
        {
            if (_previewCamera != null)
            {
                _previewCamera.targetTexture = null;
            }

            if (_characterInstance != null)
            {
                Destroy(_characterInstance);
                _characterInstance = null;
            }

            if (_renderTexture != null)
            {
                _renderTexture.Release();
                Destroy(_renderTexture);
                _renderTexture = null;
            }
        }

        /// <summary>
        ///     対象とその子孫すべてのレイヤーを再帰的に設定します。
        /// </summary>
        private static void SetLayerRecursively(GameObject target, int layer)
        {
            target.layer = layer;
            foreach (Transform child in target.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        private const string PREVIEW_LAYER_NAME = "HomeCharacterPreview";
        private const string PREVIEW_CAMERA_NAME = "HomeCharacterPreviewCamera";

        [SerializeField, Tooltip("ホーム画面に表示する3Dキャラクターのプレハブです。")]
        private GameObject _characterPrefab;
        [SerializeField, Tooltip("trueの場合、待機ポーズのAnimatorControllerを再生します。falseの場合はバインドポーズ(Tポーズ)のまま表示します。")]
        private bool _playIdlePose = false;
        [SerializeField, Tooltip("待機ポーズ用のAnimatorControllerです。")]
        private RuntimeAnimatorController _idleController;
        [SerializeField, Tooltip("他のカメラに映り込まないよう配置する孤立座標です。")]
        private Vector3 _spawnPosition = new(500f, 0f, 500f);
        [SerializeField, Tooltip("インスタンス生成時の回転です。")]
        private Vector3 _spawnEulerAngles = Vector3.zero;
        [SerializeField, Tooltip("プレビューカメラのキャラクターからの相対位置です。")]
        private Vector3 _cameraLocalPosition = new(0f, 1.1f, 2.2f);
        [SerializeField, Tooltip("プレビューカメラの回転です。")]
        private Vector3 _cameraLocalEulerAngles = new(0f, 180f, 0f);
        [SerializeField, Tooltip("プレビューカメラの画角です。")]
        private float _fieldOfView = 30f;
        [SerializeField, Tooltip("レンダーテクスチャの幅です。")]
        private int _renderTextureWidth = 1024;
        [SerializeField, Tooltip("レンダーテクスチャの高さです。")]
        private int _renderTextureHeight = 2048;

        private GameObject _characterInstance;
        private Camera _previewCamera;
        private RenderTexture _renderTexture;
    }
}
