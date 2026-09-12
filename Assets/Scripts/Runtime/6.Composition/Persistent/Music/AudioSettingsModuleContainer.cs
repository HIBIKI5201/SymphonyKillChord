using KillChord.Runtime.Adaptor.Persistent.Music;
using System;

namespace KillChord.Runtime.Composition.Persistent.Music
{
    /// <summary>
    ///     音量設定モジュールの公開物を保持するContainer。
    /// </summary>
    public sealed class AudioSettingsModuleContainer
    {
        /// <summary>
        ///     音量設定モジュールの公開物を初期化する。
        /// </summary>
        public AudioSettingsModuleContainer(
            IAudioSettingsViewModel viewModel,
            IAudioSettingsCommand command)
        {
            ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            Command = command ?? throw new ArgumentNullException(nameof(command));
        }

        /// <summary> 音量設定の表示状態。 </summary>
        public IAudioSettingsViewModel ViewModel { get; }

        /// <summary> 音量設定の変更コマンド。 </summary>
        public IAudioSettingsCommand Command { get; }
    }
}
