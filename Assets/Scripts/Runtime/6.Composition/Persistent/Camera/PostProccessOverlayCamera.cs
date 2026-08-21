using KillChord.Runtime.Adaptor.InGame.PostEffect;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace KillChord.Runtime.Composition.Persistent.Camera
{
    /// <summary>
    ///     注目レイヤー（プレイヤー・スキルエフェクト）を除いた画面にだけVolumeを掛けるカメラ制御。
    ///     注目レイヤーはポストプロセスを持たないOverlayカメラで上描きする。
    ///     適用していない間はOverlayカメラをスタックから外すため、通常時の描画コストは増えない。
    /// </summary>
    [RequireComponent(typeof(UnityEngine.Camera))]
    public sealed class PostProccessOverlayCamera : MonoBehaviour, IFocusPostEffectPlayer
    {
        /// <summary> Volumeを適用中かどうかです。 </summary>
        public bool IsPlaying => _isPlaying;

        /// <summary>
        ///     待機したのち、指定秒数だけVolumeの適用を行う。
        /// </summary>
        /// <param name="delaySeconds"> 適用を開始するまでの待機秒数です。0以下なら即座に開始します。 </param>
        /// <param name="durationSeconds"> 適用を継続する秒数です。0以下なら何もしません。 </param>
        public void Play(float delaySeconds, float durationSeconds)
        {
            if (durationSeconds <= 0f || _baseCamera == null)
            {
                return;
            }

            float delay = Mathf.Max(0f, delaySeconds);

            // 別スキルの予約が残っている場合は、終了が遅い方に合わせて伸ばす。
            _endSeconds = Mathf.Max(_endSeconds, delay + durationSeconds);

            if (_isPlaying)
            {
                return;
            }

            // 待機中の予約は、より早く始まる方を採用する。
            _startSeconds = _isPending ? Mathf.Min(_startSeconds, delay) : delay;
            _isPending = true;
            _elapsedSeconds = 0f;

            if (delay <= 0f)
            {
                BeginFocus();
            }
        }

        /// <summary>
        ///     Volumeの適用と待機中の予約を即座に終了する。
        /// </summary>
        public void Stop()
        {
            _isPending = false;
            _elapsedSeconds = 0f;
            _startSeconds = 0f;
            _endSeconds = 0f;

            if (!_isPlaying)
            {
                return;
            }

            _isPlaying = false;
            ApplyFocusRendering(false);
        }

        [SerializeField]
        [Tooltip("Volumeを掛けない注目レイヤーです。プレイヤーとスキルエフェクトを指定します。")]
        private LayerMask _focusLayers;

        [SerializeField]
        [Tooltip("適用中だけ有効化するVolumeProfileです。実行時に専用のGlobal Volumeへ設定します。")]
        private VolumeProfile _focusVolumeProfile;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("適用中のVolumeの強さです。")]
        private float _focusVolumeWeight = DEFAULT_VOLUME_WEIGHT;

        private UnityEngine.Camera _baseCamera;
        private UniversalAdditionalCameraData _baseCameraData;
        private UnityEngine.Camera _overlayCamera;
        private Volume _focusVolume;

        private bool _isPlaying;
        private bool _isPending;
        private bool _wasPostProcessingEnabled;
        private float _elapsedSeconds;
        private float _startSeconds;
        private float _endSeconds;

        /// <summary>
        ///     Base側のカメラ参照を解決する。
        /// </summary>
        private void Awake()
        {
            _baseCamera = GetComponent<UnityEngine.Camera>();
            _baseCameraData = GetComponent<UniversalAdditionalCameraData>();

            if (_baseCameraData == null)
            {
                Debug.LogError(
                    $"[{nameof(PostProccessOverlayCamera)}] {nameof(UniversalAdditionalCameraData)} が必要です。",
                    this);
                _baseCamera = null;
            }
        }

        /// <summary>
        ///     待機時間と適用時間を消化し、開始と終了を切り替える。
        /// </summary>
        private void Update()
        {
            if (!_isPending && !_isPlaying)
            {
                return;
            }

            _elapsedSeconds += Time.deltaTime;

            if (!_isPlaying && _elapsedSeconds >= _startSeconds)
            {
                BeginFocus();
            }

            if (_elapsedSeconds >= _endSeconds)
            {
                Stop();
            }
        }

        /// <summary>
        ///     破棄時に描画構成を元へ戻す。
        /// </summary>
        private void OnDestroy()
        {
            Stop();
        }

        /// <summary>
        ///     待機を終えてVolumeの適用を開始する。
        /// </summary>
        private void BeginFocus()
        {
            if (!TryPrepareFocusObjects())
            {
                Stop();
                return;
            }

            _isPending = false;
            _isPlaying = true;
            ApplyFocusRendering(true);
        }

        /// <summary>
        ///     OverlayカメラとVolumeを必要時に生成する。
        /// </summary>
        /// <returns> 使用できる構成が揃っている場合はtrue。 </returns>
        private bool TryPrepareFocusObjects()
        {
            if (_overlayCamera != null)
            {
                return true;
            }

            GameObject overlayObject = new GameObject($"{nameof(PostProccessOverlayCamera)}_Focus");
            overlayObject.transform.SetParent(transform, false);

            _overlayCamera = overlayObject.AddComponent<UnityEngine.Camera>();

            // 画角や描画距離をBase側から引き継ぎ、注目レイヤーの見え方を一致させる。
            _overlayCamera.CopyFrom(_baseCamera);
            _overlayCamera.cullingMask = _focusLayers.value;
            _overlayCamera.clearFlags = CameraClearFlags.Depth;
            _overlayCamera.enabled = false;

            UniversalAdditionalCameraData overlayData = overlayObject.AddComponent<UniversalAdditionalCameraData>();
            overlayData.renderType = CameraRenderType.Overlay;

            // 注目レイヤーはVolumeの影響を受けさせない。
            overlayData.renderPostProcessing = false;

            CreateFocusVolume();
            return true;
        }

        /// <summary>
        ///     BaseカメラがVolumeを検出できるレイヤー上に、専用のGlobal Volumeを生成する。
        /// </summary>
        private void CreateFocusVolume()
        {
            if (_focusVolumeProfile == null)
            {
                Debug.LogWarning(
                    $"[{nameof(PostProccessOverlayCamera)}] {nameof(VolumeProfile)} が未設定のため、レイヤー分割のみ行います。",
                    this);
                return;
            }

            GameObject volumeObject = new GameObject($"{nameof(PostProccessOverlayCamera)}_Volume");
            volumeObject.transform.SetParent(transform, false);
            volumeObject.layer = ResolveVolumeLayer();

            _focusVolume = volumeObject.AddComponent<Volume>();
            _focusVolume.isGlobal = true;
            _focusVolume.priority = FOCUS_VOLUME_PRIORITY;
            _focusVolume.sharedProfile = _focusVolumeProfile;
            _focusVolume.weight = _focusVolumeWeight;
            _focusVolume.enabled = false;
        }

        /// <summary>
        ///     Baseカメラの検出対象に含まれるVolume用レイヤーを決定する。
        /// </summary>
        /// <returns> Volumeを配置するレイヤー番号です。 </returns>
        private int ResolveVolumeLayer()
        {
            int volumeLayerMask = _baseCameraData.volumeLayerMask.value;
            for (int layer = 0; layer < LAYER_COUNT; layer++)
            {
                if ((volumeLayerMask & (1 << layer)) != 0)
                {
                    return layer;
                }
            }

            // 検出対象が空の場合はDefaultへ置く。カメラ側の設定漏れに気付けるよう警告する。
            Debug.LogWarning(
                $"[{nameof(PostProccessOverlayCamera)}] BaseカメラのVolume Maskが空のため、Volumeが適用されません。",
                this);
            return DEFAULT_LAYER;
        }

        /// <summary>
        ///     注目レイヤーの描画をBaseとOverlayのどちらへ寄せるか切り替える。
        /// </summary>
        /// <param name="isFocused"> 注目レイヤーをOverlayへ逃がす場合はtrue。 </param>
        private void ApplyFocusRendering(bool isFocused)
        {
            if (_baseCamera == null || _overlayCamera == null)
            {
                return;
            }

            if (isFocused)
            {
                _wasPostProcessingEnabled = _baseCameraData.renderPostProcessing;
                _baseCameraData.renderPostProcessing = true;
                _baseCamera.cullingMask &= ~_focusLayers.value;
                _baseCameraData.cameraStack.Add(_overlayCamera);
                _overlayCamera.enabled = true;

                if (_focusVolume != null)
                {
                    _focusVolume.weight = _focusVolumeWeight;
                    _focusVolume.enabled = true;
                }

                return;
            }

            if (_focusVolume != null)
            {
                _focusVolume.enabled = false;
            }

            _overlayCamera.enabled = false;
            _baseCameraData.cameraStack.Remove(_overlayCamera);
            _baseCamera.cullingMask |= _focusLayers.value;
            _baseCameraData.renderPostProcessing = _wasPostProcessingEnabled;
        }

        private const float DEFAULT_VOLUME_WEIGHT = 1f;
        private const float FOCUS_VOLUME_PRIORITY = 100f;
        private const int LAYER_COUNT = 32;
        private const int DEFAULT_LAYER = 0;
    }
}
