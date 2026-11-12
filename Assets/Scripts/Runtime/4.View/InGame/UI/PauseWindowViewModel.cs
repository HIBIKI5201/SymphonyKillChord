using KillChord.Runtime.Adaptor.InGame.UI;
using R3;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.View.InGame.UI
{
    /// <summary>
    ///     ポーズ画面の文言とミッション欄の表示データを保持するViewModel。
    /// </summary>
    public sealed class PauseWindowViewModel : IPauseWindowViewModel, IDisposable
    {
        /// <summary> ミッション欄の内容が更新された時に発火するイベント。 </summary>
        public event Action OnMissionsUpdated;

        /// <summary> 「ゲームに戻る」ボタンの文言。 </summary>
        public ReadOnlyReactiveProperty<string> ResumeLabel => _resumeLabel;

        /// <summary> 「オプション」ボタンと設定パネルの見出しの文言。 </summary>
        public ReadOnlyReactiveProperty<string> OptionLabel => _optionLabel;

        /// <summary> 「ステージを再挑戦する」ボタンの文言。 </summary>
        public ReadOnlyReactiveProperty<string> RetryLabel => _retryLabel;

        /// <summary> 「ステージをあきらめる」ボタンの文言。 </summary>
        public ReadOnlyReactiveProperty<string> GiveUpLabel => _giveUpLabel;

        /// <summary> ミッション欄の見出し。 </summary>
        public ReadOnlyReactiveProperty<string> MissionLabel => _missionLabel;

        /// <summary> メインミッションの見出し。 </summary>
        public ReadOnlyReactiveProperty<string> MainMissionLabel => _mainMissionLabel;

        /// <summary> サブミッションの見出し。 </summary>
        public ReadOnlyReactiveProperty<string> SubMissionLabel => _subMissionLabel;

        /// <summary> BGM音量の項目名。 </summary>
        public ReadOnlyReactiveProperty<string> BgmVolumeLabel => _bgmVolumeLabel;

        /// <summary> SE音量の項目名。 </summary>
        public ReadOnlyReactiveProperty<string> SoundEffectVolumeLabel => _soundEffectVolumeLabel;

        /// <summary> ボイス音量の項目名。 </summary>
        public ReadOnlyReactiveProperty<string> VoiceVolumeLabel => _voiceVolumeLabel;

        /// <summary> 判定調整の項目名。 </summary>
        public ReadOnlyReactiveProperty<string> RhythmOffsetLabel => _rhythmOffsetLabel;

        /// <summary> 設定パネルを閉じるボタンの文言。 </summary>
        public ReadOnlyReactiveProperty<string> CloseLabel => _closeLabel;

        /// <summary> 表示するミッションがある場合はtrue。 </summary>
        public bool HasMission => _hasMission;

        /// <summary> メインミッション。 </summary>
        public PauseMissionRowViewModel MainMission => _mainMission;

        /// <summary> サブミッションの一覧。 </summary>
        public IReadOnlyList<PauseMissionRowViewModel> SubMissions => _subMissions;

        /// <summary> サブミッションが無いときに出す文。 </summary>
        public string EmptySubMissionText => _emptySubMissionText;

        /// <summary>
        ///     保持している通知ストリームを破棄する。
        /// </summary>
        public void Dispose()
        {
            _resumeLabel.Dispose();
            _optionLabel.Dispose();
            _retryLabel.Dispose();
            _giveUpLabel.Dispose();
            _missionLabel.Dispose();
            _mainMissionLabel.Dispose();
            _subMissionLabel.Dispose();
            _bgmVolumeLabel.Dispose();
            _soundEffectVolumeLabel.Dispose();
            _voiceVolumeLabel.Dispose();
            _rhythmOffsetLabel.Dispose();
            _closeLabel.Dispose();
        }

        /// <inheritdoc/>
        public void ApplyLabels(in PauseWindowLabelDTO dto)
        {
            _resumeLabel.Value = dto.Resume;
            _optionLabel.Value = dto.Option;
            _retryLabel.Value = dto.Retry;
            _giveUpLabel.Value = dto.GiveUp;
            _missionLabel.Value = dto.Mission;
            _mainMissionLabel.Value = dto.MainMission;
            _subMissionLabel.Value = dto.SubMission;
            _bgmVolumeLabel.Value = dto.BgmVolume;
            _soundEffectVolumeLabel.Value = dto.SoundEffectVolume;
            _voiceVolumeLabel.Value = dto.VoiceVolume;
            _rhythmOffsetLabel.Value = dto.RhythmOffset;
            _closeLabel.Value = dto.Close;
        }

        /// <inheritdoc/>
        public void ApplyMissions(in PauseMissionDTO dto)
        {
            _hasMission = dto.HasMission;
            _mainMission = new PauseMissionRowViewModel(
                dto.MainMission.Description,
                dto.MainMission.IsAchieved,
                dto.MainMission.StatusText);
            _emptySubMissionText = dto.EmptySubMissionText;

            _subMissions.Clear();
            ReadOnlySpan<PauseMissionItemDTO> subMissions = dto.SubMissions;
            for (int i = 0; i < subMissions.Length; i++)
            {
                _subMissions.Add(new PauseMissionRowViewModel(
                    subMissions[i].Description,
                    subMissions[i].IsAchieved,
                    subMissions[i].StatusText));
            }

            OnMissionsUpdated?.Invoke();
        }

        private readonly ReactiveProperty<string> _resumeLabel = new(string.Empty);
        private readonly ReactiveProperty<string> _optionLabel = new(string.Empty);
        private readonly ReactiveProperty<string> _retryLabel = new(string.Empty);
        private readonly ReactiveProperty<string> _giveUpLabel = new(string.Empty);
        private readonly ReactiveProperty<string> _missionLabel = new(string.Empty);
        private readonly ReactiveProperty<string> _mainMissionLabel = new(string.Empty);
        private readonly ReactiveProperty<string> _subMissionLabel = new(string.Empty);
        private readonly ReactiveProperty<string> _bgmVolumeLabel = new(string.Empty);
        private readonly ReactiveProperty<string> _soundEffectVolumeLabel = new(string.Empty);
        private readonly ReactiveProperty<string> _voiceVolumeLabel = new(string.Empty);
        private readonly ReactiveProperty<string> _rhythmOffsetLabel = new(string.Empty);
        private readonly ReactiveProperty<string> _closeLabel = new(string.Empty);
        private readonly List<PauseMissionRowViewModel> _subMissions = new();
        private bool _hasMission;
        private PauseMissionRowViewModel _mainMission;
        private string _emptySubMissionText = string.Empty;
    }
}
