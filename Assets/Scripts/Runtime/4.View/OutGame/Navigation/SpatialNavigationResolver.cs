using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Navigation
{
    /// <summary>
    ///     画面上の実座標をもとに、指定方向で最も適切な次のフォーカス候補を探すリゾルバです。
    ///     <para>
    ///         UI Toolkit標準の自動ナビゲーションは、CSSスケール変形や、
    ///         ScrollViewで一部が画面外にはみ出す非格子状のレイアウト(スキルツリーの木構造など)で
    ///         次の要素を正しく見つけられないことがあるため、代わりに使用します。
    ///     </para>
    /// </summary>
    public static class SpatialNavigationResolver
    {
        /// <summary>
        ///     進行方向に対して垂直な向きのズレへ課すペナルティの重み。
        ///     大きいほど、進行方向にまっすぐ近い候補を優先する。
        /// </summary>
        private const float PERPENDICULAR_PENALTY_WEIGHT = 1.5f;

        /// <summary>
        ///     候補として認める、進行方向の距離に対する垂直方向ズレの最大比率。
        ///     木構造の分岐は急角度になりやすいため、角度の固定しきい値ではなく
        ///     この比率で緩やかに許容範囲を決める(概ね約72度まで許容)。
        /// </summary>
        private const float MAX_PERPENDICULAR_TO_PRIMARY_RATIO = 3f;

        /// <summary>
        ///     候補群の中から、現在位置から指定方向に最も適切な要素を探します。
        ///     <para>
        ///         進行方向への距離(主軸成分)を基本の近さとし、進行方向からの横ズレ(垂直成分)に
        ///         ペナルティを加えて評価する。角度による固定的な足切りをしないことで、
        ///         急角度に枝分かれする木構造でも、本来の隣接ノードが遠い候補に負けて
        ///         取りこぼされることを防ぐ。
        ///     </para>
        /// </summary>
        /// <param name="current"> 現在フォーカス中の要素です。 </param>
        /// <param name="candidates"> 移動先候補の要素群です。currentが含まれていても構いません。 </param>
        /// <param name="direction"> 探索する方向です。 </param>
        /// <param name="viewportWorldBound">
        ///     指定した場合、この矩形と重ならない(画面外の)候補を除外します。
        ///     nullの場合は画面外かどうかを考慮しません。
        /// </param>
        /// <returns> 見つかった要素です。見つからない場合はnullです。 </returns>
        public static VisualElement FindNearestInDirection(
            VisualElement current,
            IReadOnlyList<VisualElement> candidates,
            NavigationMoveEvent.Direction direction,
            Rect? viewportWorldBound = null)
        {
            if (current == null || candidates == null)
            {
                return null;
            }

            Vector2 directionVector = ToVector(direction);
            if (directionVector == Vector2.zero)
            {
                return null;
            }

            Vector2 perpendicularVector = new Vector2(-directionVector.y, directionVector.x);
            Vector2 currentCenter = current.worldBound.center;
            VisualElement best = null;
            float bestScore = float.MaxValue;
            int excludedBySelectable = 0;
            int excludedByViewport = 0;
            int excludedByOppositeDirection = 0;
            int excludedByPerpendicularRatio = 0;

            for (int i = 0; i < candidates.Count; i++)
            {
                VisualElement candidate = candidates[i];
                if (candidate == null || ReferenceEquals(candidate, current))
                {
                    continue;
                }

                if (!IsSelectable(candidate))
                {
                    excludedBySelectable++;
                    continue;
                }

                if (viewportWorldBound.HasValue && !viewportWorldBound.Value.Overlaps(candidate.worldBound))
                {
                    excludedByViewport++;
                    continue;
                }

                Vector2 delta = candidate.worldBound.center - currentCenter;
                float primary = Vector2.Dot(delta, directionVector);
                if (primary <= 0f)
                {
                    excludedByOppositeDirection++;
                    continue;
                }

                float perpendicular = Mathf.Abs(Vector2.Dot(delta, perpendicularVector));
                if (perpendicular > primary * MAX_PERPENDICULAR_TO_PRIMARY_RATIO)
                {
                    excludedByPerpendicularRatio++;
                    continue;
                }

                float score = primary + perpendicular * PERPENDICULAR_PENALTY_WEIGHT;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }

            NavigationDebugLog.Log(
                $"[SpatialNav] {NavigationDebugLog.Describe(current)} dir={direction} "
                + $"total={candidates.Count} excluded(selectable={excludedBySelectable}, "
                + $"viewport={excludedByViewport}, oppositeDir={excludedByOppositeDirection}, "
                + $"perpRatio={excludedByPerpendicularRatio}) "
                + $"-> {NavigationDebugLog.Describe(best)} score={(best == null ? "-" : bestScore.ToString("F0"))} "
                + $"currentBound={current.worldBound} viewport={viewportWorldBound}");

            return best;
        }

        /// <summary>
        ///     要素が現在フォーカス移動先として選択可能かどうかを判定します。
        /// </summary>
        /// <param name="element"> 判定する要素です。 </param>
        /// <returns> 選択可能な場合はtrueです。 </returns>
        private static bool IsSelectable(VisualElement element)
        {
            return element.focusable
                && element.enabledInHierarchy
                && element.resolvedStyle.display != DisplayStyle.None
                && element.resolvedStyle.visibility == Visibility.Visible;
        }

        /// <summary>
        ///     ナビゲーション方向を画面座標系のベクトルへ変換します。
        /// </summary>
        /// <param name="direction"> 変換する方向です。 </param>
        /// <returns> 対応するベクトルです。UI ToolkitのY座標は下向きが正のため、上方向は負のYで表します。 </returns>
        private static Vector2 ToVector(NavigationMoveEvent.Direction direction)
        {
            switch (direction)
            {
                case NavigationMoveEvent.Direction.Up:
                    return new Vector2(0f, -1f);
                case NavigationMoveEvent.Direction.Down:
                    return new Vector2(0f, 1f);
                case NavigationMoveEvent.Direction.Left:
                    return new Vector2(-1f, 0f);
                case NavigationMoveEvent.Direction.Right:
                    return new Vector2(1f, 0f);
                default:
                    return Vector2.zero;
            }
        }
    }
}
