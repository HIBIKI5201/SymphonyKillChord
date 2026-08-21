using KillChord.Runtime.Domain.InGame.Mission.StepEntryAction;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     ボイスを再生する目標ステップ進入時アクションのアセットです。
    /// </summary>
    public class PlayVoiceStepEntryActionAsset : MissionStepEntryActionAssetBase
    {
        /// <inheritdoc />
        public override IMissionStepEntryAction Create()
        {
            return new PlayVoiceStepEntryAction(_voiceCueName);
        }

        /// <inheritdoc />
        protected override string BuildSummary()
        {
            return  $"ボイスを再生する：{_voiceCueName}";
        }

        [SerializeField, Tooltip("再生するボイスのCueName")]
        private string _voiceCueName;
    }
}
