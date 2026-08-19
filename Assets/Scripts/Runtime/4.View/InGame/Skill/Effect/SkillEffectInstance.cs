using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using KillChord.Runtime.View.InGame.Skill.Effect.Placement;
using KillChord.Runtime.View.InGame.Skill.Effect.Presentation;
using System;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill.Effect
{
    /// <summary>
    ///     プールで再利用されるスキルエフェクト1体分のルートView。
    ///     配置ストラテジーと再生ストラテジーを束ね、全再生の完了を待機してプールへの返却を通知する。
    /// </summary>
    public sealed class SkillEffectInstance : MonoBehaviour, ISkillEffectHandle
    {
        /// <summary> 再生中かどうかです。 </summary>
        public bool IsPlaying => _isPlaying;

        /// <summary>
        ///     プール生成時の事前準備を行う。
        /// </summary>
        public void Prewarm()
        {
            CachePresentations();
            for (int i = 0; i < _presentations.Length; i++)
            {
                _presentations[i]?.Prewarm();
            }
        }

        /// <summary>
        ///     配置ストラテジーに従ってエフェクトの再生を開始する。
        /// </summary>
        /// <param name="placement"> 使用する配置ストラテジーです。 </param>
        /// <param name="context"> エフェクトの参照点です。 </param>
        /// <param name="onFinished"> 再生完了時に呼ばれるコールバックです。 </param>
        /// <returns> 再生を開始できた場合はtrue。 </returns>
        public bool Play(ISkillEffectPlacement placement, in SkillEffectContext context, Action<SkillEffectInstance> onFinished)
        {
            if (placement == null || !placement.TryResolve(context, out SkillEffectPose pose))
            {
                return false;
            }

            CachePresentations();
            if (_presentations.Length == 0)
            {
                Debug.LogError($"[{nameof(SkillEffectInstance)}] 再生ストラテジーが1つも存在しません。", this);
                return false;
            }

            _placement = placement.IsFollow ? placement : null;
            _context = context;
            _onFinished = onFinished;
            _elapsedSeconds = 0f;
            _isPlaying = true;
            _completionSource.Reset();
            ApplyPose(pose.Position, pose.Rotation);
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

        [SerializeField, Tooltip("このエフェクトが束ねる再生ストラテジーです。未設定時は子階層から収集します。")]
        private SkillEffectPresentationBase[] _presentations;

        [SerializeField, Tooltip("配置位置に加算するローカルオフセットです。")]
        private Vector3 _positionOffset;

        [SerializeField, Tooltip("配置回転に加算するオイラー角オフセットです。")]
        private Vector3 _rotationOffset;

        [SerializeField, Tooltip("追従型のとき、対象の回転にも追従するかです。")]
        private bool _followsRotation = true;

        [SerializeField, Min(0f), Tooltip("再生完了を検出できなかった場合に強制返却するまでの時間です。0なら無効です。")]
        private float _maxLifetimeSeconds = 30f;

        /// <summary>
        ///     再生ストラテジーの参照を収集する。
        /// </summary>
        private void Awake()
        {
            CachePresentations();
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

            UpdateFollow();

            if (_maxLifetimeSeconds <= 0f)
            {
                return;
            }

            _elapsedSeconds += Time.deltaTime;
            if (_elapsedSeconds < _maxLifetimeSeconds)
            {
                return;
            }

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
        ///     全再生ストラテジーを開始し、すべての完了を待機する。
        /// </summary>
        /// <param name="context"> エフェクトの参照点です。 </param>
        /// <param name="cancellationToken"> 再生を中断するためのキャンセルトークンです。 </param>
        /// <returns> 全再生の完了を待機するAwaitableです。 </returns>
        private async Awaitable RunAsync(SkillEffectContext context, CancellationToken cancellationToken)
        {
            if (_runningPlaybacks == null || _runningPlaybacks.Length != _presentations.Length)
            {
                _runningPlaybacks = new Awaitable[_presentations.Length];
            }

            try
            {
                // 先に全ストラテジーを開始し、同時再生させてから完了を待機する。
                for (int i = 0; i < _presentations.Length; i++)
                {
                    _runningPlaybacks[i] = _presentations[i]?.PlayAsync(context, cancellationToken);
                }

                for (int i = 0; i < _runningPlaybacks.Length; i++)
                {
                    if (_runningPlaybacks[i] == null)
                    {
                        continue;
                    }

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
            Array.Clear(_runningPlaybacks, 0, _runningPlaybacks.Length);

            Action<SkillEffectInstance> onFinished = _onFinished;
            _onFinished = null;
            _placement = null;
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

        /// <summary>
        ///     再生ストラテジーの参照をキャッシュする。
        /// </summary>
        private void CachePresentations()
        {
            if (_presentations != null && _presentations.Length > 0)
            {
                return;
            }

            // インスペクタ未設定時のみ、生成時に一度だけ子階層から収集する。
            _presentations = GetComponentsInChildren<SkillEffectPresentationBase>(true);
        }

        /// <summary>
        ///     追従型の場合に、配置ストラテジーへ姿勢を再計算させる。
        /// </summary>
        private void UpdateFollow()
        {
            // 追従先はTransformとは限らないため、位置の決定はストラテジーへ委ねる。
            if (_placement == null || !_placement.TryResolve(_context, out SkillEffectPose pose))
            {
                return;
            }

            ApplyPose(pose.Position, pose.Rotation);
        }

        /// <summary>
        ///     オフセットを加味した姿勢を適用する。
        /// </summary>
        /// <param name="position"> 基準となるワールド座標です。 </param>
        /// <param name="rotation"> 基準となるワールド回転です。 </param>
        private void ApplyPose(Vector3 position, Quaternion rotation)
        {
            Quaternion appliedRotation = _followsRotation || _placement == null
                ? rotation * Quaternion.Euler(_rotationOffset)
                : transform.rotation;
            transform.SetPositionAndRotation(position + rotation * _positionOffset, appliedRotation);
        }

        private readonly AwaitableCompletionSource _completionSource = new();
        private Awaitable[] _runningPlaybacks;
        private CancellationTokenSource _cancellationTokenSource;
        private Action<SkillEffectInstance> _onFinished;
        private ISkillEffectPlacement _placement;
        private SkillEffectContext _context;
        private float _elapsedSeconds;
        private bool _isPlaying;
    }
}
