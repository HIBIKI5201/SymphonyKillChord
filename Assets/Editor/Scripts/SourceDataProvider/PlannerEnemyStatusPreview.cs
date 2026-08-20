using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.SourceDataProvider
{
    /// <summary>
    ///     敵キャラクターの主要ステータスをレーダーグラフで表示します。
    /// </summary>
    internal static class PlannerEnemyStatusPreview
    {
        /// <summary>
        ///     指定SourceAssetが敵ステータスプレビュー対象か判定します。
        /// </summary>
        /// <param name="addressableKey"> SourceAssetのAddressableキーです。 </param>
        /// <param name="sourceAsset"> 対象SourceAssetです。 </param>
        /// <returns> 敵キャラクターとして表示する場合はtrueです。 </returns>
        public static bool CanDraw(string addressableKey, ScriptableObject sourceAsset)
        {
            if (sourceAsset == null
                || (!string.Equals(addressableKey, ENEMY_ADDRESSABLE_KEY, StringComparison.Ordinal)
                    && !addressableKey.Contains(BOSS_KEYWORD, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            SerializedObject serializedObject = new(sourceAsset);
            return serializedObject.FindProperty(MAX_HEALTH_PROPERTY_NAME) != null;
        }

        /// <summary>
        ///     敵ステータスのレーダーグラフを描画します。
        /// </summary>
        /// <param name="sourceAsset"> 敵キャラクターSourceAssetです。 </param>
        public static void Draw(ScriptableObject sourceAsset)
        {
            List<Metric> metrics = BuildMetrics(sourceAsset);
            if (metrics.Count < MINIMUM_METRIC_COUNT)
            {
                EditorGUILayout.HelpBox(
                    "レーダーグラフに必要な敵ステータスを取得できません。",
                    MessageType.None);
                return;
            }

            EditorGUILayout.LabelField("Enemy Status", EditorStyles.boldLabel);
            Rect graphRect = GUILayoutUtility.GetRect(
                GRAPH_SIZE,
                GRAPH_SIZE,
                GUILayout.ExpandWidth(false));
            Vector2 center = graphRect.center;
            float radius = GRAPH_SIZE * 0.32f;

            DrawRadarGrid(center, radius, metrics.Count);
            DrawRadarValues(center, radius, metrics);
            DrawMetricLabels(center, radius, metrics);
        }

        /// <summary>
        ///     SourceAssetからグラフ指標を構築します。
        /// </summary>
        /// <param name="sourceAsset"> 敵キャラクターSourceAssetです。 </param>
        /// <returns> グラフ指標一覧です。 </returns>
        private static List<Metric> BuildMetrics(ScriptableObject sourceAsset)
        {
            SerializedObject serializedObject = new(sourceAsset);
            float health = serializedObject.FindProperty(MAX_HEALTH_PROPERTY_NAME)?.floatValue ?? 0f;
            float baseDamage = serializedObject.FindProperty(BASE_DAMAGE_PROPERTY_NAME)?.intValue ?? 0;
            float attackInterval = serializedObject.FindProperty(ATTACK_INTERVAL_PROPERTY_NAME)?.floatValue ?? 0f;
            SerializedProperty attacks = serializedObject.FindProperty(ATTACK_DEFINITIONS_PROPERTY_NAME);
            int attackCount = attacks?.arraySize ?? 0;
            CalculateAttackAverages(attacks, out float confirmedDamage, out float justMultiplier);

            return new List<Metric>
            {
                new("体力", health, health / HEALTH_PREVIEW_MAX),
                new("基礎攻撃", baseDamage, baseDamage / BASE_DAMAGE_PREVIEW_MAX),
                new(
                    "攻撃速度",
                    attackInterval > 0f ? 1f / attackInterval : 0f,
                    attackInterval > 0f ? (1f / attackInterval) / ATTACK_SPEED_PREVIEW_MAX : 0f),
                new("攻撃数", attackCount, attackCount / ATTACK_COUNT_PREVIEW_MAX),
                new("攻撃威力", confirmedDamage, confirmedDamage / CONFIRMED_DAMAGE_PREVIEW_MAX),
                new("Just倍率", justMultiplier, justMultiplier / JUST_MULTIPLIER_PREVIEW_MAX),
            };
        }

        /// <summary>
        ///     攻撃定義一覧から確定ダメージとJust倍率の平均を計算します。
        /// </summary>
        /// <param name="attacks"> 攻撃定義配列です。 </param>
        /// <param name="confirmedDamage"> 確定ダメージ平均です。 </param>
        /// <param name="justMultiplier"> Just倍率平均です。 </param>
        private static void CalculateAttackAverages(
            SerializedProperty attacks,
            out float confirmedDamage,
            out float justMultiplier)
        {
            confirmedDamage = 0f;
            justMultiplier = 0f;
            if (attacks == null || !attacks.isArray || attacks.arraySize == 0)
            {
                return;
            }

            int validCount = 0;
            for (int i = 0; i < attacks.arraySize; i++)
            {
                if (attacks.GetArrayElementAtIndex(i).objectReferenceValue
                    is not ScriptableObject attackDefinition)
                {
                    continue;
                }

                SerializedObject serializedAttack = new(attackDefinition);
                justMultiplier += serializedAttack.FindProperty(JUST_DAMAGE_MULTIPLIER_PROPERTY_NAME)
                    ?.floatValue ?? 0f;
                if (serializedAttack.FindProperty(ATTACK_SPEC_PROPERTY_NAME)?.objectReferenceValue
                    is ScriptableObject attackSpec)
                {
                    SerializedObject serializedSpec = new(attackSpec);
                    confirmedDamage += serializedSpec.FindProperty(CONFIRMED_DAMAGE_PROPERTY_NAME)
                        ?.floatValue ?? 0f;
                }
                validCount++;
            }

            if (validCount > 0)
            {
                confirmedDamage /= validCount;
                justMultiplier /= validCount;
            }
        }

        /// <summary>
        ///     レーダーグラフの目盛りと軸を描画します。
        /// </summary>
        /// <param name="center"> グラフ中心です。 </param>
        /// <param name="radius"> グラフ半径です。 </param>
        /// <param name="axisCount"> 軸数です。 </param>
        private static void DrawRadarGrid(Vector2 center, float radius, int axisCount)
        {
            Handles.color = GRID_COLOR;
            for (int level = 1; level <= GRID_LEVEL_COUNT; level++)
            {
                Vector3[] points = BuildPolygonPoints(
                    center,
                    radius * level / GRID_LEVEL_COUNT,
                    axisCount,
                    null);
                Handles.DrawAAPolyLine(GRID_LINE_WIDTH, points);
            }

            for (int i = 0; i < axisCount; i++)
            {
                Vector2 direction = GetAxisDirection(i, axisCount);
                Handles.DrawLine(center, center + direction * radius);
            }
        }

        /// <summary>
        ///     ステータス値の多角形を描画します。
        /// </summary>
        /// <param name="center"> グラフ中心です。 </param>
        /// <param name="radius"> グラフ半径です。 </param>
        /// <param name="metrics"> ステータス指標一覧です。 </param>
        private static void DrawRadarValues(Vector2 center, float radius, List<Metric> metrics)
        {
            Vector3[] points = BuildPolygonPoints(center, radius, metrics.Count, metrics);
            Handles.color = VALUE_FILL_COLOR;
            Handles.DrawAAConvexPolygon(points);
            Handles.color = VALUE_LINE_COLOR;
            Handles.DrawAAPolyLine(VALUE_LINE_WIDTH, points);
        }

        /// <summary>
        ///     各軸の名称と実値を描画します。
        /// </summary>
        /// <param name="center"> グラフ中心です。 </param>
        /// <param name="radius"> グラフ半径です。 </param>
        /// <param name="metrics"> ステータス指標一覧です。 </param>
        private static void DrawMetricLabels(Vector2 center, float radius, List<Metric> metrics)
        {
            for (int i = 0; i < metrics.Count; i++)
            {
                Vector2 direction = GetAxisDirection(i, metrics.Count);
                Vector2 labelCenter = center + direction * (radius + LABEL_OFFSET);
                Rect labelRect = new(
                    labelCenter.x - LABEL_WIDTH * 0.5f,
                    labelCenter.y - LABEL_HEIGHT * 0.5f,
                    LABEL_WIDTH,
                    LABEL_HEIGHT);
                GUI.Label(
                    labelRect,
                    $"{metrics[i].Label}\n{metrics[i].RawValue:0.##}",
                    EditorStyles.centeredGreyMiniLabel);
            }
        }

        /// <summary>
        ///     レーダーグラフ用の閉じた多角形座標を生成します。
        /// </summary>
        /// <param name="center"> 中心座標です。 </param>
        /// <param name="radius"> 最大半径です。 </param>
        /// <param name="axisCount"> 軸数です。 </param>
        /// <param name="metrics"> 値を反映する指標です。nullの場合は最大半径を使用します。 </param>
        /// <returns> 始点を末尾にも含む座標一覧です。 </returns>
        private static Vector3[] BuildPolygonPoints(
            Vector2 center,
            float radius,
            int axisCount,
            List<Metric> metrics)
        {
            Vector3[] points = new Vector3[axisCount + 1];
            for (int i = 0; i < axisCount; i++)
            {
                float normalizedValue = metrics == null
                    ? 1f
                    : Mathf.Clamp01(metrics[i].NormalizedValue);
                points[i] = center + GetAxisDirection(i, axisCount) * radius * normalizedValue;
            }

            points[axisCount] = points[0];
            return points;
        }

        /// <summary>
        ///     指定軸の中心から外側への方向を取得します。
        /// </summary>
        /// <param name="axisIndex"> 軸インデックスです。 </param>
        /// <param name="axisCount"> 軸数です。 </param>
        /// <returns> 軸方向です。 </returns>
        private static Vector2 GetAxisDirection(int axisIndex, int axisCount)
        {
            float angle = -Mathf.PI * 0.5f + Mathf.PI * 2f * axisIndex / axisCount;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        private const int MINIMUM_METRIC_COUNT = 3;
        private const int GRID_LEVEL_COUNT = 4;
        private const float GRAPH_SIZE = 360f;
        private const float LABEL_OFFSET = 42f;
        private const float LABEL_WIDTH = 80f;
        private const float LABEL_HEIGHT = 36f;
        private const float GRID_LINE_WIDTH = 1.5f;
        private const float VALUE_LINE_WIDTH = 3f;
        private const float HEALTH_PREVIEW_MAX = 500f;
        private const float BASE_DAMAGE_PREVIEW_MAX = 100f;
        private const float ATTACK_SPEED_PREVIEW_MAX = 10f;
        private const float ATTACK_COUNT_PREVIEW_MAX = 8f;
        private const float CONFIRMED_DAMAGE_PREVIEW_MAX = 100f;
        private const float JUST_MULTIPLIER_PREVIEW_MAX = 3f;
        private const string ENEMY_ADDRESSABLE_KEY = "Enemy";
        private const string BOSS_KEYWORD = "Boss";
        private const string MAX_HEALTH_PROPERTY_NAME = "_maxHealth";
        private const string BASE_DAMAGE_PROPERTY_NAME = "_baseDamage";
        private const string ATTACK_INTERVAL_PROPERTY_NAME = "_attackInterval";
        private const string ATTACK_DEFINITIONS_PROPERTY_NAME = "_attackDifinitions";
        private const string ATTACK_SPEC_PROPERTY_NAME = "_attackParameterSetData";
        private const string CONFIRMED_DAMAGE_PROPERTY_NAME = "_confirmedDamage";
        private const string JUST_DAMAGE_MULTIPLIER_PROPERTY_NAME = "_justDamageMultiplier";

        private static readonly Color GRID_COLOR = new(0.45f, 0.48f, 0.52f, 0.8f);
        private static readonly Color VALUE_FILL_COLOR = new(0.2f, 0.65f, 1f, 0.3f);
        private static readonly Color VALUE_LINE_COLOR = new(0.15f, 0.55f, 1f, 1f);

        /// <summary>
        ///     レーダーグラフの1指標です。
        /// </summary>
        private readonly struct Metric
        {
            /// <summary>
            ///     指標を初期化します。
            /// </summary>
            /// <param name="label"> 表示名です。 </param>
            /// <param name="rawValue"> 実値です。 </param>
            /// <param name="normalizedValue"> 0から1を基準とした表示値です。 </param>
            public Metric(string label, float rawValue, float normalizedValue)
            {
                Label = label;
                RawValue = rawValue;
                NormalizedValue = normalizedValue;
            }

            public readonly string Label;
            public readonly float RawValue;
            public readonly float NormalizedValue;
        }
    }
}
