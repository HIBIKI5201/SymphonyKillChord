using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace KillChord.Runtime.View.InGame.Stage.Barrage
{
    /// <summary>
    ///     弾幕を発射させる区間を表すTimelineクリップです。
    /// </summary>
    public sealed class BarrageFireClip : PlayableAsset, ITimelineClipAsset
    {
        /// <summary> クリップがサポートする機能です。ブレンドやループは使用しません。 </summary>
        public ClipCaps clipCaps => ClipCaps.None;

        /// <summary>
        ///     クリップのPlayableを生成します。
        /// </summary>
        /// <param name="graph"> 生成先のPlayableGraphです。 </param>
        /// <param name="owner"> クリップを保持するGameObjectです。 </param>
        /// <returns> 生成したPlayableです。 </returns>
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<BarrageFireBehaviour>.Create(graph, _template);
        }

        [SerializeField, Tooltip("再生区間中に弾幕を命令する設定です。")]
        private BarrageFireBehaviour _template = new();
    }
}
