using KillChord.Runtime.Domain.InGame.Mission;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     会話情報を表示するPresenter。
    /// </summary>
    public sealed class MissionDialoguePresenter
    {
        public MissionDialoguePresenter(IMissionDialogueViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        }

        /// <summary>
        ///     会話文字と演出状態を反映する。
        /// </summary>
        public void Present(MissionDialogueLine line, bool isVisible, bool isPaused, int version, bool isImmediate = false)
        {
            MissionDialogueDTO dto = new(line.Text, line.Portrait, isVisible, isPaused, version, isImmediate);
            _viewModel.Apply(in dto);
        }

        private readonly IMissionDialogueViewModel _viewModel;
    }
}
