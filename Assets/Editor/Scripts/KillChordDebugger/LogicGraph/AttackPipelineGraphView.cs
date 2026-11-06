using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Editor.Debugger.LogicGraph
{
    /// <summary>
    ///     攻撃パイプライン1回分の実行記録を、左から右へ流れるノードのグラフで表示する。
    ///     <para> ボードはドラッグで移動、ホイールで拡大縮小できる。編集はしない。 </para>
    /// </summary>
    public sealed class AttackPipelineGraphView : GraphView
    {
        /// <summary>
        ///     パン・ズーム・ミニマップ・背景グリッドを設定して初期化する。
        /// </summary>
        public AttackPipelineGraphView()
        {
            AddToClassList("lgd-graph");

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            GridBackground grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            MiniMap miniMap = new MiniMap { anchored = true };
            miniMap.SetPosition(new Rect(MINIMAP_MARGIN, MINIMAP_MARGIN, MINIMAP_WIDTH, MINIMAP_HEIGHT));
            Add(miniMap);
        }

        /// <summary>
        ///     表示中のノードをすべて消す。
        /// </summary>
        public void ClearGraph()
        {
            DeleteElements(graphElements.ToList());
        }

        /// <summary>
        ///     実行記録をグラフにして表示する。
        /// </summary>
        /// <param name="record"> 表示する記録。nullの場合は何も表示しない。 </param>
        public void ShowRecord(AttackTraceRecord record)
        {
            ClearGraph();
            if (record == null) { return; }

            // 開始ノード → 各ステップ → 結果ノードを左から右へ並べ、順につなぐ。
            Node startNode = CreateNode(
                $"開始 #{record.SequenceNumber}",
                "lgd-node--start",
                0,
                hasInput: false,
                record.AttackName,
                $"基礎ダメージ {FormatDamage(record.BaseDamage)}",
                $"Just {FormatFlag(record.IsJustHit)} / 射程外 {FormatFlag(record.IsOutOfRange)}",
                $"frame {record.FrameCount} / {record.Time:F2}s");
            Node previousNode = startNode;

            for (int i = 0; i < record.Steps.Count; i++)
            {
                AttackStepTraceEntry step = record.Steps[i];
                Node stepNode = CreateNode(
                    step.StepName,
                    step.HasDamageChanged ? "lgd-node--changed" : "lgd-node--unchanged",
                    i + 1,
                    hasInput: true,
                    $"ダメージ {FormatDamage(step.DamageBefore)} → {FormatDamage(step.DamageAfter)}",
                    FormatRatio(step.DamageBefore, step.DamageAfter),
                    $"クリティカル {step.CriticalCountAfter} 回");
                Connect(previousNode, stepNode);
                previousNode = stepNode;
            }

            string resultTitle = record.IsCompleted ? "結果" : "未完了";
            Node resultNode = CreateNode(
                resultTitle,
                record.IsCompleted ? "lgd-node--result" : "lgd-node--incomplete",
                record.Steps.Count + 1,
                hasInput: true,
                $"最終ダメージ {FormatDamage(record.FinalDamage)}",
                $"クリティカル {FormatFlag(record.IsCritical)}",
                $"ステップ {record.Steps.Count} / {record.StepCount}");
            Connect(previousNode, resultNode);

            // レイアウトが決まってから全体が収まるように表示範囲を合わせる。
            schedule.Execute(() => FrameAll());
        }

        private const float NODE_WIDTH = 220f;
        private const float NODE_HEIGHT = 120f;
        private const float NODE_SPACING_X = 280f;
        private const float NODE_ORIGIN_Y = 80f;
        private const float MINIMAP_MARGIN = 8f;
        private const float MINIMAP_WIDTH = 180f;
        private const float MINIMAP_HEIGHT = 110f;
        private const string DAMAGE_FORMAT = "F1";
        private const string INPUT_PORT_NAME = "入力";
        private const string OUTPUT_PORT_NAME = "出力";

        /// <summary>
        ///     ノードを作り、指定した列に置いてグラフへ追加する。
        /// </summary>
        /// <param name="title"> ノードの見出し。 </param>
        /// <param name="stateClass"> 状態を表すUSSクラス。 </param>
        /// <param name="column"> 左から数えた列の位置。 </param>
        /// <param name="hasInput"> 入力ポートを付けるか。 </param>
        /// <param name="lines"> 本文に並べる行。 </param>
        /// <returns> 追加したノード。 </returns>
        private Node CreateNode(string title, string stateClass, int column, bool hasInput, params string[] lines)
        {
            Node node = new Node { title = title };
            node.AddToClassList("lgd-node");
            node.AddToClassList(stateClass);

            // 見るためのグラフなので、削除・複製はさせない。
            node.capabilities &= ~(Capabilities.Deletable | Capabilities.Copiable);

            if (hasInput)
            {
                Port input = node.InstantiatePort(
                    Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
                input.portName = INPUT_PORT_NAME;
                node.inputContainer.Add(input);
            }

            Port output = node.InstantiatePort(
                Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            output.portName = OUTPUT_PORT_NAME;
            node.outputContainer.Add(output);

            VisualElement body = new VisualElement();
            body.AddToClassList("lgd-node__body");
            foreach (string line in lines)
            {
                Label label = new Label(line);
                label.AddToClassList("lgd-node__line");
                body.Add(label);
            }

            node.extensionContainer.Add(body);
            node.expanded = true;
            node.RefreshExpandedState();
            node.RefreshPorts();
            node.SetPosition(new Rect(column * NODE_SPACING_X, NODE_ORIGIN_Y, NODE_WIDTH, NODE_HEIGHT));
            AddElement(node);
            return node;
        }

        /// <summary>
        ///     前のノードの出力から次のノードの入力へエッジを引く。
        /// </summary>
        /// <param name="from"> 出力側のノード。 </param>
        /// <param name="to"> 入力側のノード。 </param>
        private void Connect(Node from, Node to)
        {
            Port output = from.outputContainer.Q<Port>();
            Port input = to.inputContainer.Q<Port>();
            if (output == null || input == null) { return; }

            Edge edge = output.ConnectTo(input);
            edge.capabilities &= ~Capabilities.Deletable;
            AddElement(edge);
        }

        /// <summary>
        ///     ダメージを表示用の文字列にする。
        /// </summary>
        /// <param name="damage"> ダメージ。 </param>
        /// <returns> 表示用の文字列。 </returns>
        private static string FormatDamage(float damage)
        {
            return damage.ToString(DAMAGE_FORMAT);
        }

        /// <summary>
        ///     ステップ前後のダメージの倍率を表示用の文字列にする。
        /// </summary>
        /// <param name="before"> ステップ前のダメージ。 </param>
        /// <param name="after"> ステップ後のダメージ。 </param>
        /// <returns> 表示用の文字列。 </returns>
        private static string FormatRatio(float before, float after)
        {
            if (Mathf.Approximately(before, 0f)) { return "倍率 -"; }

            return $"倍率 ×{after / before:F2}";
        }

        /// <summary>
        ///     真偽値を表示用の文字列にする。
        /// </summary>
        /// <param name="value"> 真偽値。 </param>
        /// <returns> 表示用の文字列。 </returns>
        private static string FormatFlag(bool value)
        {
            return value ? "あり" : "なし";
        }
    }
}
