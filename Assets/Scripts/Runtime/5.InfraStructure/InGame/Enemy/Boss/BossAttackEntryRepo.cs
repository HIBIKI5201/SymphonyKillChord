using KillChord.Runtime.Utility.Constant;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Enemy
{
    /// <summary>
    ///     ボスの攻撃定義の集合。
    /// </summary>
    [CreateAssetMenu(fileName = "BossAttackEntryRepo", menuName = PathConst.CREATE_ASSET_MENU_PATH + "Enemy/" + nameof(BossAttackEntryRepo))]
    public class BossAttackEntryRepo : ScriptableObject
    {
        public BossAttackEntryDefinition[] AttackEntries => _attackEntries;
        [SerializeField] private BossAttackEntryDefinition[] _attackEntries;
    }
}
