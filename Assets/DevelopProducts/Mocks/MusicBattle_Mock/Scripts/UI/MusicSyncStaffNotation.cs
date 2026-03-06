using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mock.MusicBattle.UI
{
    /// <summary>
    ///     音楽同期システムの五線譜表記UI要素。
    ///     UXMLからインスタンス化できます。
    /// </summary>
    [UxmlElement]
    public partial class MusicSyncStaffNotation : VisualElement
    {
        #region コンストラクタ
        /// <summary>
        ///     <see cref="MusicSyncStaffNotation"/>クラスの新しいインスタンスを初期化します。
        /// </summary>
        public MusicSyncStaffNotation()
        {
            style.position = Position.Absolute;
            style.width = Length.Percent(100);
            style.height = Length.Percent(100);

            // UXMLを読み込んで要素を取得する。
            VisualTreeAsset notationAsset = Resources.Load<VisualTreeAsset>(NOTATION_UXML_RESOURCES_PATH);
            if (notationAsset == null)
            {
                Debug.LogError($"UXMLパス: {NOTATION_UXML_RESOURCES_PATH} の読み込みに失敗しました。");
                return;
            }

            notationAsset.CloneTree(this);

            // 拍子線要素を取得する。
            VisualElement lines = this.Q<VisualElement>(ELEMENT_NAME_STAFF_LINE_CONTAINER);
            _staffLines = lines.Children().ToArray();
            Debug.Assert(_staffLines != null, $"要素: {ELEMENT_NAME_STAFF_LINE_CONTAINER} の検索に失敗しました。");

            // ノーツコンテナ要素を取得する。
            _noteAsset = Resources.Load<VisualTreeAsset>(NOTE_UXML_RESOURCES_PATH);
            _noteContainer = this.Q<VisualElement>(ELEMENT_NAME_NOTE_CONTAINER);
            Debug.Assert(_noteContainer != null, $"要素: {ELEMENT_NAME_NOTE_CONTAINER} の検索に失敗しました。");
            Debug.Assert(_noteAsset != null, $"UXMLパス: {NOTE_UXML_RESOURCES_PATH} の読み込みに失敗しました。");
        }
        #endregion

        #region Publicメソッド
        /// <summary>
        ///     指定された拍と色でノーツを生成し、表示します。
        /// </summary>
        /// <param name="measure">ノーツを表示する拍。</param>
        /// <param name="color">ノーツの色。</param>
        public void CreateNotes(float measure, Color color)
        {
            Debug.Log($"ノートを作成: {measure}拍, 色: {color}");

            // ノーツ要素を生成して配置する。
            VisualElement noteElement = _noteAsset.Instantiate();
            noteElement.style.position = Position.Absolute;
            noteElement.Q<VisualElement>("note").style.backgroundColor = color;
            _noteContainer.Add(noteElement);

            // ノーツエンティティを作成して管理リストに追加する。
            NoteEntity noteEntity = new NoteEntity(measure, noteElement);
            _activeNotes.Add(noteEntity);
        }

        /// <summary>
        ///     五線譜とノーツの表示位置を更新します。
        /// </summary>
        /// <param name="deltaTime">前回のフレームからの経過時間。</param>
        /// <param name="currentMeasure">現在の拍。</param>
        public void Update(float deltaTime, float currentMeasure)
        {
            MoveStaffLines(currentMeasure);
            MoveNotes(deltaTime, currentMeasure);
        }
        #endregion
        #region 定数
        /// <summary> 五線譜UXMLアセットのリソースパス。 </summary>
        private const string NOTATION_UXML_RESOURCES_PATH = "MusicSyncStaffNotation";
        /// <summary> ノーツUXMLアセットのリソースパス。 </summary>
        private const string NOTE_UXML_RESOURCES_PATH = "MusicSyncNote";
        /// <summary> 拍子線コンテナ要素のUXML名。 </summary>
        private const string ELEMENT_NAME_STAFF_LINE_CONTAINER = "staff-line-container";
        /// <summary> ノーツコンテナ要素のUXML名。 </summary>
        private const string ELEMENT_NAME_NOTE_CONTAINER = "note-container";
        /// <summary> 拍子線の移動サイクルの小節数。 </summary>
        private const float STAFF_LINE_MOVE_CYCLE_MEASURES = 4f;
        #endregion
        #region プライベートフィールド
        /// <summary> 五線譜のライン要素配列。 </summary>
        private VisualElement[] _staffLines;
        /// <summary> ノーツを格納するコンテナ要素。 </summary>
        private VisualElement _noteContainer;
        /// <summary> ノーツのVisualTreeAsset。 </summary>
        private VisualTreeAsset _noteAsset;
        /// <summary> アクティブなノーツエンティティのリスト。 </summary>
        private readonly List<NoteEntity> _activeNotes = new();
        #endregion
        #region プライベートメソッド
        /// <summary>
        ///     拍子線の位置を現在の拍に基づいて移動させます。
        /// </summary>
        /// <param name="currentMeasure">現在の拍。</param>
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

        /// <summary>
        ///     ノーツの位置を更新し、一定小節数を超えたノーツを削除します。
        /// </summary>
        /// <param name="deltaTime">前回のフレームからの経過時間。</param>
        /// <param name="currentMeasure">現在の拍。</param>
        private void MoveNotes(float deltaTime, float currentMeasure)
        {
            if (_activeNotes.Count <= 0) { return; }

            for (int i = 0; i < _activeNotes.Count; i++)
            {
                NoteEntity note = _activeNotes[i];

                // ノートの位置を更新する。
                float diff = currentMeasure - note.Measure;
                float x = diff / STAFF_LINE_MOVE_CYCLE_MEASURES * 100;
                // 右端から左端へ移動するので right プロパティを更新する。
                note.Element.style.right = new StyleLength(new Length(x, LengthUnit.Percent));

                // 一定小節数を超えたノートは削除する。
                if (STAFF_LINE_MOVE_CYCLE_MEASURES < diff)
                {
                    _activeNotes.RemoveAt(i);
                    i--;
                }
            }
        }
        #endregion

        #region プライベートStruct定義
        /// <summary>
        ///     五線譜上に表示されるノーツのエンティティ。
        /// </summary>
        private struct NoteEntity : IComparable<NoteEntity>, IDisposable
        {
            /// <summary>
            ///     <see cref="NoteEntity"/>構造体の新しいインスタンスを初期化します。
            /// </summary>
            /// <param name="measure">ノーツが表示される拍。</param>
            /// <param name="element">ノーツのVisualElement。</param>
            public NoteEntity(float measure, VisualElement element)
            {
                _measure = measure;
                _element = element;
            }

            /// <summary> ノーツが表示される拍を取得します。 </summary>
            public float Measure => _measure;
            /// <summary> ノーツのVisualElementを取得します。 </summary>
            public VisualElement Element => _element;

            /// <summary>
            ///     リソースを解放します。
            /// </summary>
            public void Dispose()
            {
                _element.RemoveFromHierarchy();
            }

            /// <summary>
            ///     他の<see cref="NoteEntity"/>と比較します。
            /// </summary>
            /// <param name="other">比較対象の<see cref="NoteEntity"/>。</param>
            /// <returns>比較結果。</returns>
            public int CompareTo(NoteEntity other) => _measure.CompareTo(other._measure);

            /// <summary> ノーツが表示される拍。 </summary>
            private readonly float _measure;
            /// <summary> ノーツのVisualElement。 </summary>
            private readonly VisualElement _element;
        }
        #endregion
    }
}

