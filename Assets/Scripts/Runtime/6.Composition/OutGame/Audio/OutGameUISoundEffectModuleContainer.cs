using KillChord.Runtime.Adaptor.OutGame.Audio;
using System;

namespace KillChord.Runtime.Composition.OutGame.Audio
{
    /// <summary>
    ///     OutGame UI操作音モジュールの公開物を保持するContainer。
    /// </summary>
    public sealed class OutGameUISoundEffectModuleContainer
    {
        /// <summary>
        ///     UI操作音モジュールの公開物を初期化する。
        /// </summary>
        /// <param name="command"> UI操作音の再生コマンド。 </param>
        public OutGameUISoundEffectModuleContainer(IUISoundEffectCommand command)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
        }

        /// <summary> UI操作音の再生コマンド。 </summary>
        public IUISoundEffectCommand Command { get; }
    }
}
