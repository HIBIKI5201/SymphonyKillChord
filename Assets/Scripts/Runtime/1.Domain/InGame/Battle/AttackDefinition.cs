using KillChord.Runtime.Domain.InGame.Music;

namespace KillChord.Runtime.Domain.InGame.Battle
{
    /// <summary>
    ///     攻撃の基本情報を保持するクラス。
    ///     攻撃の種類や基本ダメージなど、攻撃に関する定数的な情報を管理するためのクラス。
    /// </summary>
    public class AttackDefinition
    {
        /// <summary>
        ///     攻撃の基本情報を初期化するコンストラクタ。
        /// </summary>
        public AttackDefinition(string attackName,
            AttackSpec attackSpec,
            IAttackPipeline attackPipeline,
            BeatType? beatType = null,
            float justDamageMultiplier = 1.0f
            )
        {
            AttackName = attackName;
            AttackSpec = attackSpec;
            AttackPipeline = attackPipeline;
            BeatType = beatType;
            JustDamageMultiplier = justDamageMultiplier;
        }

        /// <summary> 攻撃の名前を表すプロパティ。 </summary>
        public string AttackName { get; }
        /// <summary> 攻撃のパラメータセットを表すプロパティ。 </summary>
        public AttackSpec AttackSpec { get; }
        /// <summary> 攻撃の処理パイプラインを表すプロパティ。 </summary>
        public IAttackPipeline AttackPipeline { get; }

        /// <summary> 対応するビートタイプを取得する。 </summary>
        public BeatType? BeatType { get; }

        public float JustDamageMultiplier { get; }
    }
}

