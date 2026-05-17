using KillChord.Runtime.Domain.InGame.Mission;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     敵のミッションキーを定義するScriptableObject。
    /// </summary>
    [CreateAssetMenu(
        fileName = nameof(EnemyMissionKeyAsset),
        menuName = "KillChord/Mission" + "/" + nameof(EnemyMissionKeyAsset))]
    public class EnemyMissionKeyAsset : ScriptableObject
    {
        /// <summary> 敵のミッションキーを取得します。 </summary>
        public EnemyMissionKey Id => new EnemyMissionKey(_id);
        /// <summary> 表示名を取得します。 </summary>
        public string DisplayName => _displayName;

        [SerializeField, Tooltip("敵を識別するためのユニークなID。")] private string _id;
        [SerializeField, Tooltip("ミッションHUD等に表示される敵の名称。")] private string _displayName;

#if UNITY_EDITOR
        /// <summary>
        ///     値の検証を行います。
        /// </summary>
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(_id))
            {
                Debug.LogError($"{nameof(EnemyMissionKeyAsset)} の ID が未設定です。", this);
            }
        }
#endif
    }
}
