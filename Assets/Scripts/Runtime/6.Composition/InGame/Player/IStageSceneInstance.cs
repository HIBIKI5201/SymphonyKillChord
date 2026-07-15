using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Player
{
    /// <summary>
    ///     ステージシーン参照の公開インターフェース。
    /// </summary>
    public interface IStageSceneInstance
    {
        /// <summary> プレイヤー生成位置のTransform。 </summary>
        Transform PlayerSpawnPointTransform { get; }
    }
}
