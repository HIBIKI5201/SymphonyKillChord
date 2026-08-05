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
        /// <param name="attackName"> 攻撃名。 </param>
        /// <param name="attackSpec"> 攻撃のパラメータセット。 </param>
        /// <param name="attackPipeline"> 攻撃の処理パイプライン。 </param>
        /// <param name="beatType"> 対応する拍子。拍子に紐付かない攻撃の場合はnull。 </param>
        /// <param name="justDamageMultiplier"> ジャスト入力時のダメージ倍率。 </param>
        /// <param name="weaponDamageMultiplier"> 武器ごとのダメージ倍率。攻撃力に対する倍率。 </param>
        /// <param name="range"> 射程。 </param>
        /// <param name="halfAngleDegrees"> 攻撃範囲の中心軸からの半角（度）。 </param>
        /// <param name="isMultiTarget"> 範囲内の複数対象へ同時に命中する場合はtrue。 </param>
        /// <param name="hitCount"> 1回の攻撃で発生するヒット数。 </param>
        /// <param name="hitInterval"> 多段ヒット時の1ヒットあたりの間隔（秒）。 </param>
        public AttackDefinition(string attackName,
            AttackSpec attackSpec,
            IAttackPipeline attackPipeline,
            BeatType? beatType = null,
            float justDamageMultiplier = 1.0f,
            float weaponDamageMultiplier = 1.0f,
            float range = 0f,
            float halfAngleDegrees = 0f,
            bool isMultiTarget = false,
            int hitCount = 1,
            float hitInterval = 0f
            )
        {
            AttackName = attackName;
            AttackSpec = attackSpec;
            AttackPipeline = attackPipeline;
            BeatType = beatType;
            JustDamageMultiplier = justDamageMultiplier;
            WeaponDamageMultiplier = weaponDamageMultiplier;
            Range = range;
            HalfAngleDegrees = halfAngleDegrees;
            IsMultiTarget = isMultiTarget;
            HitCount = hitCount;
            HitInterval = hitInterval;
        }

        /// <summary> 攻撃の名前を表すプロパティ。 </summary>
        public string AttackName { get; }
        /// <summary> 攻撃のパラメータセットを表すプロパティ。 </summary>
        public AttackSpec AttackSpec { get; }
        /// <summary> 攻撃の処理パイプラインを表すプロパティ。 </summary>
        public IAttackPipeline AttackPipeline { get; }

        /// <summary> 対応するビートタイプを取得する。 </summary>
        public BeatType? BeatType { get; }

        /// <summary> ジャスト入力時のダメージ倍率。 </summary>
        public float JustDamageMultiplier { get; }

        /// <summary> 武器ごとのダメージ倍率。攻撃力に対する倍率を表す。 </summary>
        public float WeaponDamageMultiplier { get; }

        /// <summary> 射程。 </summary>
        public float Range { get; }

        /// <summary> 攻撃範囲の中心軸からの半角（度）。 </summary>
        public float HalfAngleDegrees { get; }

        /// <summary> 範囲内の複数対象へ同時に命中する場合はtrue。falseの場合は最も近い1体のみを攻撃する。 </summary>
        public bool IsMultiTarget { get; }

        /// <summary> 1回の攻撃で発生するヒット数。三点バーストなどの多段攻撃で使用する。 </summary>
        public int HitCount { get; }

        /// <summary> 多段ヒット時の1ヒットあたりの間隔（秒）。 </summary>
        public float HitInterval { get; }

        /// <summary> 指定できる半角の最小値（度）。 </summary>
        public const float MIN_HALF_ANGLE_DEGREES = 0f;

        /// <summary> 指定できる半角の最大値（度）。全方位を表す。 </summary>
        public const float MAX_HALF_ANGLE_DEGREES = 180f;

        /// <summary> 指定できるヒット数の最小値。 </summary>
        public const int MIN_HIT_COUNT = 1;
    }
}
