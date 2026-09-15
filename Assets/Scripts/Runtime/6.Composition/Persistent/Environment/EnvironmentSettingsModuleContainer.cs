using KillChord.Runtime.Adaptor.Persistent.Environment;
using System;

namespace KillChord.Runtime.Composition.Persistent.Environment
{
    /// <summary>
    ///     環境設定モジュールの公開物を保持するContainer。
    /// </summary>
    public sealed class EnvironmentSettingsModuleContainer
    {
        /// <summary>
        ///     環境設定モジュールの公開物を初期化する。
        /// </summary>
        public EnvironmentSettingsModuleContainer(
            IEnvironmentSettingsViewModel viewModel,
            IEnvironmentSettingsCommand command)
        {
            ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            Command = command ?? throw new ArgumentNullException(nameof(command));
        }

        /// <summary> 環境設定の表示状態。 </summary>
        public IEnvironmentSettingsViewModel ViewModel { get; }

        /// <summary> 環境設定の変更コマンド。 </summary>
        public IEnvironmentSettingsCommand Command { get; }
    }
}
