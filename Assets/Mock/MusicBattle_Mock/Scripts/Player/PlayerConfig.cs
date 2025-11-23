using UnityEngine;

namespace Mock.MusicBattle
{
    [CreateAssetMenu(fileName = nameof(PlayerConfig), menuName = "Mock/Music_Battle/" + nameof(PlayerConfig))]
    public class PlayerConfig : ScriptableObject
    {
        public string IgnoreAttackTagName => _ignoreAttackTagName;
        [SerializeField, Tooltip("攻撃が当たらないタグ")]
        private string _ignoreAttackTagName;
    }
}
