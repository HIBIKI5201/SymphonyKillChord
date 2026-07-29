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
            EditorGUILayout.LabelField("判定ゾーン ビジュアライズ（1小節基準）", EditorStyles.boldLabel);

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

                Rect zoneRect = new Rect(
                    gaugeRect.x + gaugeRect.width * Mathf.Clamp01(start),
                    gaugeRect.y,
                    gaugeRect.width * Mathf.Max(0f, Mathf.Clamp01(end) - Mathf.Clamp01(start)),
                    gaugeRect.height);
                EditorGUI.DrawRect(zoneRect, GetZoneColor(beatType));

                float justNormalized = beatType > 0 ? 1f / beatType : 0f;
                bool isJustWithinZone = justNormalized >= start && justNormalized <= end;
                DrawJustMarker(gaugeRect, justNormalized, isJustWithinZone);

                Rect labelRect = new Rect(
                    zoneRect.x,
                    labelAreaRect.y,
                    Mathf.Max(zoneRect.width, MIN_LABEL_WIDTH),
                    LABEL_AREA_HEIGHT);
                GUI.Label(
                    labelRect,
                    $"n={beatType}\n[{start:0.###}, {end:0.###}]\nJust={justNormalized:0.###}",
                    EditorStyles.miniLabel);

                if (!isJustWithinZone)
                {
                    warnings.Add(
                        $"BeatType {beatType}: Just位置({justNormalized:0.###})が自身の判定ウィンドウ" +
                        $"[{start:0.###}, {end:0.###}]の外側にあります。");
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
                float normalized = (float)i / SCALE_TICK_COUNT;
                float x = gaugeRect.x + gaugeRect.width * normalized;
                Rect tickRect = new Rect(x - TICK_WIDTH * 0.5f, gaugeRect.y, TICK_WIDTH, gaugeRect.height);
                EditorGUI.DrawRect(tickRect, SCALE_TICK_COLOR);
            }
        }

        /// <summary>
        ///     Just位置を示すマーカー線を描画する。
        /// </summary>
        /// <param name="gaugeRect"> ゲージの描画領域。 </param>
        /// <param name="justNormalized"> Justの正規化位置(0～1)。 </param>
        /// <param name="isWithinZone"> 自身の判定ゾーン内にJust位置が収まっているか。 </param>
        private static void DrawJustMarker(Rect gaugeRect, float justNormalized, bool isWithinZone)
        {
            float x = gaugeRect.x + gaugeRect.width * Mathf.Clamp01(justNormalized);
            Rect markerRect = new Rect(
                x - MARKER_WIDTH * 0.5f,
                gaugeRect.y - MARKER_OVERHANG,
                MARKER_WIDTH,
                gaugeRect.height + MARKER_OVERHANG * 2f);
            EditorGUI.DrawRect(markerRect, isWithinZone ? JUST_MARKER_COLOR : JUST_MARKER_WARNING_COLOR);
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

        private const float SECTION_SPACING = 12f;
        private const float WARNING_SPACING = 4f;
        private const float GAUGE_HEIGHT = 32f;
        private const float LABEL_AREA_HEIGHT = 40f;
        private const float MIN_LABEL_WIDTH = 70f;
        private const float MARKER_WIDTH = 2f;
        private const float MARKER_OVERHANG = 4f;
        private const float TICK_WIDTH = 1f;
        private const int SCALE_TICK_COUNT = 4;
        private const float GOLDEN_RATIO_CONJUGATE = 0.618034f;
        private const float ZONE_COLOR_SATURATION = 0.55f;
        private const float ZONE_COLOR_VALUE = 0.85f;

        private static readonly Color BACKGROUND_COLOR = new(0.15f, 0.15f, 0.15f, 1f);
        private static readonly Color SCALE_TICK_COLOR = new(1f, 1f, 1f, 0.25f);
        private static readonly Color JUST_MARKER_COLOR = new(1f, 1f, 1f, 0.95f);
        private static readonly Color JUST_MARKER_WARNING_COLOR = new(1f, 0.15f, 0.15f, 0.95f);
    }
}
