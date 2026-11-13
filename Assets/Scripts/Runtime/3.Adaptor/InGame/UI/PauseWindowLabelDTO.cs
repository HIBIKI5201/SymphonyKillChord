namespace KillChord.Runtime.Adaptor.InGame.UI
{
    /// <summary>
    ///     ポーズ画面に出す決まった文言を、選択中の言語で受け渡すDTO。
    /// </summary>
    public readonly ref struct PauseWindowLabelDTO
    {
        /// <summary>
        ///     ポーズ画面の文言を指定して生成する。
        /// </summary>
        /// <param name="resume"> 「ゲームに戻る」ボタンの文言。 </param>
        /// <param name="option"> 「オプション」ボタンと設定パネルの見出しの文言。 </param>
        /// <param name="retry"> 「ステージを再挑戦する」ボタンの文言。 </param>
        /// <param name="giveUp"> 「ステージをあきらめる」ボタンの文言。 </param>
        /// <param name="mission"> ミッション欄の見出し。 </param>
        /// <param name="mainMission"> メインミッションの見出し。 </param>
        /// <param name="subMission"> サブミッションの見出し。 </param>
        /// <param name="bgmVolume"> BGM音量の項目名。 </param>
        /// <param name="soundEffectVolume"> SE音量の項目名。 </param>
        /// <param name="voiceVolume"> ボイス音量の項目名。 </param>
        /// <param name="rhythmOffset"> 判定調整の項目名。 </param>
        /// <param name="close"> 設定パネルを閉じるボタンの文言。 </param>
        public PauseWindowLabelDTO(
            string resume,
            string option,
            string retry,
            string giveUp,
            string mission,
            string mainMission,
            string subMission,
            string bgmVolume,
            string soundEffectVolume,
            string voiceVolume,
            string rhythmOffset,
            string close)
        {
            Resume = resume;
            Option = option;
            Retry = retry;
            GiveUp = giveUp;
            Mission = mission;
            MainMission = mainMission;
            SubMission = subMission;
            BgmVolume = bgmVolume;
            SoundEffectVolume = soundEffectVolume;
            VoiceVolume = voiceVolume;
            RhythmOffset = rhythmOffset;
            Close = close;
        }

        /// <summary> 「ゲームに戻る」ボタンの文言。 </summary>
        public string Resume { get; }

        /// <summary> 「オプション」ボタンと設定パネルの見出しの文言。 </summary>
        public string Option { get; }

        /// <summary> 「ステージを再挑戦する」ボタンの文言。 </summary>
        public string Retry { get; }

        /// <summary> 「ステージをあきらめる」ボタンの文言。 </summary>
        public string GiveUp { get; }

        /// <summary> ミッション欄の見出し。 </summary>
        public string Mission { get; }

        /// <summary> メインミッションの見出し。 </summary>
        public string MainMission { get; }

        /// <summary> サブミッションの見出し。 </summary>
        public string SubMission { get; }

        /// <summary> BGM音量の項目名。 </summary>
        public string BgmVolume { get; }

        /// <summary> SE音量の項目名。 </summary>
        public string SoundEffectVolume { get; }

        /// <summary> ボイス音量の項目名。 </summary>
        public string VoiceVolume { get; }

        /// <summary> 判定調整の項目名。 </summary>
        public string RhythmOffset { get; }

        /// <summary> 設定パネルを閉じるボタンの文言。 </summary>
        public string Close { get; }
    }
}
