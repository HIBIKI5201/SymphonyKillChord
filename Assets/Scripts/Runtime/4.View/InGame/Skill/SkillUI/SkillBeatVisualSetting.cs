using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill
{
    /// <summary>
    ///     スキル入力進行UIの拍子ごとの表示設定。
    /// </summary>
    public class SkillBeatVisualSetting
    {
        /// <summary>
        ///     スキル入力進行UIの拍子ごとの表示設定を生成する。
        /// </summary>
        /// <param name="beatType"> 拍子の種類。 </param>
        /// <param name="normalColor"> 通常時の色。 </param>
        /// <param name="activeColor"> アクティブ時の色。 </param>
        /// <param name="icon"> 表示するアイコン。 </param>
        public SkillBeatVisualSetting(int beatType, Color normalColor, Color activeColor, Sprite icon)
        {
            BeatType = beatType;
            NormalColor = normalColor;
            ActiveColor = activeColor;
            Icon = icon;
        }

        /// <summary> 対応する拍子の種類。 </summary>
        public int BeatType { get; }

        /// <summary> 通常時の色。 </summary>
        public Color NormalColor {  get; }

        /// <summary> アクティブ時の色。 </summary>
        public Color ActiveColor { get; }

        /// <summary> 表示するアイコン。 </summary>
        public Sprite Icon { get; }
    }
}
