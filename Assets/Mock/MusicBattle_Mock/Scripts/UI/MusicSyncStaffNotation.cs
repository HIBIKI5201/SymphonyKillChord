using Mock.MusicBattle.MusicSync;
using System;
using System.Collections.Generic;
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
            VisualTreeAsset notationAsset = Resources.Load<VisualTreeAsset>(NOTATION_UXML_RESOURCES_PATH);
            if (notationAsset == null)
            {
                Debug.LogError($"Failed to load UXML at path: {NOTATION_UXML_RESOURCES_PATH}");
                return;
            }

            notationAsset.CloneTree(this);

            VisualElement lines = this.Q<VisualElement>(ELEMENT_NAME_STAFF_LINE_CONTAINER);
            _staffLines = lines.Children().ToArray();
            _noteContainer = this.Q<VisualElement>(ELEMENT_NAME_NOTE_CONTAINER);

            Debug.Assert(_staffLines != null, $"Failed to find element: {ELEMENT_NAME_STAFF_LINE_CONTAINER}");
            Debug.Assert(_noteContainer != null, $"Failed to find element: {ELEMENT_NAME_NOTE_CONTAINER}");

            _noteAsset = Resources.Load<VisualTreeAsset>(NOTE_UXML_RESOURCES_PATH);
            Debug.Assert(_noteAsset != null, $"Failed to load UXML at path: {NOTE_UXML_RESOURCES_PATH}");
        }

        /// <summary>
        ///     ノーツを生成する。
        /// </summary>
        /// <param name="measure"></param>
        public void CreateNotes(float measure)
        {
            NoteEntity noteEntity = new NoteEntity(measure, _noteAsset.Instantiate());
            _activeNotes.Add(noteEntity);
        }

        public void Update(float deltaTime, float currentMeasure)
        {
            MoveStaffLines(currentMeasure);
            MoveNotes(deltaTime, currentMeasure);
        }

        private const string NOTATION_UXML_RESOURCES_PATH = "MusicSyncStaffNotation";
        private const string NOTE_UXML_RESOURCES_PATH = "MusicSyncNote";

        private const string ELEMENT_NAME_STAFF_LINE_CONTAINER = "staff-line-container";
        private const string ELEMENT_NAME_NOTE_CONTAINER = "note-container";

        private const float STAFF_LINE_MOVE_CYCLE_MEASURES = 4f;

        private VisualElement[] _staffLines;
        private VisualElement _noteContainer;

        private VisualTreeAsset _noteAsset;
        private List<NoteEntity> _activeNotes = new();

        private struct NoteEntity : IComparable<NoteEntity>, IDisposable
        {
            public NoteEntity(float measure, VisualElement element)
            {
                _measure = measure;
                _element = element;
            }

            public float Measure => _measure;
            public VisualElement Element => _element;

            public void Dispose()
            {
                _element.RemoveFromHierarchy();
            }

            public int CompareTo(NoteEntity other) => _measure.CompareTo(other._measure);

            private readonly float _measure;
            private readonly VisualElement _element;
        }

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

        private void MoveNotes(float deltaTime, float currentMeasure)
        {
            if (_activeNotes.Count <= 0) { return; }

            for (int i = 0; i < _activeNotes.Count; i++)
            {
                NoteEntity note = _activeNotes[i];

                // ノートの位置を更新する。
                float diff = currentMeasure - note.Measure;
                float x = diff / STAFF_LINE_MOVE_CYCLE_MEASURES * 100;
                note.Element.style.left = new StyleLength(new Length(x, LengthUnit.Percent));

                // 一定小節数を超えたノートは削除する。
                if (STAFF_LINE_MOVE_CYCLE_MEASURES < diff)
                {
                    _activeNotes.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
