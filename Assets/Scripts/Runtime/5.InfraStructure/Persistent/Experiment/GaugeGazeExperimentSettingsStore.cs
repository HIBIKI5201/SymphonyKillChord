using KillChord.Runtime.Domain.Persistent.Experiment;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.Persistent.Experiment
{
    /// <summary>
    ///     ゲージ注視の改善案の切り替えを、端末のPlayerPrefsに読み書きするクラスです。
    ///     試遊用の設定のため、セーブデータには含めません。
    /// </summary>
    public sealed class GaugeGazeExperimentSettingsStore
    {
        /// <summary>
        ///     保存した設定を読みます。保存していない場合は全ての案を有効にした設定を返します。
        /// </summary>
        /// <returns> 読んだ設定です。 </returns>
        public GaugeGazeExperimentSettings Load()
        {
            if (!PlayerPrefs.HasKey(PREFS_KEY))
            {
                return GaugeGazeExperimentSettings.CreateAllEnabled();
            }

            return new GaugeGazeExperimentSettings((GaugeGazeExperimentFeature)PlayerPrefs.GetInt(PREFS_KEY));
        }

        /// <summary>
        ///     設定を保存します。
        /// </summary>
        /// <param name="settings"> 保存する設定です。 </param>
        public void Save(GaugeGazeExperimentSettings settings)
        {
            PlayerPrefs.SetInt(PREFS_KEY, (int)settings.EnabledFeatures);
            PlayerPrefs.Save();
        }

        private const string PREFS_KEY = "KillChord.Experiment.GaugeGaze.EnabledFeatures";
    }
}
