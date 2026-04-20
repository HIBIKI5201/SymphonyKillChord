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
            Damage baseDamage,
            AttackParameterSet attackParameterSet,
            IAttackPipeline attackPipeline,
            BeatType? beatType = null
            )
        {
            AttackName = attackName;
            BaseDamage = baseDamage;
            AttackParameterSet = attackParameterSet;
            AttackPipeline = attackPipeline;
            BeatType = beatType;
        }

        /// <summary> 攻撃の名前を表すプロパティ。 </summary>
        public string AttackName { get; }
        /// <summary> 攻撃の基本ダメージを表すプロパティ。 </summary>
        public Damage BaseDamage { get; }
        /// <summary> 攻撃のパラメータセットを表すプロパティ。 </summary>
        public AttackParameterSet AttackParameterSet { get; }
        /// <summary> 攻撃の処理パイプラインを表すプロパティ。 </summary>
        public IAttackPipeline AttackPipeline { get; }
        public BeatType? BeatType { get; }
    }
}
