using System;
using System.Collections.Generic;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.OutGame.StageSelect;
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
            DrawDefaultInspector();

            // 入力が不正な場合は範囲を作れないため、理由だけを表示する。
            RhythmJudgmentDefinitionFactory factory;
            try
            {
                factory = ((RhythmJudgmentDefinitionAsset)target).ToFactory();
            }
            catch (Exception exception)
            {
                EditorGUILayout.HelpBox($"判定範囲を作れません: {exception.Message}", MessageType.Warning);
                return;
            }

            EditorGUILayout.Space(SECTION_SPACING);
            EditorGUILayout.LabelField("判定ゾーン / ジャスト範囲（1.2小節まで表示）", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("解放条件のある拍子が未解放のとき", EditorStyles.miniBoldLabel);
            DrawGauge(factory.Create(factory.ResolveAvailableBeatTypes(Array.Empty<StageId>(), null)));

            EditorGUILayout.Space(SECTION_SPACING);
            EditorGUILayout.LabelField("全ての拍子を解放したとき", EditorStyles.miniBoldLabel);
            DrawGauge(factory.Create(factory.AllBeatTypes));
        }

        /// <summary>
        ///     判定ゾーンとJust位置のゲージを描画する。
        /// </summary>
        /// <param name="definition"> 描画するリズム判定の定義。 </param>
        private static void DrawGauge(RhythmJudgmentDefinition definition)
        {
            IReadOnlyList<RhythmJudgmentRange> ranges = definition.JudgmentRanges;
            if (ranges.Count == 0)
            {
                EditorGUILayout.HelpBox("判定ゾーンが設定されていません。", MessageType.Info);
                return;
            }

            // ゲージ本体とラベル表示の領域を確保し、背景と目盛りを描く。
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

            // 判定ゾーンごとに、範囲・ジャスト範囲・ラベルを描く。
            for (int i = 0; i < ranges.Count; i++)
            {
                RhythmJudgmentRange range = ranges[i];
                int beatType = (int)range.BeatType;

                Rect zoneRect = new Rect(
                    gaugeRect.x + gaugeRect.width * Mathf.Clamp01(range.StartNormalized / GAUGE_LENGTH_IN_BARS),
                    gaugeRect.y,
                    gaugeRect.width * Mathf.Max(0f, Mathf.Clamp01(range.EndNormalized / GAUGE_LENGTH_IN_BARS) - Mathf.Clamp01(range.StartNormalized / GAUGE_LENGTH_IN_BARS)),
                    gaugeRect.height);
                EditorGUI.DrawRect(zoneRect, GetZoneColor(beatType));
                DrawJustMarker(gaugeRect, range.JustStartNormalized, range.JustEndNormalized);

                // 狭いゾーンのラベルが重ならないよう、偶数番目と奇数番目で段を分ける。
                float labelHeight = LABEL_AREA_HEIGHT * 0.5f;
                Rect labelRect = new Rect(
                    zoneRect.x,
                    labelAreaRect.y + (i % 2) * labelHeight,
                    Mathf.Max(zoneRect.width, MIN_LABEL_WIDTH),
                    labelHeight);
                GUI.Label(
                    labelRect,
                    $"n={beatType} [{range.StartNormalized:0.###}, {range.EndNormalized:0.###}]",
                    EditorStyles.miniLabel);
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
        private static void DrawJustMarker(Rect gaugeRect, float justStart, float justEnd)
        {
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

        private const float GAUGE_LENGTH_IN_BARS = 1.2f;
        private const float SECTION_SPACING = 12f;
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
