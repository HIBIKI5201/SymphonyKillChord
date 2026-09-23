using System;
using UnityEngine;
using UnityEngine.UIElements;
using PointerType = UnityEngine.UIElements.PointerType;

namespace KillChord.Runtime.View.OutGame.SkillBuild
{
    /// <summary>
    ///   スキルアイコン UI (スキルビルド画面のスキル要素) のドラッグ&ドロップ操作を管理するクラス。
    /// </summary>
    public class SkillElementDragAndDropManipulator : PointerManipulator
    {
        /// <summary>
        ///     ドラッグ&ドロップ操作を管理するマニピュレータのコンストラクタ。
        /// </summary>
        /// <param name="target"> 操作対象の VisualElement。 </param>
        /// <param name="onDropAction"> ドロップ時に実行されるアクション。 </param>
        /// <param name="slotContainerName"> スロットコンテナの名前。 </param>
        /// <param name="slotName"> スロットの名前。 </param>
        public SkillElementDragAndDropManipulator(
            VisualElement target,
            Action<VisualElement, VisualElement> onDropAction = null,
            string slotContainerName = "skill-element-container",
            string slotName = "skill-element-slot")
        {
            this.target = target;
            _onDropAction = onDropAction;
            _slotContainerName = slotContainerName;
            _slotName = slotName;

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }
        }

        /// <summary>
        ///     ドラッグ&ドロップ操作に必要なポインタイベントのコールバックを
        ///     ターゲットの VisualElement に登録するメソッド。
        /// </summary>
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);

            if (target.panel != null)
            {
                FetchSkillBuildScreen();
            }

            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
            target.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
        }

        /// <summary>
        ///    ドラッグ&ドロップ操作に必要なポインタイベントのコールバックを
        ///    ターゲットの VisualElement から解除するメソッド。
        /// </summary>
        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<AttachToPanelEvent>(OnAttachToPanel);

            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            target.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
        }

        private readonly string _slotContainerName;
        private readonly string _slotName;
        private readonly Action<VisualElement, VisualElement> _onDropAction;

        private const string DRAG_COMPLETED_CLASS_NAME = "drag-just-completed";
        private const string SKILL_BUILD_ROOT_NAME = "SkillBuildRoot";
        private const float DRAG_START_THRESHOLD = 10f;

        private bool _isPointerDown;
        private bool _isDragging;
        private bool _isReparentingForDrag;
        private int _activePointerId = -1;

        // ドラッグ開始時のパネル・ワールド座標を保持するフィールド。
        private Vector2 _pointerStartPanel;

        // ドラッグ開始時のスキル要素のワールド座標を保持するフィールド。
        private Vector2 _elementStartWorld;

        // ドラッグ開始時のスキル要素の親要素を保持するフィールド。
        private VisualElement _startParent;

        // ドラッグ開始時のスキル要素の並び順(インデックス)を保持するフィールド。
        // スナップバック時に一覧の末尾へ追加されてしまわないよう、元の位置へ挿入し直すために使う。
        private int _startIndex;

        // ドラッグ中にスキル要素を一時的に追加するスキルビルド画面のルート要素を保持するフィールド。
        private VisualElement _skillBuildScreen;

        /// <summary>
        ///     ドラッグ開始時のポインタダウンイベントを処理するメソッド。
        ///     スキル要素を絶対位置に設定し、ドラッグ開始座標と親要素を保存する。
        /// </summary>
        /// <param name="evt"></param>
        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.pointerType == PointerType.mouse && evt.button != 0)
            { return; }


            _isPointerDown = true;
            _isDragging = false;
            _activePointerId = evt.pointerId;
            _pointerStartPanel = (Vector2)evt.position;
            _elementStartWorld = target.worldBound.position;
            _startParent = target.parent;
            _startIndex = GetChildIndex(_startParent, target);
            target.CapturePointer(evt.pointerId);

            // 親の ScrollView にポインター操作を渡すと、ドラッグ開始前に
            // スクロール操作として扱われるため、スキル要素側で処理を完結させる。
            evt.StopPropagation();
        }

        /// <summary>
        ///     ドラッグ中のポインタ移動イベントを処理するメソッド。
        ///     スキル要素をポインタの移動に合わせて移動させる。
        /// </summary>
        /// <param name="evt"></param>
        private void OnPointerMove(PointerMoveEvent evt)
        {
            // ドラッグ開始前のポインタ移動イベントは無視する。
            if (!_isPointerDown ||
                evt.pointerId != _activePointerId)
            { return; }

            // ドラッグ中にポインターキャプチャを保持していない場合は、ドラッグ操作を中断する。
            if (_isDragging && !target.HasPointerCapture(evt.pointerId))
            { return; }

            Vector2 pointerCurrent = (Vector2)evt.position;
            Vector2 pointerDelta = pointerCurrent - _pointerStartPanel;
            if (!_isDragging)
            {
                if (pointerDelta.sqrMagnitude < DRAG_START_THRESHOLD * DRAG_START_THRESHOLD)
                {
                    evt.StopPropagation();
                    return;
                }

                StartDragging(evt.pointerId);
            }

            VisualElement parent = target.parent;
            if (parent == null) { return; }

            Vector2 newWorld = _elementStartWorld + pointerDelta;

            Vector2 newLocal = parent.WorldToLocal(newWorld);

            target.style.left = newLocal.x;
            target.style.top = newLocal.y;

            evt.StopPropagation();
        }

        /// <summary>
        ///    ドラッグ終了時のポインタアップイベントを処理するメソッド。
        /// </summary>
        /// <param name="evt"></param>
        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!_isPointerDown ||
                evt.pointerId != _activePointerId)
            { return; }

            if (!_isDragging)
            {
                _isPointerDown = false;
                _activePointerId = -1;

                if (target.HasPointerCapture(evt.pointerId))
                {
                    target.ReleasePointer(evt.pointerId);
                }

                return;
            }

            _isPointerDown = false;
            _activePointerId = -1;

            if (target.HasPointerCapture(evt.pointerId))
            {
                target.ReleasePointer(evt.pointerId);
            }

            MarkDragCompleted();
            CompleteDrag();
            evt.StopPropagation();
        }

        /// <summary>
        ///     ドラッグ終了時のポインタキャプチャアウトイベントを処理するメソッド。
        /// </summary>
        /// <param name="evt"></param>
        private void OnPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            if (!_isPointerDown ||
                evt.pointerId != _activePointerId)
            { return; }

            // ドラッグ用ルートへの親変更で発生したキャプチャ解除は操作終了として扱わない。
            // 再取得後に遅れて通知された場合も、現在キャプチャ中なら同様に無視する。
            if (_isReparentingForDrag ||
                target.HasPointerCapture(evt.pointerId))
            { return; }

            bool wasDragging = _isDragging;
            _isPointerDown = false;
            _activePointerId = -1;

            if (!wasDragging)
            {
                return;
            }

            // OS や別要素にキャプチャを奪われた場合にも、最後に確認できた位置で
            // ドロップを完了し、画面上に絶対配置の要素を残さない。
            MarkDragCompleted();
            CompleteDrag();
        }

        /// <summary>
        ///     ドラッグ直後のクリックを選択操作として扱わないための印を付ける。
        /// </summary>
        private void MarkDragCompleted()
        {
            target.AddToClassList(DRAG_COMPLETED_CLASS_NAME);
            target.schedule.Execute(() =>
                target.RemoveFromClassList(DRAG_COMPLETED_CLASS_NAME));
        }

        /// <summary>
        ///     ドロップ位置を判定して装備状態の更新を通知し、
        ///     スキル要素は結果によらず常に一覧上の元の位置へ戻す。
        /// </summary>
        private void CompleteDrag()
        {
            if (!_isDragging) { return; }

            bool droppedOnSlot = TryFindClosestSlot(requireOverlap: true, out VisualElement slotElement);

            // カード要素は装備の成否によらず、常に一覧の元位置へ戻す。
            // 装備状態はバッジ表示で示すため、スロットへの再親化は行わない。
            SnapBackToStart();

            _onDropAction?.Invoke(target, droppedOnSlot ? slotElement : null);

            _isDragging = false;
        }

        /// <summary>
        ///    ターゲットの VisualElement がパネルにアタッチされたときのイベントを処理するメソッド。
        /// </summary>
        /// <param name="evt"></param>
        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            FetchSkillBuildScreen();
        }


        /// <summary>
        ///     ターゲットの VisualElement が属するパネルのビジュアルツリーから、
        ///     改造画面のルート要素を検索して保存するメソッド。
        /// </summary>
        private void FetchSkillBuildScreen()
        {
            var root = target.panel?.visualTree;
            var skillBuildContainer = root.Q<TemplateContainer>("SkillBuildContainer");
            _skillBuildScreen = skillBuildContainer?.Q<VisualElement>(SKILL_BUILD_ROOT_NAME);

            if (_skillBuildScreen == null)
            {
                Debug.LogError($"SkillElementDragAndDropManipulator: スキルビルド画面のルート要素 '{SKILL_BUILD_ROOT_NAME}' が見つかりません。ドラッグ&ドロップ操作が正しく機能しない可能性があります。");
            }
        }

        /// <summary>
        ///     移動閾値を超えた時にドラッグ状態へ移行する。
        /// </summary>
        private void StartDragging(int pointerId)
        {
            _isDragging = true;
            target.style.position = Position.Absolute;

            // キャプチャ中の要素を別の親へ移すと PointerCaptureOutEvent が発生するため、
            // 親変更による通知を無視し、移動後にキャプチャを再取得する。
            if (_skillBuildScreen != null)
            {
                _isReparentingForDrag = true;
                try
                {
                    _skillBuildScreen.Add(target);
                    Vector2 localPosition = _skillBuildScreen.WorldToLocal(_elementStartWorld);
                    target.style.left = localPosition.x;
                    target.style.top = localPosition.y;

                    if (!target.HasPointerCapture(pointerId))
                    {
                        target.CapturePointer(pointerId);
                    }
                }
                finally
                {
                    _isReparentingForDrag = false;
                }
            }

            target.BringToFront();

            if (!target.HasPointerCapture(pointerId))
            {
                target.CapturePointer(pointerId);
            }
        }

        /// <summary>
        ///     ドロップ可能なスロットを検索するメソッド。
        ///     requireOverlap が true の場合、スキル要素と重なっているスロットのみを対象とする。
        /// </summary>
        /// <param name="requireOverlap"></param>
        /// <returns></returns>
        private bool TryFindClosestSlot(bool requireOverlap, out VisualElement element)
        {
            element = null;
            // ドロップ対象のスロットを検索するために、
            // スキル要素が属するパネルのビジュアルツリーを取得する。
            if (target.panel == null || _skillBuildScreen == null) { return false; }

            // スロットコンテナを検索する。
            // スロットコンテナが指定されていない場合は、ルートをスロットの検索対象とする。
            VisualElement slotsRoot = string.IsNullOrEmpty(_slotContainerName)
                ? _skillBuildScreen
                : _skillBuildScreen.Query<VisualElement>(className: _slotContainerName);

            // スロットコンテナが見つからない場合は、ドロップ可能なスロットが存在しないとみなす。
            if (slotsRoot == null) { return false; }

            // スロットコンテナ内のスロットをすべて検索する。
            var slots = slotsRoot.Query<VisualElement>(className: _slotName).ToList();
            // スロットが見つからない場合は、ドロップ可能なスロットが存在しないとみなす。
            if (slots.Count == 0) { return false; }

            // スキル要素と最も近いスロットを検索する。
            VisualElement closest = null;
            float minDistance = float.MaxValue;

            foreach (var slot in slots)
            {
                // requireOverlap が true の場合、スキル要素と重なっていないスロットは候補から除外する。
                if (requireOverlap && !target.worldBound.Overlaps(slot.worldBound))
                { continue; }

                // スキル要素とスロットの中心点間の距離を計算する。
                float distance = Vector2.Distance(slot.worldBound.center, target.worldBound.center);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = slot;
                }
            }

            element = closest;
            return closest != null;
        }

        /// <summary>
        ///    ドロップ結果によらず、スキル要素をドラッグ開始位置(一覧上の元位置)へ
        ///    スナップバックさせるメソッド。
        /// </summary>
        private void SnapBackToStart()
        {
            if (target.parent == null) { return; }

            // Add() は末尾に追加されてしまい元の並び順が崩れるため、
            // ドラッグ開始時のインデックスへ挿入し直して並び順を保つ。
            int insertIndex = Mathf.Clamp(_startIndex, 0, _startParent.childCount);
            _startParent.Insert(insertIndex, target);

            // スタイルをリセットする。
            target.style.position = Position.Relative;

            // ドラッグ開始位置に戻すために、スタイルの left と top を null に設定する。
            target.style.left = StyleKeyword.Null;
            target.style.top = StyleKeyword.Null;
        }

        /// <summary>
        ///     指定した親要素における子要素のインデックスを取得するメソッド。
        /// </summary>
        /// <param name="parent"> 検索対象の親要素。 </param>
        /// <param name="child"> 検索するインデックス。 </param>
        /// <returns> 子要素のインデックス。見つからない場合は -1。 </returns>
        private static int GetChildIndex(VisualElement parent, VisualElement child)
        {
            if (parent == null) { return -1; }

            int index = 0;
            foreach (VisualElement current in parent.Children())
            {
                if (ReferenceEquals(current, child))
                {
                    return index;
                }

                index++;
            }

            return -1;
        }
    }
}
