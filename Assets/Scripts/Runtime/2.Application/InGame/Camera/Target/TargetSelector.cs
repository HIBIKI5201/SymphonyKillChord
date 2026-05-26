using KillChord.Runtime.Domain.InGame.Camera.Target;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Camera.Target
{
    /// <summary>
    ///     ロックオン対象の選択・切り替え・位置取得を管理するクラス。
    /// </summary>
    public sealed class TargetSelector
    {
        /// <summary>
        ///     ロックオン対象の一覧マネージャーを受け取り、ターゲット選択機能を初期化するコンストラクタ。
        /// </summary>
        /// <param name="manager"> ロックオン対象の一覧を管理するマネージャー。</param>
        public TargetSelector(TargetManager manager)
        {
            _manager = manager;
        }

        /// <summary>
        ///     現在のロックオン対象の位置の取得を試みる。
        ///     対象が存在しないか無効な場合はfalseを返す。
        /// </summary>
        /// <param name="playerPosition"> プレイヤーの現在位置。</param>
        /// <param name="direction"> カメラの向いている方向。</param>
        /// <param name="result"> 取得したロックオン対象のワールド座標。取得失敗時は Vector3.zero。</param>
        /// <returns> 取得に成功した場合は true。</returns>
        public bool TryGetTargetPosition(in Vector3 playerPosition, in Vector3 direction, out Vector3 result)
        {
            result = Vector3.zero;
            if (_manager.Count == 0)
            {
                return false;
            }
            if (_currentTarget is null)
            {
                return false;
            }
            if (!_currentTarget.IsAlive)
            {
                _currentTarget = null;
                return false;
            }
            result = _currentTarget.Position;

            return true;
        }

        /// <summary>
        ///     現在のロックオン対象インターフェースの取得を試みる。
        ///     対象が無効になっていた場合は内部状態をクリアしてfalseを返す。
        /// </summary>
        /// <param name="result"> 現在のロックオン対象。取得失敗時は null。</param>
        /// <returns> 有効な対象が存在する場合は true。</returns>
        public bool TryGetCurrentTarget(out ILockOnTarget result)
        {
            result = _currentTarget;

            if (result is null)
            {
                return false;
            }

            if (!result.IsAlive)
            {
                _currentTarget = null;
                result = null;
                return false;
            }

            return true;
        }

        /// <summary>
        ///     プレイヤー位置と方向をもとに最適なロックオン対象を選択して切り替える。
        /// </summary>
        /// <param name="playerPosition"> プレイヤーの現在位置。</param>
        /// <param name="direction"> カメラの向いている方向。</param>
        public void ChangeTarget(in Vector3 playerPosition, in Vector3 direction)
        {
            GetTargetPosition(playerPosition, direction, out _currentTarget);
        }

        /// <summary> NormalizeDot で使用するゼロ除算回避の下限閾値。 </summary>
        private const float NORMALIZE_DOT_EPSILON = 1E-15f;

        /// <summary> 内積の探索初期値。全方向を候補にするために最小値を設定する。 </summary>
        private const float DOT_INITIAL_VALUE = -1f;

        /// <summary> 視界内外の判定に使用する内積の閾値。0未満で視界外とみなす。 </summary>
        private const float DOT_FORWARD_THRESHOLD = 0f;

        private readonly TargetManager _manager;
        private ILockOnTarget _currentTarget;

        /// <summary>
        ///     二つのベクトルの正規化された内積を返す。
        ///     ベクトルの大きさが極小の場合は0を返す。
        /// </summary>
        /// <param name="from"> 基準ベクトル。</param>
        /// <param name="to"> 比較ベクトル。</param>
        /// <returns> 正規化された内積（-1〜1）。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float NormalizeDot(in Vector3 from, in Vector3 to)
        {
            float num = Mathf.Sqrt(from.sqrMagnitude * to.sqrMagnitude);
            if (num < NORMALIZE_DOT_EPSILON)
            {
                return 0f;
            }

            return Mathf.Clamp(Vector3.Dot(from, to) / num, -1f, 1f);
        }

        /// <summary>
        ///     優先順位に従い最適なロックオン対象を選択する。
        ///     優先順位は「視界内でカメラ方向に近い敵」→「視界内の敵」→「視界外の近くの敵」→「視界外の敵」の順。
        /// </summary>
        /// <param name="center"> 基準となるプレイヤーの位置。</param>
        /// <param name="dir"> カメラの向いている方向。</param>
        /// <param name="result"> 選択されたロックオン対象。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetTargetPosition(in Vector3 center, in Vector3 dir, out ILockOnTarget result)
        {
            ILockOnTarget shortestTarget = null;
            ILockOnTarget bestAlignedTarget = null;
            float shortestDist = float.PositiveInfinity;
            float bestDot = DOT_INITIAL_VALUE;

            float dot;
            float sqrDist;
            Vector3 pos;
            foreach (var item in _manager.GetTargets)
            {
                pos = item.Position;
                dot = NormalizeDot(dir, pos - center);
                if (dot >= bestDot)
                {
                    bestDot = dot;
                    bestAlignedTarget = item;
                }

                sqrDist = Vector3.SqrMagnitude(pos - center);
                if (sqrDist < shortestDist)
                {
                    shortestDist = sqrDist;
                    shortestTarget = item;
                }
            }

            // 内積が閾値未満の場合（視界外）は最近傍対象を優先する
            result = (bestDot < DOT_FORWARD_THRESHOLD) ? shortestTarget : bestAlignedTarget;
        }
    }
}
