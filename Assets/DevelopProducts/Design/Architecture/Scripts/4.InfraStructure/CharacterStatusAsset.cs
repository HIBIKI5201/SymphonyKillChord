using DevelopProducts.Architecture.Utility;
using UnityEngine;

namespace DevelopProducts.Architecture.InfraStructure
{
    /// <summary>
    ///     キャラクターの基本ステータスを保持するアセットクラス。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(CharacterStatusAsset),
        menuName = Const.CREATE_ASSET_PATH + nameof(CharacterStatusAsset), order = 1)]
    public class CharacterStatusAsset : ScriptableObject
    {
        /// <summary> キャラクター名。 </summary>
        public string Name => _name;

        /// <summary> 初期体力。 </summary>
        public float Health => _health;

        /// <summary> 移動速度。 </summary>
        public float Speed => _speed;

        /// <summary> 攻撃力。 </summary>
        public float AttackPower => _attackPower;

        [SerializeField, Tooltip("キャラクターの名前。")]
        private string _name = "Character Name";
        [SerializeField, Min(0), Tooltip("キャラクターの最大体力。")]
        private float _health = 100;
        [SerializeField, Min(0), Tooltip("キャラクターの移動速度。")]
        private float _speed = 1;
        [SerializeField, Min(0), Tooltip("キャラクターの攻撃力。")]
        private float _attackPower = 10;
    }
}
