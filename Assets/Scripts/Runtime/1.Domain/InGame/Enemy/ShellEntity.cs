using KillChord.Runtime.Domain.InGame.Battle;

namespace KillChord.Runtime.Domain.InGame.Enemy
{
    /// <summary>
    ///     砲弾のエンティティ。
    /// </summary>
    public class ShellEntity
    {
        public ShellEntity(ShellAttackSpec attackSpec, EnemyMusicSpec musicSpec, AttackDefinition attackDefinition)
        {
            _attackSpec = attackSpec;
            _musicSpec = musicSpec;
            _attackDefinition = attackDefinition;
        }

        /// <summary> 砲弾固有の攻撃能力 </summary>
        public ShellAttackSpec AttackSpec => _attackSpec;
        /// <summary> 音楽同期タイミング情報 </summary>
        public EnemyMusicSpec MusicSpec => _musicSpec;
        /// <summary> 攻撃情報 </summary>
        public AttackDefinition AttackDefinition => _attackDefinition;

        private ShellAttackSpec _attackSpec;
        private EnemyMusicSpec _musicSpec;
        private AttackDefinition _attackDefinition;
    }
}
