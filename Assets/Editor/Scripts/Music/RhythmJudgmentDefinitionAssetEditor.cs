using System.Collections.Generic;
using KillChord.Runtime.InfraStructure.InGame.Music;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.Music
{
    /// <summary>
    ///     RhythmJudgmentDefinitionAssetの判定ゾーンとJust位置をゲージ形式で可視化するカスタムエディタ。
    /// </summary>
    [CustomEditor(typeof(RhythmJudgmentDefinitionAsset))]
    public sealed class RhythmJudgmentDefinitionAssetEditor : UnityEditor.Editor
    {
        /// <summary>
        ///     インスペクターGUIを描画する。
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty rangeData = serializedObject.FindProperty(RANGE_DATA_PROPERTY_NAME);
            EditorGUILayout.PropertyField(rangeData, true);

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(SECTION_SPACING);
            EditorGUILayout.LabelField("判定ゾーン / ジャスト範囲（1.2小節まで表示）", EditorStyles.boldLabel);

            DrawGauge(rangeData);
        }

        /// <summary>
        ///     判定ゾーンとJust位置のゲージを描画する。
        /// </summary>
        /// <param name="rangeData"> 判定範囲データの配列プロパティ。 </param>
        private void DrawGauge(SerializedProperty rangeData)
        {
            if (rangeData == null || !rangeData.isArray || rangeData.arraySize == 0)
            {
                EditorGUILayout.HelpBox("判定ゾーンが設定されていません。", MessageType.Info);
                return;
            }

            int zoneCount = rangeData.arraySize;

            Rect fullRect = GUILayoutUtility.GetRect(
                1f,
                GAUGE_HEIGHT + LABEL_AREA_HEIGHT,
                GUILayout.ExpandWidth(true));
            Rect gaugeRect = new Rect(fullRect.x, fullRect.y, fullRect.width, GAUGE_HEIGHT);
            Rect labelAreaRect = new Rect(
                fullRect.x,
                fullRect.y + GAUGE_HEIGHT,
                fullRect.width,
                LABEL_AREA_HEIGHT);

            EditorGUI.DrawRect(gaugeRect, BACKGROUND_COLOR);
            DrawScaleTicks(gaugeRect);

            List<string> warnings = new();

            for (int i = 0; i < zoneCount; i++)
            {
                SerializedProperty element = rangeData.GetArrayElementAtIndex(i);
                int beatType = element.FindPropertyRelative(BEAT_TYPE_PROPERTY_NAME).intValue;
                float start = element.FindPropertyRelative(START_NORMALIZED_PROPERTY_NAME).floatValue;
                float end = element.FindPropertyRelative(END_NORMALIZED_PROPERTY_NAME).floatValue;
                float justStart = element.FindPropertyRelative(JUST_START_PROPERTY_NAME).floatValue;
                float justEnd = element.FindPropertyRelative(JUST_END_PROPERTY_NAME).floatValue;

                Rect zoneRect = new Rect(
                    gaugeRect.x + gaugeRect.width * Mathf.Clamp01(start / GAUGE_LENGTH_IN_BARS),
                    gaugeRect.y,
                    gaugeRect.width * Mathf.Max(0f, Mathf.Clamp01(end / GAUGE_LENGTH_IN_BARS) - Mathf.Clamp01(start / GAUGE_LENGTH_IN_BARS)),
                    gaugeRect.height);
                EditorGUI.DrawRect(zoneRect, GetZoneColor(beatType));

                bool isJustRangeValid = justStart >= 0f && justEnd > justStart
                    && !float.IsInfinity(justStart) && !float.IsInfinity(justEnd);
                DrawJustMarker(gaugeRect, justStart, justEnd, isJustRangeValid);

                Rect labelRect = new Rect(
                    zoneRect.x,
                    labelAreaRect.y,
                    Mathf.Max(zoneRect.width, MIN_LABEL_WIDTH),
                    LABEL_AREA_HEIGHT);
                GUI.Label(
                    labelRect,
                    $"n={beatType}\n[{start:0.###}, {end:0.###}]\nJust=[{justStart:0.###}, {justEnd:0.###})",
                    EditorStyles.miniLabel);

                if (!isJustRangeValid)
                {
                    warnings.Add(
                        $"BeatType {beatType}: ジャスト開始・終了には有限値を指定し、0 ≦ 開始 < 終了としてください。");
                }
            }

            EditorGUILayout.Space(WARNING_SPACING);
            for (int i = 0; i < warnings.Count; i++)
            {
                EditorGUILayout.HelpBox(warnings[i], MessageType.Warning);
            }
        }

        /// <summary>
        ///     ゲージ上に0.25刻みの目盛り線を描画する。
        /// </summary>
        /// <param name="gaugeRect"> ゲージの描画領域。 </param>
        private static void DrawScaleTicks(Rect gaugeRect)
        {
            for (int i = 0; i <= SCALE_TICK_COUNT; i++)
            {
                float normalized = (float)i / SCALE_TICK_COUNT / GAUGE_LENGTH_IN_BARS;
                float x = gaugeRect.x + gaugeRect.width * normalized;
                Rect tickRect = new Rect(x - TICK_WIDTH * 0.5f, gaugeRect.y, TICK_WIDTH, gaugeRect.height);
                EditorGUI.DrawRect(tickRect, SCALE_TICK_COLOR);
            }
        }

        /// <summary>
        ///     実際のジャスト範囲を示す帯を描画する。
        /// </summary>
        /// <param name="gaugeRect"> ゲージの描画領域。 </param>
        /// <param name="justStart"> ジャスト開始位置。 </param>
        /// <param name="justEnd"> ジャスト終了位置。 </param>
        /// <param name="isValid"> 有効なジャスト範囲か。 </param>
        private static void DrawJustMarker(Rect gaugeRect, float justStart, float justEnd, bool isValid)
        {
            if (!isValid)
            {
                return;
            }

            float start = Mathf.Clamp01(justStart / GAUGE_LENGTH_IN_BARS);
            float end = Mathf.Clamp01(justEnd / GAUGE_LENGTH_IN_BARS);
            Rect markerRect = new Rect(
                gaugeRect.x + gaugeRect.width * start,
                gaugeRect.y - MARKER_OVERHANG,
                gaugeRect.width * (end - start),
                gaugeRect.height + MARKER_OVERHANG * 2f);
            EditorGUI.DrawRect(markerRect, JUST_MARKER_COLOR);
        }

        /// <summary>
        ///     拍種に応じたゾーン色を取得する。
        /// </summary>
        /// <param name="beatType"> 拍種の数値。 </param>
        /// <returns> ゾーンに使用する色。 </returns>
        private static Color GetZoneColor(int beatType)
        {
            float hue = (beatType * GOLDEN_RATIO_CONJUGATE) % 1f;
            return Color.HSVToRGB(hue, ZONE_COLOR_SATURATION, ZONE_COLOR_VALUE);
        }

        private const string RANGE_DATA_PROPERTY_NAME = "_rangeData";
        private const string BEAT_TYPE_PROPERTY_NAME = "BeatType";
        private const string START_NORMALIZED_PROPERTY_NAME = "StartNormalized";
        private const string END_NORMALIZED_PROPERTY_NAME = "EndNormalized";
        private const string JUST_START_PROPERTY_NAME = "JustStartNormalized";
        private const string JUST_END_PROPERTY_NAME = "JustEndNormalized";

        private const float GAUGE_LENGTH_IN_BARS = 1.2f;
        private const float SECTION_SPACING = 12f;
        private const float WARNING_SPACING = 4f;
        private const float GAUGE_HEIGHT = 32f;
        private const float LABEL_AREA_HEIGHT = 40f;
        private const float MIN_LABEL_WIDTH = 70f;
        private const float MARKER_OVERHANG = 4f;
        private const float TICK_WIDTH = 1f;
        private const int SCALE_TICK_COUNT = 4;
        private const float GOLDEN_RATIO_CONJUGATE = 0.618034f;
        private const float ZONE_COLOR_SATURATION = 0.55f;
        private const float ZONE_COLOR_VALUE = 0.85f;

        private static readonly Color BACKGROUND_COLOR = new(0.15f, 0.15f, 0.15f, 1f);
        private static readonly Color SCALE_TICK_COLOR = new(1f, 1f, 1f, 0.25f);
        private static readonly Color JUST_MARKER_COLOR = new(1f, 1f, 1f, 0.95f);
    }
}
