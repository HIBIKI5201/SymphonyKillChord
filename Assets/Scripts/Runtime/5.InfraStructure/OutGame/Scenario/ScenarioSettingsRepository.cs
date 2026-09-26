using System;
using KillChord.Runtime.Application.OutGame.Scenario;

namespace KillChord.Runtime.InfraStructure.OutGame.Scenario
{
    /// <summary>
    /// シナリオ再生設定を設定アセットから取得する。
    /// </summary>
    public class ScenarioSettingsRepository : IScenarioSettingsRepository
    {
        /// <summary>
        /// 設定アセットから参照する再生設定の取得元を初期化する。
        /// </summary>
        public ScenarioSettingsRepository(ScenarioSettingsAsset asset)
        {
            _asset = asset;
        }

        /// <summary> 通常時の1文字あたりの表示間隔。 </summary>
        public TimeSpan NormalTextCharInterval => ToTimeSpan(_asset != null ? _asset.NormalTextCharIntervalSec : 0.2f);
        /// <summary> 早送り時の1文字あたりの表示間隔。 </summary>
        public TimeSpan FastForwardTextCharInterval => ToTimeSpan(_asset != null ? _asset.FastForwardTextCharIntervalSec : 0.02f);
        /// <summary> 一時停止中に再開を確認する間隔。 </summary>
        public TimeSpan PausePollInterval => ToTimeSpan(_asset != null ? _asset.PausePollIntervalSec : 0.05f);
        /// <summary> 再生完了後に画面を閉じるまでの待ち時間。 </summary>
        public TimeSpan CloseDelayAfterComplete => ToTimeSpan(_asset != null ? _asset.CloseDelayAfterCompleteSec : 3f);
        /// <summary> オート送りで次の行へ進むまでの待ち時間。 </summary>
        public TimeSpan AutoAdvanceDelay => ToTimeSpan(_asset != null ? _asset.AutoAdvanceDelaySec : 2f);
        /// <summary> SkipClosesImmediately を取得する。 </summary>
        public bool SkipClosesImmediately => _asset == null || _asset.SkipClosesImmediately;
        /// <summary> WaitForInputOnLastText を取得する。 </summary>
        public bool WaitForInputOnLastText => _asset != null && _asset.WaitForInputOnLastText;

        /// <summary>
        /// 秒指定を TimeSpan へ変換する。
        /// </summary>
        private static TimeSpan ToTimeSpan(float seconds)
        {
            return TimeSpan.FromSeconds(Math.Max(0f, seconds));
        }

        private readonly ScenarioSettingsAsset _asset;
    }
}