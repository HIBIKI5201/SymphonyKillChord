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

        private const string DRAGGABLE_CLASS_NAME = "draggable";
        private const string DRAG_COMPLETED_CLASS_NAME = "drag-just-completed";
        private const string SKILL_ELEMENT_LIST_CLASS_NAME = "skill-element-list";
        private const string SKILL_BUILD_ROOT_NAME = "SkillBuildRoot";
        private const float DRAG_START_THRESHOLD = 10f;

        private bool _isPointerDown;
        private bool _isDragging;
        private int _activePointerId = -1;

        // ドラッグ開始時のパネル・ワールド座標を保持するフィールド。
        private Vector2 _pointerStartPanel;

        // ドラッグ開始時のスキル要素のワールド座標を保持するフィールド。
        private Vector2 _elementStartWorld;

        // ドラッグ開始時のスキル要素の親要素を保持するフィールド。
        private VisualElement _startParent;

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
                return;
            }

            if (target.HasPointerCapture(evt.pointerId))
            {
                // ドラッグ中にポインターキャプチャを保持している場合、ドロップ処理を行う。
                target.ReleasePointer(evt.pointerId);
            }

            evt.StopPropagation();
        }

        /// <summary>
        ///     ドラッグ終了時のポインタキャプチャアウトイベントを処理するメソッド。
        /// </summary>
        /// <param name="evt"></param>
        private void OnPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            if (!_isPointerDown) { return; }

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
        ///     現在位置に応じてドロップ、リストへの移動、または開始位置への復帰を行う。
        /// </summary>
        private void CompleteDrag()
        {
            if (!_isDragging) { return; }

            VisualElement targetElement;

            if (TryFindClosestSlot(requireOverlap: true, out targetElement))
            {
                // スロットに既存のスキル要素がある場合は、元の親要素へ移動させてスワップする。
                VisualElement existingSkill = FindSkillInSlot(targetElement);
                if (existingSkill != null)
                {
                    SwapSkillToParent(existingSkill, _startParent);
                }

                // ドラッグしたスキル要素をスロットへ移動する。
                MoveSkillToSlot(target, targetElement);
                _onDropAction?.Invoke(target, targetElement);
            }
            else if (TryFindSkillList(requireOverlap: true, out targetElement))
            {
                // ドロップ先が入手済みスキルリストの場合は、スキル要素を入手済みスキルリストへ移動する。
                MoveSkillToList(target, targetElement);
                _onDropAction?.Invoke(target, targetElement);
            }
            else
            {
                // ドロップ先スロットが見つからない場合は、ドラッグ開始位置に戻す。
                SnapBackToStart();
                _onDropAction?.Invoke(target, null);
            }

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

            // キャプチャ中の要素を別の親へ移すと PointerCaptureOutEvent が発生する。
            // 先にドラッグ用ルートへ移し、その後でポインターをキャプチャする。
            if (_skillBuildScreen != null)
            {
                _skillBuildScreen.Add(target);
                Vector2 localPosition = _skillBuildScreen.WorldToLocal(_elementStartWorld);
                target.style.left = localPosition.x;
                target.style.top = localPosition.y;
            }

            target.BringToFront();
            target.CapturePointer(pointerId);
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
        ///     ドロップ可能な入手済みスキルリストを検索するメソッド。
        /// </summary>
        /// <param name="requireOverlap"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        private bool TryFindSkillList(bool requireOverlap, out VisualElement element)
        {
            element = null;
            if (target.panel == null || _skillBuildScreen == null) { return false; }

            VisualElement skillListRoot =
                _skillBuildScreen.Query<VisualElement>(className: SKILL_ELEMENT_LIST_CLASS_NAME);

            if (skillListRoot == null) { return false; }
            if (requireOverlap && !target.worldBound.Overlaps(skillListRoot.worldBound)) { return false; }

            element = skillListRoot;
            return true;
        }

        /// <summary>
        ///    ドロップ可能なスロットが見つからない場合や、ドロップがキャンセルされた場合に、
        ///    スキル要素をドラッグ開始位置にスナップバックさせるメソッド。
        /// </summary>
        private void SnapBackToStart()
        {
            if (target.parent == null) { return; }

            // ドラッグ開始位置にスキル要素を戻すために、ドラッグ開始時の親要素に再度追加する。
            _startParent.Add(target);

            // スタイルをリセットする。
            target.style.position = Position.Relative;

            // ドラッグ開始位置に戻すために、スタイルの left と top を null に設定する。
            target.style.left = StyleKeyword.Null;
            target.style.top = StyleKeyword.Null;
        }

        /// <summary>
        ///     指定スロット内に含まれるスキル要素を返すメソッド。
        ///     スロット直下に draggable クラスを持つ子要素が存在する場合にそれを返す。
        ///     存在しない場合は null を返す。
        /// </summary>
        /// <param name="slot"> 検索対象のスロット VisualElement。 </param>
        /// <returns> スロット内のスキル要素。存在しない場合は null。 </returns>
        private VisualElement FindSkillInSlot(VisualElement slot)
        {
            // スロット直下の draggable クラスを持つ要素を検索する。
            return slot.Q<VisualElement>(className: DRAGGABLE_CLASS_NAME);
        }

        /// <summary>
        ///     スキル要素を指定した親要素へ移動させるメソッド。
        ///     スワップ時に既存スキルをドラッグ元の親へ戻す際に使用する。
        /// </summary>
        /// <param name="skill"> 移動対象のスキル VisualElement。 </param>
        /// <param name="newParent"> 移動先の親 VisualElement。 </param>
        private void SwapSkillToParent(VisualElement skill, VisualElement newParent)
        {
            if (skill == null || newParent == null) { return; }

            // 移動先の親要素にスキル要素を追加する（元の親から自動的に切り離される）。
            newParent.Add(skill);

            // 移動先の親要素のレイアウトに合わせて、スキル要素のスタイルをリセットする。
            skill.style.position = Position.Relative;
            skill.style.left = StyleKeyword.Null;
            skill.style.top = StyleKeyword.Null;
        }

        /// <summary>
        ///     スキル要素をスロットへ移動し、スロット中央にスナップさせるメソッド。
        /// </summary>
        /// <param name="skill"> 移動対象のスキル VisualElement。 </param>
        /// <param name="slot"> 移動先のスロット VisualElement。 </param>
        private void MoveSkillToSlot(VisualElement skill, VisualElement slot)
        {
            if (skill == null || slot == null) { return; }

            // スロットにスキル要素を追加する（元の親から自動的に切り離される）。
            slot.Add(skill);

            Vector2 skillElementSize = new Vector2(skill.resolvedStyle.width, skill.resolvedStyle.height);

            // スキル要素の中心がスロットの中心に来るように位置を設定する。
            Vector2 desiredWorld = slot.worldBound.center - (skillElementSize * 0.5f);
            Vector2 desiredLocal = slot.WorldToLocal(desiredWorld);

            skill.style.position = Position.Absolute;
            skill.style.left = desiredLocal.x;
            skill.style.top = desiredLocal.y;
        }

        /// <summary>
        ///     ドロップされたスキル要素を入手済みスキルリストへ移動させるメソッド。
        /// </summary>
        /// <param name="skill"> 移動対象のスキル VisualElement。 </param>
        /// <param name="list"> 移動先の入手済みスキルリスト VisualElement。 </param>
        private void MoveSkillToList(VisualElement skill, VisualElement list)
        {
            if (skill == null || list == null) { return; }

            // 入手済みスキルリストにスキル要素を追加する（元の親から自動的に切り離される）。
            list.Add(skill);

            // スタイルをリセットする。
            skill.style.position = Position.Relative;
            skill.style.left = StyleKeyword.Null;
            skill.style.top = StyleKeyword.Null;
        }
    }
}
