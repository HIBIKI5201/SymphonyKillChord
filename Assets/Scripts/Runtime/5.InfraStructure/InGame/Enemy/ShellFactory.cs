using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Music;

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
        public static ShellAttackSpec CreateAttackSpec(ShellAttackSpecAsset attackData)
        {
            return new ShellAttackSpec(attackData.ExplosionRadius);
        }

        /// <summary>
        ///     砲弾の音楽同期タイミング情報を生成する。
        /// </summary>
        /// <param name="musicData"></param>
        /// <returns></returns>
        public static MusicSyncSpec CreateMusicSpec(EnemyMusicSpecAsset musicData)
        {
            return new MusicSyncSpec(musicData.BarFlag, musicData.TimeSignature, musicData.TargetBeat);
        }
    }
}
