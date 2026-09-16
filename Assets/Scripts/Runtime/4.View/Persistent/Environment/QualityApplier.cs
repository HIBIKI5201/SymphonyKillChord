using KillChord.Runtime.Adaptor.Persistent.Environment;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.View.Persistent.Environment
{
    /// <summary>
    ///     画質プリセットをデバイスへ適用するクラス。
    ///     開発用プリセットはプレイヤーへ表示しない。
    /// </summary>
    public sealed class QualityApplier : IQualityApplier
    {
        /// <summary>
        ///     選択可能な画質プリセットの一覧を取得する。
        /// </summary>
        public IReadOnlyList<QualityLevelOption> GetAvailableQualityLevels()
        {
            if (_availableQualityLevels != null)
            {
                return _availableQualityLevels;
            }

            string[] qualityLevelNames = QualitySettings.names;
            List<QualityLevelOption> options = new(qualityLevelNames.Length);

            for (int i = 0; i < qualityLevelNames.Length; i++)
            {
                if (qualityLevelNames[i].Contains(DEVELOP_QUALITY_LEVEL_NAME_KEYWORD))
                {
                    continue;
                }

                options.Add(new QualityLevelOption(i, qualityLevelNames[i]));
            }

            _availableQualityLevels = options;
            return _availableQualityLevels;
        }

        /// <summary>
        ///     画質プリセットを適用する。
        /// </summary>
        public void Apply(int qualityLevelIndex)
        {
            QualitySettings.SetQualityLevel(qualityLevelIndex, true);
        }

        /// <summary>
        ///     指定した画質プリセットの表示名を取得する。
        /// </summary>
        public string GetQualityLevelName(int qualityLevelIndex)
        {
            string[] qualityLevelNames = QualitySettings.names;
            if (qualityLevelIndex < 0 || qualityLevelIndex >= qualityLevelNames.Length)
            {
                return string.Empty;
            }

            return qualityLevelNames[qualityLevelIndex];
        }

        private const string DEVELOP_QUALITY_LEVEL_NAME_KEYWORD = "Develop";

        private List<QualityLevelOption> _availableQualityLevels;
    }
}
