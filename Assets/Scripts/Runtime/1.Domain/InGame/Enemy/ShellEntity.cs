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

        public ShellAttackSpec AttackSpec => _attackSpec;
        public EnemyMusicSpec MusicSpec => _musicSpec;
        public AttackDefinition AttackDefinition => _attackDefinition;

        private ShellAttackSpec _attackSpec;
        private EnemyMusicSpec _musicSpec;
        private AttackDefinition _attackDefinition;
    }
}
