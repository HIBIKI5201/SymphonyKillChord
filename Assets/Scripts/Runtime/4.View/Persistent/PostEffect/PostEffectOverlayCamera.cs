using System.Collections.Generic;
using LitMotion;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace KillChord.Runtime.View.Persistent.PostEffect
{
    /// <summary>
    ///     Config単位でポストプロセスの適用を切り替えるBaseカメラ側の制御。
    ///     起動中はConfigの除外レイヤーをポストプロセス無しのOverlayカメラへ逃がし、
    ///     Baseカメラ側にだけConfigのVolumeを適用する。停止時はスタックから外して元へ戻す。
    ///     Configは同時に何個でも起動でき、Volumeは重ねて適用される。
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class PostEffectOverlayCamera : MonoBehaviour, IPostEffectOverlayPlayer
    {
        /// <summary>
        ///     指定Configのポストプロセスを開始する。
        /// </summary>
        /// <param name="config"> 開始する対象のConfigです。 </param>
        public void Add(PostEffectOverlayConfig config)
        {
            if (config == null || _baseCamera == null)
            {
                return;
            }

            if (_overlays.TryGetValue(config, out OverlayEntry entry))
            {
                if (entry.ReferenceCount > 0)
                {
                    // 同じConfigの重複要求は数えるだけで、描画構成は変えない。
                    entry.ReferenceCount++;
                    return;
                }

                // フェードアウト中の再要求は、モーションを完了させて辞書から消し切ってから作り直す。
                entry.Handle.TryComplete();
                FinalizeRemove(config, entry);
            }

            // 最初の1件を起動する時だけ、Base側のポストプロセスを有効化する。
            if (_overlays.Count == 0)
            {
                _wasPostProcessingEnabled = _baseCameraData.renderPostProcessing;
                _baseCameraData.renderPostProcessing = true;
            }

            entry = CreateEntry(config);
            _overlays.Add(config, entry);
            ApplyOverlayRendering(entry, true);
            PlayFadeIn(config, entry);
        }

        /// <summary>
        ///     指定Configのポストプロセスを取り下げる。
        ///     フェードアウトの秒数が設定されている場合は、下がり切ってから破棄する。
        /// </summary>
        /// <param name="config"> 取り下げる対象のConfigです。 </param>
        public void Remove(PostEffectOverlayConfig config)
        {
            if (config == null || !_overlays.TryGetValue(config, out OverlayEntry entry))
            {
                return;
            }

            // フェードアウト中のエントリはすでに取り下げ済みのため、参照数を割り込ませない。
            if (entry.ReferenceCount <= 0)
            {
                return;
            }

            entry.ReferenceCount--;
            if (entry.ReferenceCount > 0)
            {
                return;
            }

            // フェードインが残っていると開始値が競合するため、先に完了させる。
            entry.Handle.TryComplete();

            if (!TryPlayFadeOut(config, entry))
            {
                FinalizeRemove(config, entry);
            }
        }

        /// <summary>
        ///     待機したのち、指定秒数だけポストプロセスを開始する。
        /// </summary>
        /// <param name="config"> 開始する対象のConfigです。 </param>
        /// <param name="delaySeconds"> 開始までの待機秒数です。0以下なら即座に開始します。 </param>
        /// <param name="durationSeconds"> 継続する秒数です。0以下なら何もしません。フェードアウトはこの秒数の経過後に始まります。 </param>
        public void AddForSeconds(PostEffectOverlayConfig config, float delaySeconds, float durationSeconds)
        {
            if (config == null || durationSeconds <= 0f)
            {
                return;
            }

            float delay = Mathf.Max(0f, delaySeconds);
            TimedReservation reservation = new TimedReservation(config, delay, durationSeconds);

            // 待機が無い場合はフレームを跨がずに開始し、要求と同時に見えるようにする。
            if (delay <= 0f)
            {
                reservation = reservation.AsStarted();
                Add(config);
            }

            _reservations.Add(reservation);
        }

        /// <summary>
        ///     指定Configが適用中かどうかを取得する。
        /// </summary>
        /// <param name="config"> 判定する対象のConfigです。 </param>
        /// <returns> 適用中の場合はtrue。 </returns>
        public bool IsActive(PostEffectOverlayConfig config)
        {
            return config != null && _overlays.ContainsKey(config);
        }

        /// <summary>
        ///     すべてのポストプロセスを取り下げる。
        /// </summary>
        public void RemoveAll()
        {
            _reservations.Clear();

            if (_overlays.Count == 0)
            {
                return;
            }

            foreach (KeyValuePair<PostEffectOverlayConfig, OverlayEntry> pair in _overlays)
            {
                ApplyOverlayRendering(pair.Value, false);
                DestroyEntry(pair.Value);
            }

            _overlays.Clear();
            _baseCameraData.renderPostProcessing = _wasPostProcessingEnabled;
        }

        private Camera _baseCamera;
        private UniversalAdditionalCameraData _baseCameraData;
        private bool _wasPostProcessingEnabled;
        private int _volumeLayer = NO_LAYER;

        private readonly Dictionary<PostEffectOverlayConfig, OverlayEntry> _overlays = new();
        private readonly List<TimedReservation> _reservations = new();

        /// <summary>
        ///     Base側のカメラ参照とVolumeの配置先レイヤーを解決する。
        /// </summary>
        private void Awake()
        {
            _baseCamera = GetComponent<Camera>();
            _baseCameraData = GetComponent<UniversalAdditionalCameraData>();

            if (_baseCameraData == null)
            {
                Debug.LogError(
                    $"[{nameof(PostEffectOverlayCamera)}] {nameof(UniversalAdditionalCameraData)} が必要です。",
                    this);
                _baseCamera = null;
                return;
            }

            _volumeLayer = ResolveVolumeLayer();
        }

        /// <summary>
        ///     時間指定の予約を進め、開始と終了を反映する。
        /// </summary>
        private void Update()
        {
            if (_reservations.Count == 0)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            for (int i = _reservations.Count - 1; i >= 0; i--)
            {
                TimedReservation reservation = _reservations[i].Advance(deltaTime);

                if (!reservation.HasStarted && reservation.IsStartReached)
                {
                    reservation = reservation.AsStarted();
                    Add(reservation.Config);
                }

                if (reservation.HasStarted && reservation.IsEndReached)
                {
                    _reservations.RemoveAt(i);
                    Remove(reservation.Config);
                    continue;
                }

                _reservations[i] = reservation;
            }
        }

        /// <summary>
        ///     破棄時に描画構成を元へ戻す。
        /// </summary>
        private void OnDestroy()
        {
            RemoveAll();
        }

        /// <summary>
        ///     Config1件分のOverlayカメラとVolumeを生成する。
        /// </summary>
        /// <param name="config"> 生成元のConfigです。 </param>
        /// <returns> 生成したエントリです。 </returns>
        private OverlayEntry CreateEntry(PostEffectOverlayConfig config)
        {
            GameObject overlayObject = new GameObject($"{nameof(PostEffectOverlayCamera)}_{config.name}");
            overlayObject.transform.SetParent(transform, false);

            Camera overlayCamera = overlayObject.AddComponent<Camera>();

            // 画角や描画距離をBase側から引き継ぎ、除外レイヤーの見え方を一致させる。
            overlayCamera.CopyFrom(_baseCamera);
            overlayCamera.cullingMask = config.ExcludedLayers.value;
            overlayCamera.clearFlags = CameraClearFlags.Depth;
            overlayCamera.enabled = false;

            UniversalAdditionalCameraData overlayData = overlayObject.AddComponent<UniversalAdditionalCameraData>();
            overlayData.renderType = CameraRenderType.Overlay;

            // 除外レイヤーはVolumeの影響を受けさせない。
            overlayData.renderPostProcessing = false;

            Volume volume = CreateVolume(config);
            return new OverlayEntry(overlayCamera, volume);
        }

        /// <summary>
        ///     Baseカメラが検出するGlobal Volumeを生成する。
        /// </summary>
        /// <param name="config"> 生成元のConfigです。 </param>
        /// <returns> 生成したVolumeです。生成できない場合はnull。 </returns>
        private Volume CreateVolume(PostEffectOverlayConfig config)
        {
            if (config.VolumeProfile == null)
            {
                Debug.LogWarning(
                    $"[{nameof(PostEffectOverlayCamera)}] {nameof(VolumeProfile)} が未設定です。 Config: {config.name}",
                    this);
                return null;
            }

            if (_volumeLayer == NO_LAYER)
            {
                return null;
            }

            GameObject volumeObject = new GameObject($"{nameof(Volume)}_{config.name}");
            volumeObject.transform.SetParent(transform, false);
            volumeObject.layer = _volumeLayer;

            Volume volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = OVERLAY_VOLUME_PRIORITY;
            volume.sharedProfile = config.VolumeProfile;

            // 強さはフェードで持ち上げるため、生成時点では効かせない。
            volume.weight = 0f;
            return volume;
        }

        /// <summary>
        ///     Volumeの強さを現在値からConfigの設定値まで持ち上げる。
        /// </summary>
        /// <param name="config"> 適用する対象のConfigです。 </param>
        /// <param name="entry"> 対象のエントリです。 </param>
        private void PlayFadeIn(PostEffectOverlayConfig config, OverlayEntry entry)
        {
            if (entry.Volume == null)
            {
                return;
            }

            // フェード時間が無い場合はモーションを作らず、そのフレームから効かせる。
            if (config.FadeInSeconds <= 0f)
            {
                entry.Volume.weight = config.VolumeWeight;
                return;
            }

            entry.Handle = LMotion.Create(entry.Volume.weight, config.VolumeWeight, config.FadeInSeconds)
                .WithEase(config.FadeEase)
                .WithScheduler(MotionScheduler.Update)
                .Bind(entry.Volume, static (weight, volume) => volume.weight = weight);
        }

        /// <summary>
        ///     Volumeの強さを0まで下げ、下がり切った時点で破棄するモーションを開始する。
        /// </summary>
        /// <param name="config"> 取り下げる対象のConfigです。 </param>
        /// <param name="entry"> 対象のエントリです。 </param>
        /// <returns> フェードアウトを開始できた場合はtrue。 </returns>
        private bool TryPlayFadeOut(PostEffectOverlayConfig config, OverlayEntry entry)
        {
            if (entry.Volume == null || config.FadeOutSeconds <= 0f)
            {
                return false;
            }

            entry.Handle = LMotion.Create(entry.Volume.weight, 0f, config.FadeOutSeconds)
                .WithEase(config.FadeEase)
                .WithScheduler(MotionScheduler.Update)
                .WithOnComplete(() => FinalizeRemove(config, entry))
                .Bind(entry.Volume, static (weight, volume) => volume.weight = weight);
            return true;
        }

        /// <summary>
        ///     取り下げの後始末として描画構成を戻し、エントリを破棄する。
        /// </summary>
        /// <param name="config"> 取り下げる対象のConfigです。 </param>
        /// <param name="entry"> 対象のエントリです。 </param>
        private void FinalizeRemove(PostEffectOverlayConfig config, OverlayEntry entry)
        {
            // フェードアウトの完了と再要求時のTryCompleteの双方から呼ばれるため、二重実行を弾く。
            if (!_overlays.TryGetValue(config, out OverlayEntry current) || current != entry)
            {
                return;
            }

            ApplyOverlayRendering(entry, false);
            _overlays.Remove(config);
            DestroyEntry(entry);

            if (_overlays.Count == 0)
            {
                _baseCameraData.renderPostProcessing = _wasPostProcessingEnabled;
            }
        }

        /// <summary>
        ///     BaseカメラのVolume検出対象に含まれるレイヤーを決定する。
        /// </summary>
        /// <returns> Volumeを配置するレイヤー番号です。検出対象が空の場合は-1。 </returns>
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

            Debug.LogWarning(
                $"[{nameof(PostEffectOverlayCamera)}] BaseカメラのVolume Maskが空のため、Volumeが適用されません。",
                this);
            return NO_LAYER;
        }

        /// <summary>
        ///     除外レイヤーの描画をBaseとOverlayのどちらへ寄せるか切り替える。
        /// </summary>
        /// <param name="entry"> 切り替える対象のエントリです。 </param>
        /// <param name="isOverlaid"> 除外レイヤーをOverlayへ逃がす場合はtrue。 </param>
        private void ApplyOverlayRendering(OverlayEntry entry, bool isOverlaid)
        {
            if (_baseCamera == null || entry.Camera == null)
            {
                return;
            }

            int excludedLayers = entry.Camera.cullingMask;

            if (isOverlaid)
            {
                _baseCamera.cullingMask &= ~excludedLayers;
                _baseCameraData.cameraStack.Add(entry.Camera);
                entry.Camera.enabled = true;
                return;
            }

            entry.Camera.enabled = false;
            _baseCameraData.cameraStack.Remove(entry.Camera);

            // 他のConfigがまだ除外しているレイヤーは、Base側へ戻さない。
            _baseCamera.cullingMask |= excludedLayers & ~CollectExcludedLayers(entry);
        }

        /// <summary>
        ///     指定エントリを除いた、起動中のConfigが除外しているレイヤーを集計する。
        /// </summary>
        /// <param name="exclusion"> 集計から外すエントリです。 </param>
        /// <returns> 集計した除外レイヤーのマスクです。 </returns>
        private int CollectExcludedLayers(OverlayEntry exclusion)
        {
            int mask = 0;
            foreach (KeyValuePair<PostEffectOverlayConfig, OverlayEntry> pair in _overlays)
            {
                if (pair.Value == exclusion)
                {
                    continue;
                }

                mask |= pair.Value.Camera.cullingMask;
            }

            return mask;
        }

        /// <summary>
        ///     エントリが保持するオブジェクトを破棄する。
        /// </summary>
        /// <param name="entry"> 破棄する対象のエントリです。 </param>
        private void DestroyEntry(OverlayEntry entry)
        {
            // 破棄後のVolumeへモーションが書き込まないよう、先に止める。
            entry.Handle.TryCancel();

            if (entry.Volume != null)
            {
                Destroy(entry.Volume.gameObject);
            }

            if (entry.Camera != null)
            {
                Destroy(entry.Camera.gameObject);
            }
        }

        private const int LAYER_COUNT = 32;
        private const int NO_LAYER = -1;
        private const float OVERLAY_VOLUME_PRIORITY = 100f;

        /// <summary>
        ///     起動中のConfig1件分の描画資源を保持するクラス。
        /// </summary>
        private sealed class OverlayEntry
        {
            /// <summary>
            ///     エントリを生成する。
            /// </summary>
            /// <param name="camera"> Overlay描画用カメラです。 </param>
            /// <param name="volume"> 適用するVolumeです。 </param>
            public OverlayEntry(Camera camera, Volume volume)
            {
                Camera = camera;
                Volume = volume;
                ReferenceCount = 1;
            }

            /// <summary> Overlay描画用カメラです。 </summary>
            public Camera Camera { get; }

            /// <summary> 適用するVolumeです。 </summary>
            public Volume Volume { get; }

            /// <summary> 現在の要求数です。0以下はフェードアウト中を表します。 </summary>
            public int ReferenceCount { get; set; }

            /// <summary> Volumeの強さを変化させている再生中のモーションです。 </summary>
            public MotionHandle Handle { get; set; }
        }

        /// <summary>
        ///     時間指定で開始と終了を行う予約1件分。
        /// </summary>
        private readonly struct TimedReservation
        {
            /// <summary>
            ///     予約を生成する。
            /// </summary>
            /// <param name="config"> 対象のConfigです。 </param>
            /// <param name="startSeconds"> 開始までの待機秒数です。 </param>
            /// <param name="durationSeconds"> 継続する秒数です。 </param>
            public TimedReservation(PostEffectOverlayConfig config, float startSeconds, float durationSeconds)
            {
                Config = config;
                HasStarted = false;
                _elapsedSeconds = 0f;
                _startSeconds = startSeconds;
                _endSeconds = startSeconds + durationSeconds;
            }

            /// <summary> 対象のConfigです。 </summary>
            public PostEffectOverlayConfig Config { get; }

            /// <summary> 開始済みかどうかです。 </summary>
            public bool HasStarted { get; }

            /// <summary> 開始時刻へ到達したかどうかです。 </summary>
            public bool IsStartReached => _elapsedSeconds >= _startSeconds;

            /// <summary> 終了時刻へ到達したかどうかです。 </summary>
            public bool IsEndReached => _elapsedSeconds >= _endSeconds;

            /// <summary>
            ///     経過時間を進めた予約を返す。
            /// </summary>
            /// <param name="deltaSeconds"> 進める秒数です。 </param>
            /// <returns> 経過時間を進めた予約です。 </returns>
            public TimedReservation Advance(float deltaSeconds)
            {
                return new TimedReservation(this, HasStarted, _elapsedSeconds + deltaSeconds);
            }

            /// <summary>
            ///     開始済みにした予約を返す。
            /// </summary>
            /// <returns> 開始済みの予約です。 </returns>
            public TimedReservation AsStarted()
            {
                return new TimedReservation(this, true, _elapsedSeconds);
            }

            /// <summary>
            ///     経過状態だけを差し替えた予約を生成する。
            /// </summary>
            /// <param name="source"> 複製元の予約です。 </param>
            /// <param name="hasStarted"> 開始済みかどうかです。 </param>
            /// <param name="elapsedSeconds"> 経過秒数です。 </param>
            private TimedReservation(in TimedReservation source, bool hasStarted, float elapsedSeconds)
            {
                Config = source.Config;
                HasStarted = hasStarted;
                _elapsedSeconds = elapsedSeconds;
                _startSeconds = source._startSeconds;
                _endSeconds = source._endSeconds;
            }

            private readonly float _elapsedSeconds;
            private readonly float _startSeconds;
            private readonly float _endSeconds;
        }
    }
}
