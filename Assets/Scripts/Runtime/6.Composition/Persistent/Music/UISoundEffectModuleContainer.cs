using KillChord.Runtime.Adaptor.Persistent.Music;
using System;

namespace KillChord.Runtime.Composition.Persistent.Music
{
    /// <summary>
    ///     Persistent UI操作音モジュールの公開物を保持するContainer。
    /// </summary>
    public sealed class UISoundEffectModuleContainer
    {
        /// <summary>
        ///     UI操作音モジュールの公開物を初期化する。
        /// </summary>
        /// <param name="player"> UI操作音の再生ポート。 </param>
        public UISoundEffectModuleContainer(IPlayableAudioSource player)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
        }

        /// <summary> UI操作音の再生ポート。 </summary>
        public IPlayableAudioSource Player { get; }
    }
}
