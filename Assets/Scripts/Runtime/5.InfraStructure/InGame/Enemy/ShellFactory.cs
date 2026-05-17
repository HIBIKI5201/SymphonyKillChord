using KillChord.Runtime.Domain.InGame.Enemy;

namespace KillChord.Runtime.InfraStructure.InGame.Enemy
{
    /// <summary>
    ///     ScriptableObjectから砲弾エンティティを生成する。
    /// </summary>
    public static class ShellFactory
    {
        /// <summary>
        ///     砲弾の攻撃情報を生成する。
        /// </summary>
        /// <param name="attackData"></param>
        /// <returns></returns>
        public static ShellAttackSpec CreateAttackSpec(ShellAttackData attackData)
        {
            return new ShellAttackSpec(attackData.ExplosionRadius);
        }
        /// <summary>
        ///     砲弾の音楽同期タイミング情報を生成する。
        /// </summary>
        /// <param name="musicData"></param>
        /// <returns></returns>
        public static EnemyMusicSpec CreateMusicSpec(EnemyMusicData musicData)
        {
            return new EnemyMusicSpec(musicData.BarFlag, musicData.TimeSignature, musicData.TargetBeat);
        }
    }
}
