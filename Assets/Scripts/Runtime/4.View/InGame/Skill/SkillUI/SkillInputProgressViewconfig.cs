using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill
{
    /// <summary>
    ///     スキル入力進行UIの表示設定を管理するクラス。
    /// </summary>
    public class SkillInputProgressViewconfig
    {
        /// <summary>
        ///     スキル入力進行UIの表示設定を管理するクラスを生成する。
        /// </summary>
        /// <param name="settings"> スキル入力進行UIの拍子ごとの表示設定のリスト。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public SkillInputProgressViewconfig(IReadOnlyList<SkillBeatVisualSetting> settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>
        ///     指定されたBeatTypeに対応する表示設定を取得する。
        /// </summary>
        /// <param name="beatType"> 取得対象のBeatType。 </param>
        /// <returns> 指定されたBeatTypeに対応する表示設定。 </returns>
        /// <exception cref="InvalidOperationException"></exception>
        public SkillBeatVisualSetting GetSetting(int beatType)
        {
            for (int i = 0; i < _settings.Count; i++)
            {
                if (_settings[i].BeatType == beatType)
                {
                    return _settings[i];
                }
            }

            throw new InvalidOperationException($"BeatType {beatType} の表示設定がありません。");
        }

        private readonly IReadOnlyList<SkillBeatVisualSetting> _settings;
    }
}
