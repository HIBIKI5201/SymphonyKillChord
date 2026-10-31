using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using KillChord.Runtime.InfraStructure.InGame.Enemy;
using System;
using System.Collections.Generic;

namespace KillChord.DebugWorld.Battle
{
    /// <summary>
    ///     裏世界の戦闘で使う出撃先のステージと、選べる敵の一覧をまとめるクラスです。
    /// </summary>
    public sealed class DebugBattleCatalog
    {
        /// <summary>
        ///     ステージツリーと敵定義から、出撃先と敵の一覧を作ります。
        /// </summary>
        /// <param name="stageTree"> ステージツリーです。 </param>
        /// <param name="enemyDefinitionRepository"> 個別の敵定義リポジトリです。 </param>
        /// <param name="battleSceneName"> 出撃先にする戦闘シーン名です。 </param>
        public DebugBattleCatalog(
            StageTree stageTree,
            EnemyDefinitionRepository enemyDefinitionRepository,
            string battleSceneName)
        {
            if (stageTree == null) { throw new ArgumentNullException(nameof(stageTree)); }

            if (enemyDefinitionRepository == null)
            {
                throw new ArgumentNullException(nameof(enemyDefinitionRepository));
            }

            _baseStage = FindBaseStage(stageTree, battleSceneName);
            IReadOnlyList<EnemyDefinitionAsset> definitions = enemyDefinitionRepository.Definitions;
            for (int i = 0; i < definitions.Count; i++)
            {
                EnemyDefinitionAsset definition = definitions[i];
                if (definition == null || definition.Id.Value == 0 || definition.ViewPrefab == null)
                {
                    continue;
                }

                _enemyIds.Add(definition.Id);
                _enemyNames.Add($"{definition.name}（{definition.EnemyType}）");
            }
        }

        /// <summary> 出撃先のステージが見つかった場合はtrueです。 </summary>
        public bool HasBaseStage => _baseStage != null;

        /// <summary> 出撃先のステージ定義です。 </summary>
        public BattleStageDefinition BaseStage => _baseStage;

        /// <summary> 選べる敵の表示名の一覧です。 </summary>
        public IReadOnlyList<string> EnemyNames => _enemyNames;

        /// <summary>
        ///     一覧の位置から敵定義IDを取得します。
        /// </summary>
        /// <param name="index"> 一覧の位置です。 </param>
        /// <param name="enemyDefinitionId"> 取得した敵定義IDです。 </param>
        /// <returns> 位置が範囲内の場合はtrueです。 </returns>
        public bool TryGetEnemyId(int index, out EnemyDefinitionId enemyDefinitionId)
        {
            if (index < 0 || index >= _enemyIds.Count)
            {
                enemyDefinitionId = default;
                return false;
            }

            enemyDefinitionId = _enemyIds[index];
            return true;
        }

        private readonly BattleStageDefinition _baseStage;
        private readonly List<EnemyDefinitionId> _enemyIds = new();
        private readonly List<string> _enemyNames = new();

        /// <summary>
        ///     チュートリアル以外で、指定した戦闘シーンを使うバトルステージを探します。
        ///     <para> 見つからない場合は、チュートリアル以外の最初のバトルステージを使います。 </para>
        /// </summary>
        /// <param name="stageTree"> ステージツリーです。 </param>
        /// <param name="battleSceneName"> 優先する戦闘シーン名です。 </param>
        /// <returns> 見つかったステージ定義です。無い場合はnullです。 </returns>
        private static BattleStageDefinition FindBaseStage(StageTree stageTree, string battleSceneName)
        {
            BattleStageDefinition fallback = null;
            IReadOnlyList<StageNode> nodes = stageTree.Nodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Definition is not BattleStageDefinition battleStage || battleStage.IsTutorial)
                {
                    continue;
                }

                if (string.Equals(battleStage.BattleSceneName, battleSceneName, StringComparison.Ordinal))
                {
                    return battleStage;
                }

                fallback ??= battleStage;
            }

            return fallback;
        }
    }
}
