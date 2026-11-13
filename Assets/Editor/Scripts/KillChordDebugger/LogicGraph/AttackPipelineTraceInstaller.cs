using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.InfraStructure.InGame.Battle;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.Debugger.LogicGraph
{
    /// <summary>
    ///     Play中だけ、攻撃パイプラインのステップを記録用のステップで包み、終わったら元に戻す。
    ///     <para> ランタイムのコードには手を入れず、リフレクションでアセットの配列を書き換える。 </para>
    ///     <para>
    ///         <see cref="AttackPipelineAsset"/> は自分の配列をそのまま <see cref="AttackPipeline"/> に渡すため、
    ///         配列の要素を差し替えると、生成済みのパイプラインにも反映される。
    ///         コードで配列を作るパイプライン（伝染ダメージなど）は対象外。
    ///     </para>
    /// </summary>
    public sealed class AttackPipelineTraceInstaller
    {
        /// <summary>
        ///     記録係を指定して初期化する。
        /// </summary>
        /// <param name="recorder"> 包んだステップが記録を渡す先。 </param>
        public AttackPipelineTraceInstaller(AttackPipelineTraceRecorder recorder)
        {
            _recorder = recorder;
        }

        /// <summary> ステップを包んでいる最中か。 </summary>
        public bool IsInstalled => _installed.Count > 0;

        /// <summary> 包んだパイプラインアセットの数。 </summary>
        public int InstalledAssetCount => _installed.Count;

        /// <summary>
        ///     プロジェクト内のすべての攻撃パイプラインアセットのステップを包む。
        ///     すでに包んでいるアセットは飛ばす。
        /// </summary>
        public void Install()
        {
            FieldInfo stepsField = GetStepsField();
            if (stepsField == null) { return; }

            string[] guids = AssetDatabase.FindAssets(ASSET_FILTER);
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AttackPipelineAsset asset = AssetDatabase.LoadAssetAtPath<AttackPipelineAsset>(path);
                if (asset == null || _installed.ContainsKey(asset)) { continue; }

                if (stepsField.GetValue(asset) is not IAttackStep[] steps || steps.Length == 0) { continue; }

                // 元の要素を控えてから、同じ配列の中身を包んだステップに置き換える。
                IAttackStep[] originals = (IAttackStep[])steps.Clone();
                bool hasWrapped = false;
                for (int i = 0; i < steps.Length; i++)
                {
                    if (steps[i] == null || steps[i] is TracingAttackStep) { continue; }

                    steps[i] = new TracingAttackStep(steps[i], i, steps.Length, _recorder);
                    hasWrapped = true;
                }

                if (hasWrapped)
                {
                    _installed.Add(asset, new InstalledSteps(steps, originals));
                }
            }
        }

        /// <summary>
        ///     包んだステップをすべて元に戻す。何度呼んでもよい。
        /// </summary>
        public void Uninstall()
        {
            foreach (InstalledSteps installed in _installed.Values)
            {
                installed.Restore();
            }

            _installed.Clear();
        }

        private const string ASSET_FILTER = "t:" + nameof(AttackPipelineAsset);
        private const string STEPS_FIELD_NAME = "_attackSteps";

        private readonly AttackPipelineTraceRecorder _recorder;
        private readonly Dictionary<AttackPipelineAsset, InstalledSteps> _installed = new();

        /// <summary>
        ///     アセットのステップ配列のフィールドを取得する。見つからなければ警告を出す。
        /// </summary>
        /// <returns> フィールド情報。見つからない場合はnull。 </returns>
        private static FieldInfo GetStepsField()
        {
            FieldInfo field = typeof(AttackPipelineAsset).GetField(
                STEPS_FIELD_NAME,
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null || field.FieldType != typeof(IAttackStep[]))
            {
                Debug.LogWarning(
                    $"[{nameof(AttackPipelineTraceInstaller)}] {nameof(AttackPipelineAsset)}.{STEPS_FIELD_NAME} が見つかりません。記録できません。");
                return null;
            }

            return field;
        }

        /// <summary>
        ///     包んだ配列と、元に戻すための元の要素の組。
        /// </summary>
        private sealed class InstalledSteps
        {
            /// <summary>
            ///     包んだ配列と元の要素を指定して初期化する。
            /// </summary>
            /// <param name="steps"> 包んだステップを入れた配列（アセットと同じ配列）。 </param>
            /// <param name="originals"> 包む前の要素の写し。 </param>
            public InstalledSteps(IAttackStep[] steps, IAttackStep[] originals)
            {
                _steps = steps;
                _originals = originals;
            }

            /// <summary>
            ///     配列の中身を包む前の要素に戻す。
            /// </summary>
            public void Restore()
            {
                int count = Mathf.Min(_steps.Length, _originals.Length);
                for (int i = 0; i < count; i++)
                {
                    _steps[i] = _originals[i];
                }
            }

            private readonly IAttackStep[] _steps;
            private readonly IAttackStep[] _originals;
        }
    }
}
