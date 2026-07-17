using UnityEngine;
using KillChord.Runtime.InfraStructure.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Battle;

namespace KillChord.Runtime.InfraStructure.InGame.Character
{
    /// <summary>
    ///     キャラクターの基本的なデータを保持するScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(CharacterDefinitionAsset),
    menuName = "KillChord/Character/CharacterDefinitionAsset")]
    public class CharacterDefinitionAsset : ScriptableObject
    {
        /// <summary> キャラクター名を取得する。 </summary>
        public string CharacterName => _characterName;

        /// <summary> キャラクターの攻撃硬直時間を取得する。 </summary>
        public float AttackInterval => _attackInterval;

        /// <summary> 最大HPを取得する。 </summary>
        public float MaxHealth => _maxHealth;

        /// <summary> 攻撃定義の配列を取得する。 </summary>
        public AttackDefinitionAsset[] AttackDefinitionAssets =>
            _attackDifinitions == null ? null : (AttackDefinitionAsset[])_attackDifinitions.Clone();

        /// <summary> キャラクターの攻撃の基本ダメージを取得する。 </summary>
        public int BaseDamage => _baseDamage;

        [SerializeField, Tooltip("キャラクターの名前。")]
        private string _characterName;

        [SerializeField, Tooltip("キャラクターの攻撃硬直時間")]
        private float _attackInterval = 0.5f;

        [SerializeField, Tooltip("キャラクターの最大体力。")]
        private float _maxHealth;

        [SerializeField, Tooltip("キャラクターが使用する攻撃の定義リスト。")]
        private AttackDefinitionAsset[] _attackDifinitions;
        [SerializeField, Tooltip("キャラクターの攻撃の基本ダメージ。")]
        private int _baseDamage;
    }
}

