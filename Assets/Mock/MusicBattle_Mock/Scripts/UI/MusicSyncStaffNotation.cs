using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mock.MusicBattle.UI
{
    /// <summary>
    ///     音楽同期システムの五線譜表記UI要素。
    /// </summary>
    [UxmlElement]
    public partial class MusicSyncStaffNotation : VisualElement
    {
        public MusicSyncStaffNotation()
        {
            style.position = Position.Absolute;
            style.width = Length.Percent(100);
            style.height = Length.Percent(100);

            // UXMLを読み込んで要素を取得する。
            VisualTreeAsset treeAsset = Resources.Load<VisualTreeAsset>(UXML_RESOURCES_PATH);
            if (treeAsset == null)
            {
                Debug.LogError($"Failed to load UXML at path: {UXML_RESOURCES_PATH}");
                return;
            }

            treeAsset.CloneTree(this);

            VisualElement lines = this.Q<VisualElement>(ELEMENT_NAME_STAFF_LINE_CONTAINER);
            _staffLines = lines.Children().ToArray();
            _noteContainer = this.Q<VisualElement>(ELEMENT_NAME_NOTE_CONTAINER);

            Debug.Assert(_staffLines != null, $"Failed to find element: {ELEMENT_NAME_STAFF_LINE_CONTAINER}");
            Debug.Assert(_noteContainer != null, $"Failed to find element: {ELEMENT_NAME_NOTE_CONTAINER}");
        }

        public void Update(float currentMeasure)
        {
            MoveStaffLines(currentMeasure);
        }

        private const string UXML_RESOURCES_PATH = "MusicSyncStaffNotation";

        private const string ELEMENT_NAME_STAFF_LINE_CONTAINER = "staff-line-container";
        private const string ELEMENT_NAME_NOTE_CONTAINER = "note-container";

        private const float STAFF_LINE_MOVE_CYCLE_MEASURES = 4f;

        private VisualElement[] _staffLines;
        private VisualElement _noteContainer;

        private void MoveStaffLines(float currentMeasure)
        {
            if (_staffLines == null || _staffLines.Length == 0) return;

            // cycleProgress は 0..1 の値（0==開始、1==ちょうどサイクル小節数経過）
            float cycleProgress = Mathf.Repeat(currentMeasure, STAFF_LINE_MOVE_CYCLE_MEASURES) / STAFF_LINE_MOVE_CYCLE_MEASURES;

            // 等間隔（%）。
            float spacing = 100f / _staffLines.Length;

            // 全体の横移動量（%）
            float shift = cycleProgress * 100f;

            for (int i = 0; i < _staffLines.Length; i++)
            {
                // 各ラインのベース位置（0..100 の範囲）
                float basePos = i * spacing;
                float x = Mathf.Repeat(basePos - shift, 100f); // 0の下回りを防ぐために繰り返す。

                _staffLines[i].style.left = new Length(x, LengthUnit.Percent);
            }
        }
    }
}
