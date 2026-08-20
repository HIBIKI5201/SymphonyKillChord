using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using KillChord.Runtime.View.InGame.Skill.Effect.Placement;
using KillChord.Runtime.View.InGame.Skill.Effect.Presentation;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill.Effect
{
    /// <summary>
    ///     プールで再利用されるスキルエフェクト1つ分のルートView。
    ///     配置の異なる構成要素をまとめて保持し、全再生の完了を待機してプールへの返却を通知する。
    /// </summary>
    public sealed class SkillEffectInstance : MonoBehaviour, ISkillEffectHandle
    {
        /// <summary> 再生中かどうかです。 </summary>
        public bool IsPlaying => _isPlaying;

        /// <summary> シーンロード時に事前生成する数です。 </summary>
        public int PrewarmCount => _prewarmCount;

        /// <summary> プールが保持する最大数です。 </summary>
        public int MaxPoolSize => Mathf.Max(_prewarmCount, _maxPoolSize);

        /// <summary>
        ///     プール生成時の事前準備を行う。
        /// </summary>
        public void Prewarm()
        {
            InitializeParts();
            for (int i = 0; i < _parts.Length; i++)
            {
                SkillEffectPresentationBase[] presentations = _parts[i].Presentations;
                for (int j = 0; j < presentations.Length; j++)
                {
                    presentations[j]?.Prewarm();
                }
            }
        }

        /// <summary>
        ///     構成要素ごとに配置を解決してエフェクトの再生を開始する。
        /// </summary>
        /// <param name="context"> エフェクトの参照点です。 </param>
        /// <param name="onFinished"> 再生完了時に呼ばれるコールバックです。 </param>
        /// <returns> 再生を開始できた場合はtrue。 </returns>
        public bool Play(in SkillEffectContext context, Action<SkillEffectInstance> onFinished)
        {
            InitializeParts();
            if (_parts.Length == 0)
            {
                Debug.LogError($"[{nameof(SkillEffectInstance)}] 構成要素が1つも存在しません。", this);
                return false;
            }

            _context = context;
            _onFinished = onFinished;
            _elapsedSeconds = 0f;
            _isPlaying = true;
            _isLifetimeExceeded = false;

            // 前回の待機者を取り残さないよう、作り直す前に必ず完了させる。
            _completionSource.TrySetResult();
            _completionSource.Reset();
            UpdatePlacements();
            ResetCancellation();

            // 再生自体は非同期だが、完了は必ずRunAsyncで待機し、返却漏れを起こさない。
            _ = RunAsync(context, _cancellationTokenSource.Token);
            return true;
        }

        /// <summary>
        ///     エフェクトの再生完了を待機する。
        /// </summary>
        /// <returns> 再生完了を待機するAwaitableです。 </returns>
        public Awaitable WaitForCompletionAsync()
        {
            return _completionSource.Awaitable;
        }

        /// <summary>
        ///     エフェクトを停止し、プールへの返却を通知する。
        /// </summary>
        public void Stop()
        {
            if (!_isPlaying)
            {
                return;
            }

            // 中断はキャンセルで伝播させ、完了処理はRunAsyncへ一本化する。
            _cancellationTokenSource?.Cancel();
        }

        [SerializeField, Tooltip("配置の異なる構成要素です。1スキル分の演出をここへまとめます。")]
        private SkillEffectPart[] _parts;

        [SerializeField, Min(0), Tooltip("シーンロード時に事前生成する数です。同時再生数の想定値を設定します。")]
        private int _prewarmCount = 1;

        [SerializeField, Min(1), Tooltip("プールが保持する最大数です。")]
        private int _maxPoolSize = 4;

        [SerializeField, Min(0f), Tooltip("再生完了を検出できなかった場合に強制返却するまでの時間です。0なら無効です。")]
        private float _maxLifetimeSeconds = 30f;

        /// <summary>
        ///     構成要素を初期化する。
        /// </summary>
        private void Awake()
        {
            InitializeParts();
        }

        /// <summary>
        ///     追従の更新と最大再生時間の監視を行う。
        /// </summary>
        private void LateUpdate()
        {
            if (!_isPlaying)
            {
                return;
            }

            UpdatePlacements();

            if (_maxLifetimeSeconds <= 0f)
            {
                return;
            }

            _elapsedSeconds += Time.deltaTime;
            if (_elapsedSeconds < _maxLifetimeSeconds || _isLifetimeExceeded)
            {
                return;
            }

            // キャンセルの伝播に複数フレームかかるため、通知は1回だけ行う。
            _isLifetimeExceeded = true;
            Debug.LogWarning($"[{nameof(SkillEffectInstance)}] 最大再生時間を超えたため強制的に返却します。 Effect: {name}", this);
            Stop();
        }

        /// <summary>
        ///     破棄時に再生を中断する。
        /// </summary>
        private void OnDestroy()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        /// <summary>
        ///     構成要素の参照と配置ストラテジーを解決する。
        /// </summary>
        private void InitializeParts()
        {
            _parts ??= Array.Empty<SkillEffectPart>();
            if (_isPartsInitialized)
            {
                return;
            }

            for (int i = 0; i < _parts.Length; i++)
            {
                _parts[i].CachePresentations();
                _parts[i].ResolvePlacement();
            }

            _isPartsInitialized = true;
        }

        /// <summary>
        ///     各構成要素の配置を解決して適用する。
        /// </summary>
        private void UpdatePlacements()
        {
            for (int i = 0; i < _parts.Length; i++)
            {
                SkillEffectPart part = _parts[i];

                // 追従しない構成要素は再生開始時の姿勢を維持する。
                if (_isPlacementApplied && !part.IsFollow)
                {
                    continue;
                }

                if (part.Placement != null && part.Placement.TryResolve(_context, out SkillEffectPose pose))
                {
                    part.ApplyPose(pose);
                }
            }

            _isPlacementApplied = true;
        }

        /// <summary>
        ///     全構成要素の再生を開始し、すべての完了を待機する。
        /// </summary>
        /// <param name="context"> エフェクトの参照点です。 </param>
        /// <param name="cancellationToken"> 再生を中断するためのキャンセルトークンです。 </param>
        /// <returns> 全再生の完了を待機するAwaitableです。 </returns>
        private async Awaitable RunAsync(SkillEffectContext context, CancellationToken cancellationToken)
        {
            _runningPlaybacks.Clear();

            try
            {
                // 先に全ストラテジーを開始し、同時再生させてから完了を待機する。
                for (int i = 0; i < _parts.Length; i++)
                {
                    SkillEffectPresentationBase[] presentations = _parts[i].Presentations;
                    for (int j = 0; j < presentations.Length; j++)
                    {
                        if (presentations[j] == null)
                        {
                            continue;
                        }

                        _runningPlaybacks.Add(presentations[j].PlayAsync(context, cancellationToken));
                    }
                }

                for (int i = 0; i < _runningPlaybacks.Count; i++)
                {
                    await _runningPlaybacks[i];
                }
            }
            catch (OperationCanceledException)
            {
                // 停止要求による中断は正常系のため、そのまま完了処理へ進む。
            }
            catch (Exception exception)
            {
                Debug.LogError($"[{nameof(SkillEffectInstance)}] エフェクトの再生に失敗しました。 Effect: {name}, {exception}", this);
            }
            finally
            {
                CompletePlayback();
            }
        }

        /// <summary>
        ///     再生完了を確定し、待機者とプールへ通知する。
        /// </summary>
        private void CompletePlayback()
        {
            if (!_isPlaying)
            {
                return;
            }

            _isPlaying = false;
            _isPlacementApplied = false;
            _runningPlaybacks.Clear();

            Action<SkillEffectInstance> onFinished = _onFinished;
            _onFinished = null;
            _context = default;

            _completionSource.TrySetResult();
            onFinished?.Invoke(this);
        }

        /// <summary>
        ///     キャンセル用トークンソースを再生開始前の状態へ戻す。
        /// </summary>
        private void ResetCancellation()
        {
            // 一度キャンセルしたトークンソースは再利用できないため、再生ごとに作り直す。
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private readonly AwaitableCompletionSource _completionSource = new();
        private readonly List<Awaitable> _runningPlaybacks = new();
        private CancellationTokenSource _cancellationTokenSource;
        private Action<SkillEffectInstance> _onFinished;
        private SkillEffectContext _context;
        private float _elapsedSeconds;
        private bool _isPlaying;
        private bool _isPartsInitialized;
        private bool _isLifetimeExceeded;
        private bool _isPlacementApplied;
    }
}
