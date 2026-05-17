using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill
{
    /// <summary>
    ///     スキル入力進行UIの拍子ごとの表示データ。
    /// </summary>
    public readonly struct SkillInputProgressStepData
    {
        /// <summary>
        ///     スキル入力進行UIの拍子ごとの表示データを生成する。
        /// </summary>
        /// <param name="beatType"> 対応する拍子の種類。 </param>
        /// <param name="isActive"> アクティブ状態かどうか。 </param>
        /// <param name="color"> 表示する色。 </param>
        /// <param name="icon"> 表示するアイコン。 </param>
        public SkillInputProgressStepData(int beatType, bool isActive, Color color, Sprite icon)
        {
            BeatType = beatType;
            IsActive = isActive;
            Color = color;
            Icon = icon;
        }

        /// <summary> 対応する拍子の種類。 </summary>
        public int BeatType { get; }

        /// <summary> アクティブ状態かどうか。 </summary>
        public bool IsActive { get; }

        /// <summary> 表示する色。 </summary>
        public Color Color { get; }

        /// <summary> 表示するアイコン。 </summary>
        public Sprite Icon { get; }
    }
}
