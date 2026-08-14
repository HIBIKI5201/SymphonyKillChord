using Unity.Entities;

namespace KillChord.Runtime.View.InGame.Stage.Barrage
{
    /// <summary>
    ///     弾の寿命です。
    /// </summary>
    public struct BulletLifetime : IComponentData
    {
        /// <summary> 消滅するまでの秒数です。 </summary>
        public float DurationSeconds;

        /// <summary> 発射からの経過秒数です。 </summary>
        public float ElapsedSeconds;
    }
}
