using KillChord.Runtime.InfraStructure.InGame.Battle;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Battle
{
    /// <summary>
    ///     攻撃の定義を保持するScriptableObjectクラス。
    /// </summary>
    [CreateAssetMenu(fileName = "AttackDefinitionAsset", menuName = "KillChord/AttackDefinitionAsset")]
    public class AttackDefinitionAsset : ScriptableObject
    {
        /// <summary> 攻撃名を取得する。 </summary>
        public string AttackName => _attackName;
        /// <summary> 攻撃パラメーターセットを取得する。 </summary>
        public AttackSpecAsset AttackSpecAsset => _attackParameterSetData;
        /// <summary> 攻撃パイプラインアセットを取得する。 </summary>
        public AttackPipelineAsset AttackPipelineAsset => _attackPipelineAsset;
        /// <summary> ビートタイプを使用するかどうかを取得する。 </summary>
        public bool UseBeatType => _useBeatType;
        /// <summary> ビートタイプを取得する。 </summary>
        public int BeatType => _beatType;
        /// <summary> ジャストダメージ倍率を取得する。 </summary>
        public float JustDamageMultiplier => _justDamageMultiplier;
        /// <summary> 武器ダメージ倍率を取得する。 </summary>
        public float WeaponDamageMultiplier => _weaponDamageMultiplier;
        /// <summary> 射程を取得する。 </summary>
        public float Range => _range;
        /// <summary> 攻撃範囲の中心軸からの半角（度）を取得する。 </summary>
        public float HalfAngleDegrees => _halfAngleDegrees;
        /// <summary> 範囲内の複数対象へ同時に命中するかどうかを取得する。 </summary>
        public bool IsMultiTarget => _isMultiTarget;
        /// <summary> 1回の攻撃で発生するヒット数を取得する。 </summary>
        public int HitCount => _hitCount;
        /// <summary> 多段ヒット時の1ヒットあたりの間隔（秒）を取得する。 </summary>
        public float HitInterval => _hitInterval;
        /// <summary> クリティカルダメージの倍率を取得する。 </summary>
        public float CriticalDamageMultiplier => _criticalDamageMultiplier;

        private const float DEFAULT_WEAPON_DAMAGE_MULTIPLIER = 1f;
        private const float DEFAULT_RANGE = 10f;
        private const float DEFAULT_HALF_ANGLE_DEGREES = 1f;
        private const int DEFAULT_HIT_COUNT = 1;
        private const float DEFAULT_HIT_INTERVAL = 0.05f;
        private const float DEFAULT_CRITICAL_DAMAGE_MULTIPLIER = 2f;

        [SerializeField, Tooltip("攻撃名")] private string _attackName;
        [SerializeField, Tooltip("攻撃パラメーターセット")] private AttackSpecAsset _attackParameterSetData;
        [SerializeField, Tooltip("攻撃パイプラインアセット")] private AttackPipelineAsset _attackPipelineAsset;
        [SerializeField, Tooltip("ジャストダメージ倍率")] private float _justDamageMultiplier;
        [SerializeField, Tooltip("ビートタイプを使用するかどうか")] private bool _useBeatType;
        [SerializeField, Tooltip("ビートタイプ")] private int _beatType;

        [SerializeField, Tooltip("武器ダメージ倍率。攻撃力に対する倍率。仕様の「攻撃力200%」は 2 を指定する")]
        private float _weaponDamageMultiplier = DEFAULT_WEAPON_DAMAGE_MULTIPLIER;

        [SerializeField, Tooltip("射程（m）。プレイヤーからの水平距離で判定する")]
        private float _range = DEFAULT_RANGE;

        [SerializeField, Tooltip("攻撃範囲の中心軸からの半角（度）。直線状の攻撃は小さい値を指定する")]
        private float _halfAngleDegrees = DEFAULT_HALF_ANGLE_DEGREES;

        [SerializeField, Tooltip("範囲内の複数対象へ同時に命中するかどうか。falseの場合は最も近い1体のみを攻撃する")]
        private bool _isMultiTarget;

        [SerializeField, Tooltip("1回の攻撃で発生するヒット数。三点バーストは 3 を指定する")]
        private int _hitCount = DEFAULT_HIT_COUNT;

        [SerializeField, Tooltip("多段ヒット時の1ヒットあたりの間隔（秒）。攻撃硬直より短い値にすること")]
        private float _hitInterval = DEFAULT_HIT_INTERVAL;

        [SerializeField,
        Tooltip("クリティカルダメージの倍率。武器ごとに設定する。仕様書の「上昇率％」とは単位が違うので注意（仕様書の「クリティカルダメージ50」は 1.5 を指定）。" +
            "会心率はキャラクター側で設定する")]
        private float _criticalDamageMultiplier = DEFAULT_CRITICAL_DAMAGE_MULTIPLIER;
    }
}
