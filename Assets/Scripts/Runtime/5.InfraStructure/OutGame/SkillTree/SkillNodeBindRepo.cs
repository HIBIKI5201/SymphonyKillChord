using KillChord.Runtime.Domain.OutGame.SkillTree;
using KillChord.Runtime.InfraStructure.Repository;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.SkillTree
{
    /// <summary>
    ///     スキルノードに対応するデータを纏めたリポジトリー。
    /// </summary>
    [CreateAssetMenu(fileName = "SkillNodeBindRepo", menuName = "SymphonyDev/SkillTree/SkillNodeBindRepo")]
    public class SkillNodeBindRepo : ScriptableObjectRepositoryBase<SkillNodeId, SkillNodeBindData, SkillNodeBindData>
    {
        public SkillNodeBindData[] SkillNodeBinds;

        /// <summary>
        ///     スキルノードのIDで対応するスキルノードデータを取得する。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SkillNodeBindData FindById(SkillNodeId id)
        {
            return TryFind(id, out SkillNodeBindData bind) ? bind : null;
        }

        /// <summary>
        ///     名前が一致するスキルノードの対応データを取得する。見つからない場合は null を返す。
        ///     呼び出し元はUIの要素名しか持たないため、名前の辞書を作って引く。
        /// </summary>
        public SkillNodeBindData FindByName(string name)
        {
#if UNITY_EDITOR
            // エディタではアセットの編集を反映するため、毎回作り直す。
            _nameMap = null;
#endif
            if (name == null)
            {
                return null;
            }

            EnsureNameMap();
            return _nameMap.TryGetValue(name, out SkillNodeBindData bind) ? bind : null;
        }

        /// <inheritdoc/>
        protected override IReadOnlyList<SkillNodeBindData> GetEntries() => SkillNodeBinds;

        private Dictionary<string, SkillNodeBindData> _nameMap;

        /// <summary>
        ///     名前からスキルノードの対応データを引く辞書を作る。名前が重複する場合は先頭を使う。
        /// </summary>
        private void EnsureNameMap()
        {
            if (_nameMap != null)
            {
                return;
            }

            _nameMap = new Dictionary<string, SkillNodeBindData>();
            if (SkillNodeBinds == null)
            {
                return;
            }

            for (int i = 0; i < SkillNodeBinds.Length; i++)
            {
                SkillNodeBindData bind = SkillNodeBinds[i];
                if (bind == null || bind.NodeName == null || _nameMap.ContainsKey(bind.NodeName))
                {
                    continue;
                }

                _nameMap.Add(bind.NodeName, bind);
            }
        }

        /// <inheritdoc/>
        protected override bool TryBuild(SkillNodeBindData entry, out SkillNodeId id, out SkillNodeBindData value)
        {
            value = entry;
            id = entry.SkillNodeId;
            return true;
        }
    }
}
