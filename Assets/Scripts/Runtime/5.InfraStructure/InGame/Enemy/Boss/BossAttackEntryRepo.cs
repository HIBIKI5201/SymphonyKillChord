using KillChord.Runtime.Utility.Constant;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Enemy
{
    /// <summary>
    ///     ボスの攻撃定義の集合。
    /// </summary>
    [CreateAssetMenu(fileName = "BossAttackEntryRepo", menuName = PathConst.CREATE_ASSET_MENU_PATH + "Enemy/" + nameof(BossAttackEntryRepo))]
    public class BossAttackEntryRepo : ScriptableObject
    {
        public BossAttackEntryAsset[] AttackEntries => _attackEntries;
        [SerializeField] private BossAttackEntryAsset[] _attackEntries;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_attackEntries == null) return;
            for (int i = 0; i < _attackEntries.Length; i++)
            {
                if (_attackEntries[i] == null)
                    Debug.LogWarning($"{name}: _attackEntries[{i}] が未設定です。");
            }
        }
#endif
    }
}

