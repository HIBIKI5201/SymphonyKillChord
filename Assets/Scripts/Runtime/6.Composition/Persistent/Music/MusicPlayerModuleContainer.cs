using KillChord.Runtime.Adaptor.Persistent.Music;
using System;

namespace KillChord.Runtime.Composition.Persistent.Music
{
    /// <summary>
    ///     Persistent音楽再生モジュールの公開物を保持するContainer。
    /// </summary>
    public sealed class MusicPlayerModuleContainer
    {
        /// <summary>
        ///     音楽再生モジュールの公開物を初期化する。
        /// </summary>
        /// <param name="cuePlayer"> BGM Cueの切り替えポート。 </param>
        public MusicPlayerModuleContainer(IBgmCuePlayer cuePlayer)
        {
            CuePlayer = cuePlayer ?? throw new ArgumentNullException(nameof(cuePlayer));
        }

        /// <summary> BGM Cueの切り替えポート。 </summary>
        public IBgmCuePlayer CuePlayer { get; }
    }
}
