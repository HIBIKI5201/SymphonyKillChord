using KillChord.Editor.SourceDataProvider.Core;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.Inspectors.SourceData
{
    /// <summary>
    ///     StageTreeAssetのステージとBindをグラフ表示する。
    /// </summary>
    internal static class StageTreeGraphView
    {
        /// <summary>
        ///     まだ一度も操作されていないPan量を表す番兵値。初回描画時のみ自動フォーカスの対象になる。
        /// </summary>
        public static readonly Vector2 UNINITIALIZED_PAN_OFFSET = new(float.NaN, float.NaN);

        /// <summary>
        ///     指定したStage/Bindアセットを実際に含むStageTreeAssetを、DemoとReleaseの両方から探して解決する。
        ///     ambientな(EditorPrefsに保存された)variantを無条件に信用すると、Releaseを見ている状態で
        ///     Demo専用のStage/Bindアセットを開いた場合に、値編集はDemo・下のグラフはReleaseという不整合が
        ///     起こるため、targetの所属先を明示的に探索する。
        /// </summary>
        /// <param name="stageTreeAddressableKey"> StageTreeAssetのAddressableキー。</param>
        /// <param name="target"> 所属先を調べたいStage/Bindアセット。</param>
        /// <param name="stageTreeAsset"> 一意に解決できた場合の所属StageTreeAsset。</param>
        /// <param name="message"> 解決できなかった場合の理由。</param>
        /// <returns> 所属先を一意に解決できた場合はtrue。</returns>
        public static bool TryFindContainingStageTree(
            string stageTreeAddressableKey,
            ScriptableObject target,
            out ScriptableObject stageTreeAsset,
            out string message)
        {
            stageTreeAsset = null;
            if (target == null)
            {
                message = "対象アセットがありません。";
                return false;
            }

            List<ScriptableObject> containingTrees = new();
            foreach (GameDataVariant variant in (GameDataVariant[])Enum.GetValues(typeof(GameDataVariant)))
            {
                if (!SourceDataProviderRepositoryResolver.TryResolveAsset(
                        stageTreeAddressableKey,
                        variant,
                        out ScriptableObject candidate,
                        out _,
                        out _)
                    || candidate == null
                    || containingTrees.Contains(candidate))
                {
                    continue;
                }

                if (ContainsAsset(candidate, target))
                {
                    containingTrees.Add(candidate);
                }
            }

            if (containingTrees.Count == 0)
            {
                message = $"対象アセットを含むStageTreeAsset(「{stageTreeAddressableKey}」)が"
                    + "Demo/Releaseいずれにも見つかりません。";
                return false;
            }

            if (containingTrees.Count > 1)
            {
                message = "対象アセットを含むStageTreeAssetが複数見つかったため、所属を一意に決定できません。";
                return false;
            }

            stageTreeAsset = containingTrees[0];
            message = null;
            return true;
        }

        /// <summary>
        ///     StageTreeAssetの_stageAssetsまたは_bindAssetsが指定アセットを参照しているか判定する。
        /// </summary>
        /// <param name="stageTreeAsset"> 判定対象のStageTreeAsset。</param>
        /// <param name="target"> 検索するアセット。</param>
        /// <returns> 含まれている場合はtrue。</returns>
        private static bool ContainsAsset(ScriptableObject stageTreeAsset, ScriptableObject target)
        {
            SerializedObject serializedTree = new(stageTreeAsset);
            return PropertyContainsReference(serializedTree.FindProperty(STAGE_ASSETS_PROPERTY_NAME), target)
                || PropertyContainsReference(serializedTree.FindProperty(BIND_ASSETS_PROPERTY_NAME), target);
        }

        /// <summary>
        ///     ObjectReference配列が指定アセットを含むか判定する。
        /// </summary>
        /// <param name="arrayProperty"> 判定対象の配列プロパティ。</param>
        /// <param name="target"> 検索するアセット。</param>
        /// <returns> 含まれている場合はtrue。</returns>
        private static bool PropertyContainsReference(SerializedProperty arrayProperty, ScriptableObject target)
        {
            if (arrayProperty == null || !arrayProperty.isArray)
            {
                return false;
            }

            for (int i = 0; i < arrayProperty.arraySize; i++)
            {
                if (arrayProperty.GetArrayElementAtIndex(i).objectReferenceValue == target)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     StageTreeグラフを描画する。ドラッグでパン移動でき、初回描画時は<paramref name="focusAsset"/>
        ///     (自分自身のノードまたは自分が属するEdge)が見える位置を既定のPan位置として使う。
        /// </summary>
        /// <param name="stageTreeAsset"> 描画対象のStageTreeAsset。</param>
        /// <param name="panOffset">
        ///     呼び出し元(Editor)が保持するPanオフセットです。<see cref="UNINITIALIZED_PAN_OFFSET"/>の場合は
        ///     初回描画とみなし、<paramref name="focusAsset"/>を中心とした位置へ自動補正します。
        /// </param>
        /// <param name="selectedBindAsset">
        ///     選択中のStageBindAsset。指定した場合、対応するEdgeを強調表示する。
        /// </param>
        /// <param name="focusAsset">
        ///     初回描画時に画面中央へ表示したいアセット(StageAssetBaseまたはStageBindAsset)です。
        /// </param>
        public static void Draw(
            ScriptableObject stageTreeAsset,
            ref Vector2 panOffset,
            ScriptableObject selectedBindAsset = null,
            ScriptableObject focusAsset = null)
        {
            EditorGUILayout.LabelField("Stage Tree Graph", EditorStyles.boldLabel);
            if (!TryBuildGraph(
                    stageTreeAsset,
                    out List<NodeInfo> nodes,
                    out List<EdgeInfo> edges,
                    out int rootCount,
                    out bool hasCycle))
            {
                EditorGUILayout.HelpBox(
                    "StageTreeAssetの_stageAssetsまたは_bindAssetsを取得できません。",
                    MessageType.Warning);
                return;
            }

            EditorGUILayout.HelpBox(
                $"Stages: {nodes.Count} / Binds: {edges.Count}　(ドラッグでスクロールできます)",
                MessageType.None);
            if (rootCount > 1)
            {
                EditorGUILayout.HelpBox(
                    $"起点ステージが{rootCount}個あります。左端に複数表示しています。",
                    MessageType.Warning);
            }
            if (hasCycle)
            {
                EditorGUILayout.HelpBox(
                    "Bindに循環があります。循環部分は同じ列へ表示しています。",
                    MessageType.Error);
            }
            if (nodes.Count == 0)
            {
                return;
            }

            AssignNodeRects(nodes);
            float contentWidth = CalculateGraphWidth(nodes);
            float contentHeight = CalculateGraphHeight(nodes);
            float viewportHeight = Mathf.Min(contentHeight, MAX_VIEWPORT_HEIGHT);
            Rect viewportRect = GUILayoutUtility.GetRect(
                0f,
                viewportHeight,
                GUILayout.ExpandWidth(true),
                GUILayout.Height(viewportHeight));

            // LayoutイベントのGUILayoutUtility.GetRectはまだ実際の幅を確定しておらず、(0,0,1,1)等の
            // ダミー値を返すことがある。ここで中央寄せを確定させて書き戻すと誤った幅で計算された値が
            // 以後ずっと使われてしまうため、実際の矩形が確定している非Layoutイベントまで書き戻しを遅延する。
            // Layoutイベント中の描画自体は結果が使われないため、書き戻さない一時値(Vector2.zero)で構わない。
            Vector2 effectivePanOffset = panOffset;
            bool isUninitialized = float.IsNaN(effectivePanOffset.x) || float.IsNaN(effectivePanOffset.y);
            bool isLayoutEvent = Event.current.type == EventType.Layout;
            if (isUninitialized)
            {
                effectivePanOffset = isLayoutEvent
                    ? Vector2.zero
                    : ResolveInitialPanOffset(focusAsset, nodes, edges, viewportRect, contentWidth, contentHeight);
            }

            ClampPanOffset(ref effectivePanOffset, viewportRect, contentWidth, contentHeight);
            if (!isUninitialized || !isLayoutEvent)
            {
                // Layoutイベントで解決した一時値は誤った幅に基づくため書き戻さない。既に確定済みの値、
                // または非Layoutイベントで解決した正しい値のみを呼び出し元へ永続化する。
                panOffset = effectivePanOffset;
            }

            EditorGUI.DrawRect(viewportRect, GRAPH_BACKGROUND_COLOR);
            GUI.BeginGroup(viewportRect);

            Vector2 contentOrigin = -effectivePanOffset;
            DrawGrid(new Rect(Vector2.zero, viewportRect.size));
            DrawEdges(contentOrigin, edges, selectedBindAsset);
            DrawNodes(contentOrigin, nodes);
            HandlePan(new Rect(Vector2.zero, viewportRect.size), ref panOffset, contentWidth, contentHeight);

            GUI.EndGroup();
        }

        /// <summary>
        ///     初回描画時のPanオフセットを、フォーカス対象が画面中央に来るよう計算する。
        ///     フォーカス対象が無い、またはグラフ内に見つからない場合は原点(0,0)を返す。
        /// </summary>
        /// <param name="focusAsset"> 中央に表示したいアセット。</param>
        /// <param name="nodes"> ノード一覧。</param>
        /// <param name="edges"> Edge一覧。</param>
        /// <param name="viewportRect"> 表示領域。</param>
        /// <param name="contentWidth"> コンテンツ全体の幅。</param>
        /// <param name="contentHeight"> コンテンツ全体の高さ。</param>
        /// <returns> 初期Panオフセット。</returns>
        private static Vector2 ResolveInitialPanOffset(
            ScriptableObject focusAsset,
            List<NodeInfo> nodes,
            List<EdgeInfo> edges,
            Rect viewportRect,
            float contentWidth,
            float contentHeight)
        {
            if (focusAsset == null)
            {
                return Vector2.zero;
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Asset == focusAsset)
                {
                    return CenterOn(nodes[i].Rect.center, viewportRect, contentWidth, contentHeight);
                }
            }

            for (int i = 0; i < edges.Count; i++)
            {
                if (edges[i].BindAsset == focusAsset)
                {
                    Vector2 midpoint = Vector2.Lerp(edges[i].From.Rect.center, edges[i].To.Rect.center, 0.5f);
                    return CenterOn(midpoint, viewportRect, contentWidth, contentHeight);
                }
            }

            return Vector2.zero;
        }

        /// <summary>
        ///     指定したコンテンツ座標が画面中央に来るPanオフセットを計算する。
        /// </summary>
        /// <param name="contentPoint"> 中央に表示したいコンテンツ座標。</param>
        /// <param name="viewportRect"> 表示領域。</param>
        /// <param name="contentWidth"> コンテンツ全体の幅。</param>
        /// <param name="contentHeight"> コンテンツ全体の高さ。</param>
        /// <returns> 計算したPanオフセット。</returns>
        private static Vector2 CenterOn(
            Vector2 contentPoint,
            Rect viewportRect,
            float contentWidth,
            float contentHeight)
        {
            Vector2 offset = new(
                contentPoint.x - viewportRect.width * 0.5f,
                contentPoint.y - viewportRect.height * 0.5f);
            ClampPanOffset(ref offset, viewportRect, contentWidth, contentHeight);
            return offset;
        }

        /// <summary>
        ///     Panオフセットがコンテンツ範囲内に収まるよう補正する。
        /// </summary>
        /// <param name="panOffset"> 補正対象のPanオフセット。</param>
        /// <param name="viewportRect"> 表示領域。</param>
        /// <param name="contentWidth"> コンテンツ全体の幅。</param>
        /// <param name="contentHeight"> コンテンツ全体の高さ。</param>
        private static void ClampPanOffset(
            ref Vector2 panOffset,
            Rect viewportRect,
            float contentWidth,
            float contentHeight)
        {
            float maxX = Mathf.Max(0f, contentWidth - viewportRect.width);
            float maxY = Mathf.Max(0f, contentHeight - viewportRect.height);
            panOffset.x = Mathf.Clamp(panOffset.x, 0f, maxX);
            panOffset.y = Mathf.Clamp(panOffset.y, 0f, maxY);
        }

        /// <summary>
        ///     空白部分のドラッグによるPan操作を処理する。ノードのGUI.Buttonが既に消費したイベントには反応しないため、
        ///     ノードクリックとPanドラッグは共存できる。
        /// </summary>
        /// <param name="localViewportRect"> グループ内ローカル座標での表示領域。</param>
        /// <param name="panOffset"> 更新対象のPanオフセット。</param>
        /// <param name="contentWidth"> コンテンツ全体の幅。</param>
        /// <param name="contentHeight"> コンテンツ全体の高さ。</param>
        private static void HandlePan(
            Rect localViewportRect,
            ref Vector2 panOffset,
            float contentWidth,
            float contentHeight)
        {
            int controlId = GUIUtility.GetControlID(PAN_HINT.GetHashCode(), FocusType.Passive, localViewportRect);
            Event current = Event.current;
            switch (current.GetTypeForControl(controlId))
            {
                case EventType.MouseDown:
                    if (current.button == 0 && localViewportRect.Contains(current.mousePosition))
                    {
                        GUIUtility.hotControl = controlId;
                        current.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlId)
                    {
                        panOffset -= current.delta;
                        ClampPanOffset(ref panOffset, localViewportRect, contentWidth, contentHeight);
                        current.Use();
                        GUI.changed = true;
                    }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlId)
                    {
                        GUIUtility.hotControl = 0;
                        current.Use();
                    }
                    break;
            }
        }

        /// <summary>
        ///     SerializedPropertyからグラフ情報を構築する。
        /// </summary>
        /// <param name="stageTreeAsset"> StageTreeAsset。</param>
        /// <param name="nodes"> 構築したノード一覧。</param>
        /// <param name="edges"> 構築したEdge一覧。</param>
        /// <param name="rootCount"> 起点ノード数。</param>
        /// <param name="hasCycle"> 循環がある場合はtrue。</param>
        /// <returns> 必要なプロパティを取得できた場合はtrue。</returns>
        private static bool TryBuildGraph(
            ScriptableObject stageTreeAsset,
            out List<NodeInfo> nodes,
            out List<EdgeInfo> edges,
            out int rootCount,
            out bool hasCycle)
        {
            nodes = new List<NodeInfo>();
            edges = new List<EdgeInfo>();
            rootCount = 0;
            hasCycle = false;
            if (stageTreeAsset == null)
            {
                return false;
            }

            SerializedObject serializedTree = new(stageTreeAsset);
            SerializedProperty stageAssets = serializedTree.FindProperty(STAGE_ASSETS_PROPERTY_NAME);
            SerializedProperty bindAssets = serializedTree.FindProperty(BIND_ASSETS_PROPERTY_NAME);
            if (stageAssets == null || bindAssets == null)
            {
                return false;
            }

            Dictionary<int, NodeInfo> nodeMap = new();
            for (int i = 0; i < stageAssets.arraySize; i++)
            {
                if (stageAssets.GetArrayElementAtIndex(i).objectReferenceValue
                    is not ScriptableObject stageAsset)
                {
                    continue;
                }

                NodeInfo node = CreateNode(stageAsset, i);
                nodes.Add(node);

                SerializedProperty stageIdProperty = new SerializedObject(stageAsset)
                    .FindProperty(STAGE_ID_PROPERTY_NAME)
                    ?.FindPropertyRelative(DATA_ID_HASH_PROPERTY_NAME);
                if (stageIdProperty != null)
                {
                    nodeMap[stageIdProperty.intValue] = node;
                }
            }

            for (int i = 0; i < bindAssets.arraySize; i++)
            {
                if (bindAssets.GetArrayElementAtIndex(i).objectReferenceValue
                    is not ScriptableObject bindAsset)
                {
                    continue;
                }

                SerializedObject serializedBind = new(bindAsset);
                SerializedProperty fromStageIdProperty = serializedBind.FindProperty(FROM_STAGE_PROPERTY_NAME)
                    ?.FindPropertyRelative(DATA_ID_HASH_PROPERTY_NAME);
                SerializedProperty toStageIdProperty = serializedBind.FindProperty(TO_STAGE_PROPERTY_NAME)
                    ?.FindPropertyRelative(DATA_ID_HASH_PROPERTY_NAME);
                SerializedProperty advanceMode = serializedBind.FindProperty(ADVANCE_MODE_PROPERTY_NAME);
                if (fromStageIdProperty == null
                    || toStageIdProperty == null
                    || !nodeMap.TryGetValue(fromStageIdProperty.intValue, out NodeInfo fromNode)
                    || !nodeMap.TryGetValue(toStageIdProperty.intValue, out NodeInfo toNode))
                {
                    continue;
                }

                EdgeInfo edge = new()
                {
                    From = fromNode,
                    To = toNode,
                    IsAutoAdvance = advanceMode != null && advanceMode.enumValueIndex == AUTO_ADVANCE_ENUM_INDEX,
                    BindAsset = bindAsset,
                };
                edges.Add(edge);
                fromNode.Outgoing.Add(toNode);
                toNode.IncomingCount++;
            }

            AssignColumns(nodes, out rootCount, out hasCycle);
            return true;
        }

        /// <summary>
        ///     ステージアセットから表示ノードを生成する。
        /// </summary>
        /// <param name="stageAsset"> ステージアセット。</param>
        /// <param name="collectionIndex"> Collection内のインデックス。</param>
        /// <returns> 表示ノード。</returns>
        private static NodeInfo CreateNode(ScriptableObject stageAsset, int collectionIndex)
        {
            SerializedObject serializedStage = new(stageAsset);
            string stageName = serializedStage.FindProperty(STAGE_NAME_PROPERTY_NAME)?.stringValue;
            if (string.IsNullOrWhiteSpace(stageName))
            {
                stageName = stageAsset.name;
            }

            return new NodeInfo
            {
                Asset = stageAsset,
                CollectionIndex = collectionIndex,
                Label = stageName,
                IsBattle = stageAsset.GetType().Name.Contains("Battle", StringComparison.Ordinal),
            };
        }

        /// <summary>
        ///     Kahn法で各ノードの列を決定する。
        /// </summary>
        /// <param name="nodes"> ノード一覧。</param>
        /// <param name="rootCount"> 起点数。</param>
        /// <param name="hasCycle"> 循環がある場合はtrue。</param>
        private static void AssignColumns(
            List<NodeInfo> nodes,
            out int rootCount,
            out bool hasCycle)
        {
            Dictionary<NodeInfo, int> remainingIncoming = new();
            Queue<NodeInfo> queue = new();
            for (int i = 0; i < nodes.Count; i++)
            {
                NodeInfo node = nodes[i];
                remainingIncoming[node] = node.IncomingCount;
                if (node.IncomingCount == 0)
                {
                    queue.Enqueue(node);
                }
            }

            rootCount = queue.Count;
            int processedCount = 0;
            while (queue.Count > 0)
            {
                NodeInfo current = queue.Dequeue();
                processedCount++;
                for (int i = 0; i < current.Outgoing.Count; i++)
                {
                    NodeInfo next = current.Outgoing[i];
                    next.Column = Mathf.Max(next.Column, current.Column + 1);
                    remainingIncoming[next]--;
                    if (remainingIncoming[next] == 0)
                    {
                        queue.Enqueue(next);
                    }
                }
            }

            hasCycle = processedCount != nodes.Count;
        }

        /// <summary>
        ///     列内の行位置とノード矩形を決定する。
        /// </summary>
        /// <param name="nodes"> ノード一覧。</param>
        private static void AssignNodeRects(List<NodeInfo> nodes)
        {
            Dictionary<int, List<NodeInfo>> columns = new();
            for (int i = 0; i < nodes.Count; i++)
            {
                NodeInfo node = nodes[i];
                if (!columns.TryGetValue(node.Column, out List<NodeInfo> columnNodes))
                {
                    columnNodes = new List<NodeInfo>();
                    columns.Add(node.Column, columnNodes);
                }

                columnNodes.Add(node);
            }

            foreach (KeyValuePair<int, List<NodeInfo>> pair in columns)
            {
                List<NodeInfo> columnNodes = pair.Value;
                for (int row = 0; row < columnNodes.Count; row++)
                {
                    NodeInfo node = columnNodes[row];
                    node.Rect = new Rect(
                        GRAPH_PADDING + node.Column * COLUMN_SPACING,
                        GRAPH_PADDING + row * ROW_SPACING,
                        NODE_WIDTH,
                        NODE_HEIGHT);
                }
            }
        }

        /// <summary>
        ///     グラフの必要幅を計算する。
        /// </summary>
        /// <param name="nodes"> ノード一覧。</param>
        /// <returns> 必要幅。</returns>
        private static float CalculateGraphWidth(List<NodeInfo> nodes)
        {
            int maxColumn = 0;
            for (int i = 0; i < nodes.Count; i++)
            {
                maxColumn = Mathf.Max(maxColumn, nodes[i].Column);
            }

            return GRAPH_PADDING * 2f + NODE_WIDTH + maxColumn * COLUMN_SPACING;
        }

        /// <summary>
        ///     グラフの必要高さを計算する。
        /// </summary>
        /// <param name="nodes"> ノード一覧。</param>
        /// <returns> 必要高さ。</returns>
        private static float CalculateGraphHeight(List<NodeInfo> nodes)
        {
            Dictionary<int, int> columnCounts = new();
            int maxRows = 1;
            for (int i = 0; i < nodes.Count; i++)
            {
                NodeInfo node = nodes[i];
                columnCounts.TryGetValue(node.Column, out int count);
                count++;
                columnCounts[node.Column] = count;
                maxRows = Mathf.Max(maxRows, count);
            }

            return Mathf.Max(
                MIN_GRAPH_HEIGHT,
                GRAPH_PADDING * 2f + NODE_HEIGHT + (maxRows - 1) * ROW_SPACING);
        }

        /// <summary>
        ///     グラフ背景のグリッドを描画する。
        /// </summary>
        /// <param name="viewportLocalRect"> グループ内ローカル座標での表示領域。</param>
        private static void DrawGrid(Rect viewportLocalRect)
        {
            Handles.color = GRID_COLOR;
            for (float x = viewportLocalRect.x; x < viewportLocalRect.xMax; x += GRID_SIZE)
            {
                Handles.DrawLine(new Vector2(x, viewportLocalRect.y), new Vector2(x, viewportLocalRect.yMax));
            }
            for (float y = viewportLocalRect.y; y < viewportLocalRect.yMax; y += GRID_SIZE)
            {
                Handles.DrawLine(new Vector2(viewportLocalRect.x, y), new Vector2(viewportLocalRect.xMax, y));
            }
        }

        /// <summary>
        ///     BindをEdgeとして描画する。
        /// </summary>
        /// <param name="contentOrigin"> コンテンツ原点(Panオフセット適用後)のグループ内ローカル座標。</param>
        /// <param name="edges"> Edge一覧。</param>
        /// <param name="selectedBindAsset"> 選択中のStageBindAsset。強調表示に使用する。</param>
        private static void DrawEdges(Vector2 contentOrigin, List<EdgeInfo> edges, ScriptableObject selectedBindAsset)
        {
            for (int i = 0; i < edges.Count; i++)
            {
                EdgeInfo edge = edges[i];
                Vector2 start = contentOrigin + new Vector2(
                    edge.From.Rect.xMax,
                    edge.From.Rect.center.y);
                Vector2 end = contentOrigin + new Vector2(
                    edge.To.Rect.xMin,
                    edge.To.Rect.center.y);
                bool isSelected = selectedBindAsset != null && edge.BindAsset == selectedBindAsset;
                Color edgeColor = isSelected
                    ? SELECTED_EDGE_COLOR
                    : edge.IsAutoAdvance ? AUTO_EDGE_COLOR : MANUAL_EDGE_COLOR;
                Handles.DrawBezier(
                    start,
                    end,
                    start + Vector2.right * EDGE_TANGENT,
                    end + Vector2.left * EDGE_TANGENT,
                    edgeColor,
                    null,
                    isSelected ? EDGE_WIDTH * SELECTED_EDGE_WIDTH_SCALE : EDGE_WIDTH);

                Rect labelRect = new(
                    (start.x + end.x) * 0.5f - EDGE_LABEL_WIDTH * 0.5f,
                    (start.y + end.y) * 0.5f - 10f,
                    EDGE_LABEL_WIDTH,
                    20f);
                GUI.Label(
                    labelRect,
                    edge.IsAutoAdvance ? "自動遷移" : "ホーム",
                    EditorStyles.centeredGreyMiniLabel);
            }
        }

        /// <summary>
        ///     ステージノードを描画する。
        /// </summary>
        /// <param name="contentOrigin"> コンテンツ原点(Panオフセット適用後)のグループ内ローカル座標。</param>
        /// <param name="nodes"> ノード一覧。</param>
        private static void DrawNodes(Vector2 contentOrigin, List<NodeInfo> nodes)
        {
            Color previousBackgroundColor = GUI.backgroundColor;
            for (int i = 0; i < nodes.Count; i++)
            {
                NodeInfo node = nodes[i];
                Rect nodeRect = node.Rect;
                nodeRect.position += contentOrigin;
                GUI.backgroundColor = Selection.activeObject == node.Asset
                    ? SELECTED_NODE_COLOR
                    : node.IsBattle ? BATTLE_NODE_COLOR : SCENARIO_NODE_COLOR;
                if (GUI.Button(nodeRect, node.Label, EditorStyles.miniButton))
                {
                    Selection.activeObject = node.Asset;
                    EditorGUIUtility.PingObject(node.Asset);
                }

                Rect typeRect = new(nodeRect.x, nodeRect.yMax - 18f, nodeRect.width, 16f);
                GUI.Label(
                    typeRect,
                    node.IsBattle ? "Battle" : "Scenario",
                    EditorStyles.centeredGreyMiniLabel);
            }

            GUI.backgroundColor = previousBackgroundColor;
        }

        private const float GRAPH_PADDING = 32f;
        private const float NODE_WIDTH = 136f;
        private const float NODE_HEIGHT = 58f;
        private const float COLUMN_SPACING = 210f;
        private const float ROW_SPACING = 92f;
        private const float MIN_GRAPH_HEIGHT = 300f;
        private const float MAX_VIEWPORT_HEIGHT = 480f;
        private const float GRID_SIZE = 24f;
        private const float EDGE_TANGENT = 54f;
        private const float EDGE_WIDTH = 3f;
        private const float SELECTED_EDGE_WIDTH_SCALE = 1.6f;
        private const float EDGE_LABEL_WIDTH = 64f;
        private const int AUTO_ADVANCE_ENUM_INDEX = 1;
        private const string PAN_HINT = "StageTreeGraphViewPan";
        private const string STAGE_ASSETS_PROPERTY_NAME = "_stageAssets";
        private const string BIND_ASSETS_PROPERTY_NAME = "_bindAssets";
        private const string STAGE_NAME_PROPERTY_NAME = "_stageName";
        private const string STAGE_ID_PROPERTY_NAME = "_stageId";
        private const string FROM_STAGE_PROPERTY_NAME = "_fromStageId";
        private const string TO_STAGE_PROPERTY_NAME = "_toStageId";
        private const string DATA_ID_HASH_PROPERTY_NAME = "_hashId";
        private const string ADVANCE_MODE_PROPERTY_NAME = "_advanceMode";

        private static readonly Color GRAPH_BACKGROUND_COLOR = new(0.14f, 0.14f, 0.16f, 1f);
        private static readonly Color GRID_COLOR = new(0.22f, 0.22f, 0.24f, 1f);
        private static readonly Color BATTLE_NODE_COLOR = new(0.48f, 0.68f, 0.92f, 1f);
        private static readonly Color SCENARIO_NODE_COLOR = new(0.78f, 0.58f, 0.88f, 1f);
        private static readonly Color SELECTED_NODE_COLOR = new(1f, 0.78f, 0.28f, 1f);
        private static readonly Color AUTO_EDGE_COLOR = new(0.35f, 0.9f, 0.55f, 1f);
        private static readonly Color MANUAL_EDGE_COLOR = new(0.65f, 0.68f, 0.72f, 1f);
        private static readonly Color SELECTED_EDGE_COLOR = new(1f, 0.78f, 0.28f, 1f);

        /// <summary>
        ///     グラフ表示用のステージノード情報。
        /// </summary>
        private sealed class NodeInfo
        {
            public ScriptableObject Asset;
            public string Label;
            public int CollectionIndex;
            public int Column;
            public int IncomingCount;
            public bool IsBattle;
            public Rect Rect;
            public readonly List<NodeInfo> Outgoing = new();
        }

        /// <summary>
        ///     グラフ表示用のBind情報。
        /// </summary>
        private sealed class EdgeInfo
        {
            public NodeInfo From;
            public NodeInfo To;
            public bool IsAutoAdvance;
            public ScriptableObject BindAsset;
        }
    }
}
