using KillChord.Runtime.Domain.InGame.Mission;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    [CreateAssetMenu(
        fileName = nameof(EnemyMissionKeyAsset),
        menuName = "KillChord/Mission" + "/" + nameof(EnemyMissionKeyAsset))]
    public class EnemyMissionKeyAsset : ScriptableObject
    {
        public EnemyMissionKey Id => new EnemyMissionKey(_id);
        public string DesplayName => _desplayName;

        [SerializeField] private string _id;
        [SerializeField] private string _desplayName;

#if UNITY_EDITOR
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
