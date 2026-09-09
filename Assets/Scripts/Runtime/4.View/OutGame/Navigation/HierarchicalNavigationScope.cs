using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Navigation
{
    /// <summary>
    ///     親子関係を持つ UI 操作レベルを切り替え、現在レベルだけをフォーカス可能にするクラスです。
    /// </summary>
    public sealed class HierarchicalNavigationScope : IDisposable
    {
        /// <summary>
        ///     階層フォーカス制御の対象ルートを設定します。
        /// </summary>x
        /// <param name="rootElement"> 階層内のイベントを受け取るルート要素です。 </param>
        /// <exception cref="ArgumentNullException"> rootElement が null の場合にスローされます。 </exception>
        public HierarchicalNavigationScope(VisualElement rootElement)
        {
            _rootElement = rootElement
                ?? throw new ArgumentNullException(nameof(rootElement));
        }

        /// <summary>
        ///     最上位で操作可能にする要素群を登録します。
        /// </summary>
        /// <param name="elements"> 最上位の操作要素群です。 </param>
        public void SetRootLevel(IReadOnlyList<VisualElement> elements)
        {
            ThrowIfDisposed();
            ThrowIfStarted();

            if (_rootNode != null)
            {
                throw new InvalidOperationException("ルートレベルは既に登録されています。");
            }

            VisualElement[] copiedElements = CopyAndValidateElements(elements, nameof(elements));
            ValidateNewOperationRanges(copiedElements, null);

            _rootNode = new LevelNode(null, null, copiedElements, null);
            RegisterNode(_rootNode);
        }

        /// <summary>
        ///     登録済みの入口要素から進む子レベルを登録します。
        /// </summary>
        /// <param name="entryElement"> 親レベルに登録済みの入口要素です。 </param>
        /// <param name="elements"> 子レベルで操作可能にする要素群です。 </param>
        /// <param name="initialFocusElement"> 子レベルへ入った直後のフォーカス先です。 </param>
        public void AddChildLevel(
            VisualElement entryElement,
            IReadOnlyList<VisualElement> elements,
            VisualElement initialFocusElement)
        {
            ThrowIfDisposed();
            ThrowIfStarted();

            if (_rootNode == null)
            {
                throw new InvalidOperationException("子レベルより先にルートレベルを登録してください。");
            }

            if (entryElement == null)
            {
                throw new ArgumentNullException(nameof(entryElement));
            }

            if (initialFocusElement == null)
            {
                throw new ArgumentNullException(nameof(initialFocusElement));
            }

            ValidateElementWithinRoot(entryElement, nameof(entryElement));
            ValidateElementWithinRoot(initialFocusElement, nameof(initialFocusElement));

            if (!_elementOwners.TryGetValue(entryElement, out LevelNode parentNode))
            {
                throw new InvalidOperationException("入口要素は登録済みレベルの操作要素である必要があります。");
            }

            if (_childrenByEntry.ContainsKey(entryElement))
            {
                throw new ArgumentException("同じ入口要素には複数の子レベルを登録できません。", nameof(entryElement));
            }

            VisualElement[] copiedElements = CopyAndValidateElements(elements, nameof(elements));
            ValidateNewOperationRanges(copiedElements, entryElement);

            if (!ContainsElementOrDescendant(copiedElements, initialFocusElement))
            {
                throw new ArgumentException(
                    "初期フォーカス先は子レベルの操作要素自身またはその子孫である必要があります。",
                    nameof(initialFocusElement));
            }

            var childNode = new LevelNode(
                parentNode,
                entryElement,
                copiedElements,
                initialFocusElement);
            _childrenByEntry.Add(entryElement, childNode);
            RegisterNode(childNode);
        }

        /// <summary>
        ///     最上位レベルだけをフォーカス可能にします。
        /// </summary>
        public void ResetToRootLevel()
        {
            ThrowIfDisposed();
            EnsureStarted();
            ChangeCurrentLevel(_rootNode);
            CancelPendingFocus();
        }

        /// <summary>
        ///     指定した入口要素に対応する子レベルへ進みます。
        /// </summary>
        /// <param name="entryElement"> 遷移先を示す入口要素です。 </param>
        public void EnterLevel(VisualElement entryElement)
        {
            ThrowIfDisposed();

            if (entryElement == null)
            {
                throw new ArgumentNullException(nameof(entryElement));
            }

            EnsureStarted();

            if (!_childrenByEntry.TryGetValue(entryElement, out LevelNode targetNode))
            {
                throw new ArgumentException("入口要素に対応する子レベルが登録されていません。", nameof(entryElement));
            }

            if (!IsCurrentNodeOrAncestor(targetNode.Parent))
            {
                throw new InvalidOperationException("現在の階層から到達できない子レベルへは遷移できません。");
            }

            ChangeCurrentLevel(targetNode);
            RequestFocus(targetNode, targetNode.InitialFocusElement);
        }

        /// <summary>
        ///     イベント購読と保留中のフォーカス要求を解除し、変更した要素状態を復元します。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            CancelPendingFocus();

            if (_isStarted)
            {
                _rootElement.UnregisterCallback<NavigationCancelEvent>(
                    HandleNavigationCancelHandler,
                    TrickleDown.TrickleDown);

                for (int i = 0; i < _elementStates.Count; i++)
                {
                    _elementStates[i].Restore();
                }
            }

            _isDisposed = true;
        }

        private readonly VisualElement _rootElement;
        private readonly List<LevelNode> _nodes = new();
        private readonly Dictionary<VisualElement, LevelNode> _elementOwners = new();
        private readonly Dictionary<VisualElement, LevelNode> _childrenByEntry = new();
        private readonly List<ElementState> _elementStates = new();
        private LevelNode _rootNode;
        private LevelNode _currentNode;
        private IVisualElementScheduledItem _pendingFocusItem;
        private int _focusRequestGeneration;
        private bool _isStarted;
        private bool _isDisposed;

        /// <summary>
        ///     子レベル内のキャンセルを親レベルへの復帰に変換します。
        /// </summary>
        /// <param name="navigationEvent"> ナビゲーションキャンセルイベントです。 </param>
        private void HandleNavigationCancelHandler(NavigationCancelEvent navigationEvent)
        {
            if (_currentNode == null ||
                ReferenceEquals(_currentNode, _rootNode) ||
                _currentNode.Parent == null ||
                _currentNode.EntryElement == null ||
                navigationEvent.target is not VisualElement targetElement ||
                !ContainsElementOrDescendant(_currentNode.Elements, targetElement))
            {
                return;
            }

            VisualElement returningEntryElement = _currentNode.EntryElement;
            LevelNode parentNode = _currentNode.Parent;
            ChangeCurrentLevel(parentNode);
            RequestFocus(parentNode, returningEntryElement);
            navigationEvent.StopPropagation();
        }

        /// <summary>
        ///     保留中のフォーカス要求が現在も有効な場合にフォーカスを移します。
        /// </summary>
        /// <param name="expectedNode"> 要求時点の遷移先レベルです。 </param>
        /// <param name="focusElement"> フォーカス先です。 </param>
        /// <param name="requestGeneration"> 要求時点の世代です。 </param>
        private void HandleDeferredFocusHandler(
            LevelNode expectedNode,
            VisualElement focusElement,
            int requestGeneration)
        {
            if (_isDisposed ||
                requestGeneration != _focusRequestGeneration ||
                !ReferenceEquals(_currentNode, expectedNode))
            {
                return;
            }

            _pendingFocusItem = null;

            if (focusElement.panel == null ||
                !focusElement.focusable ||
                !focusElement.enabledInHierarchy ||
                focusElement.resolvedStyle.display == DisplayStyle.None ||
                focusElement.resolvedStyle.visibility != Visibility.Visible)
            {
                return;
            }

            focusElement.Focus();
        }

        /// <summary>
        ///     初回遷移時に登録を確定し、要素状態の保存とイベント購読を行います。
        /// </summary>
        private void EnsureStarted()
        {
            if (_rootNode == null)
            {
                throw new InvalidOperationException("ルートレベルが登録されていません。");
            }

            if (_isStarted)
            {
                return;
            }

            foreach (VisualElement element in _elementOwners.Keys)
            {
                _elementStates.Add(new ElementState(element));
            }

            _rootElement.RegisterCallback<NavigationCancelEvent>(
                HandleNavigationCancelHandler,
                TrickleDown.TrickleDown);
            _currentNode = _rootNode;
            _isStarted = true;
        }

        /// <summary>
        ///     現在レベルだけをナビゲーション対象にします。
        /// </summary>
        /// <param name="targetNode"> 有効化するレベルです。 </param>
        private void ChangeCurrentLevel(LevelNode targetNode)
        {
            for (int nodeIndex = 0; nodeIndex < _nodes.Count; nodeIndex++)
            {
                VisualElement[] elements = _nodes[nodeIndex].Elements;
                for (int elementIndex = 0; elementIndex < elements.Length; elementIndex++)
                {
                    elements[elementIndex].ExcludeFromNavigation();
                }
            }

            for (int i = 0; i < targetNode.Elements.Length; i++)
            {
                targetNode.Elements[i].MakeNavigable();
            }

            _currentNode = targetNode;
        }

        /// <summary>
        ///     次回レイアウト後のフォーカス要求を登録します。
        /// </summary>
        /// <param name="expectedNode"> 要求を適用するレベルです。 </param>
        /// <param name="focusElement"> フォーカス先です。 </param>
        private void RequestFocus(LevelNode expectedNode, VisualElement focusElement)
        {
            CancelPendingFocus();
            int requestGeneration = _focusRequestGeneration;
            _pendingFocusItem = _rootElement.schedule.Execute(() =>
                HandleDeferredFocusHandler(expectedNode, focusElement, requestGeneration));
        }

        /// <summary>
        ///     保留中のフォーカス要求を無効化します。
        /// </summary>
        private void CancelPendingFocus()
        {
            _focusRequestGeneration++;
            _pendingFocusItem?.Pause();
            _pendingFocusItem = null;
        }

        /// <summary>
        ///     指定ノードが現在ノードまたはその祖先かを判定します。
        /// </summary>
        /// <param name="node"> 判定するノードです。 </param>
        /// <returns> 現在ノードまたは祖先の場合は true です。 </returns>
        private bool IsCurrentNodeOrAncestor(LevelNode node)
        {
            for (LevelNode current = _currentNode; current != null; current = current.Parent)
            {
                if (ReferenceEquals(current, node))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     操作要素一覧をコピーし、基本条件を検証します。
        /// </summary>
        /// <param name="elements"> コピー元の要素一覧です。 </param>
        /// <param name="parameterName"> 例外へ設定する引数名です。 </param>
        /// <returns> 検証済みの要素配列です。 </returns>
        private VisualElement[] CopyAndValidateElements(
            IReadOnlyList<VisualElement> elements,
            string parameterName)
        {
            if (elements == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (elements.Count == 0)
            {
                throw new ArgumentException("操作要素一覧は空にできません。", parameterName);
            }

            var copiedElements = new VisualElement[elements.Count];
            for (int i = 0; i < elements.Count; i++)
            {
                VisualElement element = elements[i]
                    ?? throw new ArgumentException("操作要素一覧に null は指定できません。", parameterName);
                ValidateElementWithinRoot(element, parameterName);
                copiedElements[i] = element;
            }

            return copiedElements;
        }

        /// <summary>
        ///     新しい操作範囲が既存または同時登録する範囲と重ならないことを検証します。
        /// </summary>
        /// <param name="elements"> 新しく登録する操作要素群です。 </param>
        /// <param name="entryElement"> 子レベルの入口要素です。ルートの場合は null です。 </param>
        private void ValidateNewOperationRanges(
            IReadOnlyList<VisualElement> elements,
            VisualElement entryElement)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                VisualElement element = elements[i];

                if (entryElement != null && AreRangesOverlapping(element, entryElement))
                {
                    throw new ArgumentException("入口要素と子レベルの操作範囲は重複できません。", nameof(elements));
                }

                for (int newIndex = 0; newIndex < i; newIndex++)
                {
                    if (AreRangesOverlapping(element, elements[newIndex]))
                    {
                        throw new ArgumentException("同じレベル内の操作範囲は重複できません。", nameof(elements));
                    }
                }

                foreach (VisualElement registeredElement in _elementOwners.Keys)
                {
                    if (AreRangesOverlapping(element, registeredElement))
                    {
                        throw new ArgumentException("登録済みの操作範囲と重複する要素は登録できません。", nameof(elements));
                    }
                }
            }
        }

        /// <summary>
        ///     要素が Scope のルート配下にあることを検証します。
        /// </summary>
        /// <param name="element"> 検証する要素です。 </param>
        /// <param name="parameterName"> 例外へ設定する引数名です。 </param>
        private void ValidateElementWithinRoot(VisualElement element, string parameterName)
        {
            if (!ReferenceEquals(element, _rootElement) && !_rootElement.Contains(element))
            {
                throw new ArgumentException("要素は Scope のルート配下にある必要があります。", parameterName);
            }
        }

        /// <summary>
        ///     二つの操作範囲が同一または祖先・子孫関係にあるかを判定します。
        /// </summary>
        /// <param name="left"> 一方の操作要素です。 </param>
        /// <param name="right"> もう一方の操作要素です。 </param>
        /// <returns> 操作範囲が重なる場合は true です。 </returns>
        private static bool AreRangesOverlapping(VisualElement left, VisualElement right)
        {
            return ReferenceEquals(left, right) || left.Contains(right) || right.Contains(left);
        }

        /// <summary>
        ///     一覧内の操作要素自身または子孫に対象要素があるかを判定します。
        /// </summary>
        /// <param name="elements"> 操作要素群です。 </param>
        /// <param name="targetElement"> 判定対象です。 </param>
        /// <returns> 操作範囲内にある場合は true です。 </returns>
        private static bool ContainsElementOrDescendant(
            IReadOnlyList<VisualElement> elements,
            VisualElement targetElement)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                if (ReferenceEquals(elements[i], targetElement) || elements[i].Contains(targetElement))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     ノードの操作要素を所有者表へ登録します。
        /// </summary>
        /// <param name="node"> 登録するノードです。 </param>
        private void RegisterNode(LevelNode node)
        {
            _nodes.Add(node);
            for (int i = 0; i < node.Elements.Length; i++)
            {
                _elementOwners.Add(node.Elements[i], node);
            }
        }

        /// <summary>
        ///     Scope が破棄済みの場合に例外をスローします。
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(HierarchicalNavigationScope));
            }
        }

        /// <summary>
        ///     Scope の運用開始後に登録変更を試みた場合に例外をスローします。
        /// </summary>
        private void ThrowIfStarted()
        {
            if (_isStarted)
            {
                throw new InvalidOperationException("運用開始後は階層を変更できません。");
            }
        }

        /// <summary>
        ///     一つの操作レベルと親子関係を保持します。
        /// </summary>
        private sealed class LevelNode
        {
            /// <summary>
            ///     操作レベルを初期化します。
            /// </summary>
            /// <param name="parent"> 親レベルです。 </param>
            /// <param name="entryElement"> 親からこのレベルへ進む入口要素です。 </param>
            /// <param name="elements"> このレベルの操作要素群です。 </param>
            /// <param name="initialFocusElement"> このレベルへ入った直後のフォーカス先です。 </param>
            public LevelNode(
                LevelNode parent,
                VisualElement entryElement,
                VisualElement[] elements,
                VisualElement initialFocusElement)
            {
                Parent = parent;
                EntryElement = entryElement;
                Elements = elements;
                InitialFocusElement = initialFocusElement;
            }

            /// <summary> 親レベルです。 </summary>
            public LevelNode Parent { get; }

            /// <summary> 親からこのレベルへ進む入口要素です。 </summary>
            public VisualElement EntryElement { get; }

            /// <summary> このレベルの操作要素群です。 </summary>
            public VisualElement[] Elements { get; }

            /// <summary> このレベルへ入った直後のフォーカス先です。 </summary>
            public VisualElement InitialFocusElement { get; }
        }

        /// <summary>
        ///     Scope 開始前の要素状態を保持します。
        /// </summary>
        private readonly struct ElementState
        {
            /// <summary>
            ///     要素の現在状態を保存します。
            /// </summary>
            /// <param name="element"> 保存対象の要素です。 </param>
            public ElementState(VisualElement element)
            {
                _element = element;
                _isFocusable = element.focusable;
                _tabIndex = element.tabIndex;
                _pickingMode = element.pickingMode;
                _hasNavigableClass = element.ClassListContains(
                    UINavigationExtensions.NAVIGABLE_CLASS_NAME);
            }

            /// <summary>
            ///     保存した要素状態を復元します。
            /// </summary>
            public void Restore()
            {
                _element.focusable = _isFocusable;
                _element.tabIndex = _tabIndex;
                _element.pickingMode = _pickingMode;
                _element.EnableInClassList(
                    UINavigationExtensions.NAVIGABLE_CLASS_NAME,
                    _hasNavigableClass);
            }

            private readonly VisualElement _element;
            private readonly bool _isFocusable;
            private readonly int _tabIndex;
            private readonly PickingMode _pickingMode;
            private readonly bool _hasNavigableClass;
        }
    }
}
