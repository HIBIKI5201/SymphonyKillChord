using Unity.Entities;

namespace KillChord.Runtime.View.InGame.Stage.Barrage
{
    /// <summary>
    ///     Timelineから発行される単発の弾幕コマンドです。
    /// </summary>
    /// <remarks> 専用Entityとして生成され、ルーティング後に破棄されます。 </remarks>
    public struct BarrageFireCommand : IComponentData
    {
        /// <summary> コマンドの宛先となるタレットの数値IDです。 </summary>
        public int TargetTurretId;

        /// <summary> 開始と停止のどちらを指示するかです。 </summary>
        public BarrageCommandKind Kind;
    }
}
