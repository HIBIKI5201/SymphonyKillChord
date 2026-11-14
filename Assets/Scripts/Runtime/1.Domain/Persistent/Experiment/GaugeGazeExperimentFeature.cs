using System;

namespace KillChord.Runtime.Domain.Persistent.Experiment
{
    /// <summary>
    ///     「戦闘中にゲージばかり見る問題」の改善案のうち、試遊で切り替えられる案の一覧です。
    ///     値はビットの位置で、保存した設定と対応するため、既存の値を変えないでください。
    /// </summary>
    [Flags]
    public enum GaugeGazeExperimentFeature
    {
        None = 0,

        /// <summary> スキル入力のJustの割合でスキルの効果を強くし、キルコードのJust倍率をなくす。 </summary>
        SkillJustBonus = 1 << 0,

        /// <summary> Justのときだけコンボを増やし、Justを外すとコンボを終える。 </summary>
        JustOnlyCombo = 1 << 1,

        /// <summary> スキルゲージをリズムゲージの上に置き、リズムゲージより目立たせる。 </summary>
        SkillGaugeEmphasis = 1 << 2,

        /// <summary> リズムゲージの波（ブロックの伸び縮み）を小さくする。 </summary>
        GaugeWaveReduction = 1 << 3,

        /// <summary> 移動速度・回避のクールタイム・攻撃とスキルの硬直を、回避を中心にした値にする。 </summary>
        MovementTuning = 1 << 4,

        /// <summary> リズムゲージの長さを1小節にし、伸びきったあと中央から消えてタイムアウトする。 </summary>
        OneBarGauge = 1 << 5,
    }
}
