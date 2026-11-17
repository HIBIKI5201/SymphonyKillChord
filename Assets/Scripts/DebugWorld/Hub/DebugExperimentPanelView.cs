using KillChord.Runtime.Domain.Persistent.Experiment;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.DebugWorld.Hub
{
    /// <summary>
    ///     裏世界のハブ画面に、ゲージ注視の改善案を案ごとに切り替える欄を組み立てるViewクラスです。
    ///     試遊でプランナーが、同じビルドで案の有無を比べるために使います。
    /// </summary>
    public sealed class DebugExperimentPanelView : IDisposable
    {
        /// <summary>
        ///     ルート要素の右側に、改善案の切り替え欄を組み立てます。
        /// </summary>
        /// <param name="rootElement"> 欄を置くルート要素です。 </param>
        /// <param name="settings"> 今の設定です。 </param>
        public DebugExperimentPanelView(VisualElement rootElement, GaugeGazeExperimentSettings settings)
        {
            if (rootElement == null) { throw new ArgumentNullException(nameof(rootElement)); }

            _settings = settings;
            _panel = CreatePanel();
            _panel.Add(CreateHeader(HEADER_TEXT, HEADER_FONT_SIZE));
            AddToggles(FEATURES);
            _panel.Add(CreateHeader(LOOK_HEADER_TEXT, SUB_HEADER_FONT_SIZE));
            AddToggles(LOOK_FEATURES);

            rootElement.Add(_panel);
        }

        /// <summary> 切り替えた後の設定を通知します。 </summary>
        public event Action<GaugeGazeExperimentSettings> OnSettingsChanged;

        /// <summary>
        ///     組み立てた欄を取り除きます。
        /// </summary>
        public void Dispose()
        {
            for (int i = 0; i < _toggles.Count; i++)
            {
                _toggles[i].Toggle.UnregisterValueChangedCallback(_toggles[i].Callback);
            }

            _toggles.Clear();
            _panel.RemoveFromHierarchy();
            OnSettingsChanged = null;
        }

        private const string HEADER_TEXT = "ゲージ注視の改善案（試遊）";
        private const string LOOK_HEADER_TEXT = "リズムUIの見た目（見比べ用）";
        private const int HEADER_FONT_SIZE = 28;
        private const int SUB_HEADER_FONT_SIZE = 24;
        private const int FIELD_FONT_SIZE = 22;
        private const float PANEL_WIDTH = 520f;
        private const float PANEL_MARGIN = 24f;
        private const float PANEL_PADDING = 16f;
        private const float HEADER_MARGIN_BOTTOM = 12f;
        private static readonly Color PANEL_COLOR = new(1f, 1f, 1f, 0.08f);

        /// <summary> 切り替え欄に並べる案と表示名です。 </summary>
        private static readonly (GaugeGazeExperimentFeature Feature, string Label)[] FEATURES =
        {
            (GaugeGazeExperimentFeature.SkillJustBonus, "スキル入力のJustボーナス"),
            (GaugeGazeExperimentFeature.JustOnlyCombo, "Justのときだけ増えるコンボ"),
            (GaugeGazeExperimentFeature.SkillGaugeEmphasis, "スキルゲージをリズムゲージの上に"),
            (GaugeGazeExperimentFeature.GaugeWaveReduction, "ゲージの波を小さく"),
            (GaugeGazeExperimentFeature.MovementTuning, "移動・回避・硬直の調整"),
            (GaugeGazeExperimentFeature.OneBarGauge, "1小節のゲージとタイムアウト"),
        };

        /// <summary> 見比べ用に並べるリズムUIの見た目の案と表示名です。 </summary>
        private static readonly (GaugeGazeExperimentFeature Feature, string Label)[] LOOK_FEATURES =
        {
            (GaugeGazeExperimentFeature.CenteredGauge, "ゲージを画面の中央に"),
            (GaugeGazeExperimentFeature.InwardGauge, "外側から内側へ伸びるゲージ"),
            (GaugeGazeExperimentFeature.TwentyFourStepGauge, "太いバーで1小節24段階"),
            (GaugeGazeExperimentFeature.JustBarDesign, "Justのバーを特別なデザインに"),
            (GaugeGazeExperimentFeature.LargeRing, "リング型（旧UIを大きく）"),
            (GaugeGazeExperimentFeature.ScreenEdgeRing, "リング型（画面の外周）"),
        };

        private readonly VisualElement _panel;
        private readonly List<(Toggle Toggle, GaugeGazeExperimentFeature Feature, EventCallback<ChangeEvent<bool>> Callback)> _toggles = new();
        private GaugeGazeExperimentSettings _settings;

        /// <summary>
        ///     切り替えられた案を設定に反映し、通知します。
        /// </summary>
        /// <param name="feature"> 切り替えた案です。 </param>
        /// <param name="isEnabled"> 有効にした場合はtrue。 </param>
        private void HandleToggleChanged(GaugeGazeExperimentFeature feature, bool isEnabled)
        {
            _settings = _settings.WithFeature(feature, isEnabled);

            // リングの形のように、ほかの案が一緒に切り替わることがあるため、全ての欄を設定に合わせる。
            for (int i = 0; i < _toggles.Count; i++)
            {
                _toggles[i].Toggle.SetValueWithoutNotify(_settings.IsEnabled(_toggles[i].Feature));
            }

            OnSettingsChanged?.Invoke(_settings);
        }

        /// <summary>
        ///     案ごとの切り替えを欄に並べます。
        /// </summary>
        /// <param name="features"> 並べる案と表示名です。 </param>
        private void AddToggles((GaugeGazeExperimentFeature Feature, string Label)[] features)
        {
            for (int i = 0; i < features.Length; i++)
            {
                (GaugeGazeExperimentFeature feature, string label) = features[i];
                Toggle toggle = new(label) { value = _settings.IsEnabled(feature) };
                toggle.style.fontSize = FIELD_FONT_SIZE;
                toggle.style.color = Color.white;
                EventCallback<ChangeEvent<bool>> callback = evt => HandleToggleChanged(feature, evt.newValue);
                toggle.RegisterValueChangedCallback(callback);
                _toggles.Add((toggle, feature, callback));
                _panel.Add(toggle);
            }
        }

        /// <summary>
        ///     画面の右上に置く欄を作ります。
        /// </summary>
        /// <returns> 作った欄です。 </returns>
        private static VisualElement CreatePanel()
        {
            VisualElement panel = new();
            panel.style.position = Position.Absolute;
            panel.style.right = PANEL_MARGIN;
            panel.style.top = PANEL_MARGIN;
            panel.style.width = PANEL_WIDTH;
            panel.style.paddingLeft = PANEL_PADDING;
            panel.style.paddingRight = PANEL_PADDING;
            panel.style.paddingTop = PANEL_PADDING;
            panel.style.paddingBottom = PANEL_PADDING;
            panel.style.backgroundColor = PANEL_COLOR;
            return panel;
        }

        /// <summary>
        ///     欄の見出しを作ります。
        /// </summary>
        /// <param name="text"> 見出しの文字です。 </param>
        /// <param name="fontSize"> 文字の大きさです。 </param>
        /// <returns> 作った見出しです。 </returns>
        private static Label CreateHeader(string text, int fontSize)
        {
            Label header = new(text);
            header.style.fontSize = fontSize;
            header.style.color = Color.white;
            header.style.marginBottom = HEADER_MARGIN_BOTTOM;
            return header;
        }
    }
}
