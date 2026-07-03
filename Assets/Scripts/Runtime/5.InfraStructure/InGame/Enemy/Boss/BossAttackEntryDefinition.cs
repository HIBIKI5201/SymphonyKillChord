using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.Utility.Constant;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Enemy
{
    /// <summary>
    ///     ボスの攻撃を定義するクラス。
    /// </summary>
    [CreateAssetMenu(fileName = "BossAttackEntryDefinition", menuName = PathConst.CREATE_ASSET_MENU_PATH + "Enemy/" + nameof(BossAttackEntryDefinition))]
    public class BossAttackEntryDefinition : ScriptableObject
    {
        public BossAttackKind Kind => _kind;
        public int AttackIndex => _attackIndex;
        public EnemyMusicData MusicData => _musicData;

        [SerializeField, Tooltip("ボスの攻撃種別")] private BossAttackKind _kind;
        [SerializeField, Tooltip("攻撃定義インデックス")] private int _attackIndex;
        [SerializeField, Tooltip("攻撃の音楽同期情報")] private EnemyMusicData _musicData;
    }
}
