using KillChord.Runtime.Adaptor.InGame.Target;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.UI
{
    /// <summary>
    ///     登録済みの敵から画面外方向表示の対象を選び、表示スロットへ割り当てる。
    /// </summary>
    public sealed class EnemyDirectionIndicatorPresenter
    {
        /// <summary>
        ///     敵方向表示に必要な依存を受け取る。
        /// </summary>
        /// <param name="targetSystemViewModel"> 登録済みターゲットの取得元。 </param>
        /// <param name="viewModel"> 表示情報の反映先。 </param>
        /// <param name="getPlayerPosition"> プレイヤー位置の取得処理。 </param>
        /// <param name="isOutsideViewport"> 対象Boundsが画面外かを判定する処理。 </param>
        /// <param name="maximumDistance"> 表示対象とする最大距離。 </param>
        public EnemyDirectionIndicatorPresenter(
            ITargetSystemViewModel targetSystemViewModel,
            IEnemyDirectionIndicatorViewModel viewModel,
            Func<Vector3> getPlayerPosition,
            Func<Bounds, bool> isOutsideViewport,
            float maximumDistance)
        {
            _targetSystemViewModel = targetSystemViewModel
                ?? throw new ArgumentNullException(nameof(targetSystemViewModel));
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _getPlayerPosition = getPlayerPosition ?? throw new ArgumentNullException(nameof(getPlayerPosition));
            _isOutsideViewport = isOutsideViewport ?? throw new ArgumentNullException(nameof(isOutsideViewport));

            if (float.IsNaN(maximumDistance) || float.IsInfinity(maximumDistance) || maximumDistance < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumDistance));
            }

            if (_viewModel.Capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(viewModel), "表示スロット数は1以上である必要があります。");
            }

            _maximumSqrDistance = maximumDistance * maximumDistance;
            _registeredTargets = new List<ITargetableViewModel>(_viewModel.Capacity);
            _candidates = new List<Candidate>(_viewModel.Capacity);
        }

        /// <summary>
        ///     登録済みターゲットを評価し、全表示スロットを更新する。
        /// </summary>
        public void Update()
        {
            _targetSystemViewModel.CopyRegisteredTargetsTo(_registeredTargets);
            CollectCandidates(_getPlayerPosition());
            _candidates.Sort();

            int selectedCount = Mathf.Min(_viewModel.Capacity, _candidates.Count);
            UpdateSlots(selectedCount);
        }

        /// <summary> 方向判定でゼロベクトルを除外する二乗長の閾値。 </summary>
        private const float DIRECTION_SQR_EPSILON = 0.000001f;

        private readonly ITargetSystemViewModel _targetSystemViewModel;
        private readonly IEnemyDirectionIndicatorViewModel _viewModel;
        private readonly Func<Vector3> _getPlayerPosition;
        private readonly Func<Bounds, bool> _isOutsideViewport;
        private readonly float _maximumSqrDistance;
        private readonly List<ITargetableViewModel> _registeredTargets;
        private readonly List<Candidate> _candidates;

        /// <summary>
        ///     表示条件を満たすターゲットを候補バッファへ収集する。
        /// </summary>
        /// <param name="playerPosition"> 現在のプレイヤー位置。 </param>
        private void CollectCandidates(in Vector3 playerPosition)
        {
            _candidates.Clear();

            for (int i = 0; i < _registeredTargets.Count; i++)
            {
                ITargetableViewModel target = _registeredTargets[i];
                if (target == null || !target.IsAlive)
                {
                    continue;
                }

                Vector3 targetPosition = target.Position;
                Vector3 difference = targetPosition - playerPosition;
                float sqrDistance = difference.sqrMagnitude;
                Bounds targetBounds = target is ITargetBoundsViewModel boundsViewModel
                    ? boundsViewModel.WorldBounds
                    : new Bounds(targetPosition, Vector3.zero);
                if (sqrDistance > _maximumSqrDistance || !_isOutsideViewport(targetBounds))
                {
                    continue;
                }

                difference.y = 0f;
                if (difference.sqrMagnitude <= DIRECTION_SQR_EPSILON)
                {
                    continue;
                }

                _candidates.Add(new Candidate(
                    target.TargetId,
                    difference.normalized,
                    sqrDistance));
            }
        }

        /// <summary>
        ///     距離、TargetId順で全スロットの表示情報をViewModelへ送る。
        /// </summary>
        /// <param name="selectedCount"> 距離順で選ばれた候補数。 </param>
        private void UpdateSlots(int selectedCount)
        {
            for (int slotIndex = 0; slotIndex < _viewModel.Capacity; slotIndex++)
            {
                if (slotIndex < selectedCount)
                {
                    Candidate candidate = _candidates[slotIndex];
                    _viewModel.Update(new EnemyDirectionIndicatorDTO(
                        slotIndex,
                        true,
                        candidate.Direction));
                    continue;
                }

                _viewModel.Update(new EnemyDirectionIndicatorDTO(
                    slotIndex,
                    false,
                    Vector3.zero));
            }
        }

        /// <summary>
        ///     画面外表示候補のTargetId、方向、距離を保持する。
        /// </summary>
        private readonly struct Candidate : IComparable<Candidate>
        {
            /// <summary>
            ///     画面外表示候補を生成する。
            /// </summary>
            /// <param name="targetId"> 対象のTargetId。 </param>
            /// <param name="direction"> プレイヤーから対象への水平方向。 </param>
            /// <param name="sqrDistance"> プレイヤーから対象までの二乗距離。 </param>
            public Candidate(Guid targetId, in Vector3 direction, float sqrDistance)
            {
                TargetId = targetId;
                Direction = direction;
                SqrDistance = sqrDistance;
            }

            /// <summary> 対象のTargetId。 </summary>
            public Guid TargetId { get; }

            /// <summary> プレイヤーから対象への水平方向。 </summary>
            public Vector3 Direction { get; }

            /// <summary> プレイヤーから対象までの二乗距離。 </summary>
            public float SqrDistance { get; }

            /// <summary>
            ///     距離、TargetIdの順で候補を比較する。
            /// </summary>
            /// <param name="other"> 比較対象の候補。 </param>
            /// <returns> 比較結果。 </returns>
            public int CompareTo(Candidate other)
            {
                int distanceComparison = SqrDistance.CompareTo(other.SqrDistance);
                return distanceComparison != 0
                    ? distanceComparison
                    : TargetId.CompareTo(other.TargetId);
            }
        }
    }
}
