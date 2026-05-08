using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    /// <summary>
    ///     ステージシーン内で共有する参照を登録するクラス。
    /// </summary>
    public class StageSceneObjects : MonoBehaviour, IStageSceneInstance
    {
        [SerializeField, Tooltip("プレイヤーのTransform。")]
        private Transform _playerTransform;

        [SerializeField, Tooltip("スキル初期化コンポーネント。")]
        private SkillInitializer _skillInitializer;

        /// <summary> プレイヤーのTransform。 </summary>
        public Transform PlayerTransform => _playerTransform;

        /// <summary> スキル初期化コンポーネント。 </summary>
        public SkillInitializer SkillInitializer => _skillInitializer;

        /// <summary> サービスロケーターへシーン参照を登録する。 </summary>
        private void Awake()
        {
            ServiceLocator.RegisterInstance<IStageSceneInstance>(this);
        }
    }

    /// <summary>
    ///     ステージシーン参照の公開インターフェース。
    /// </summary>
    public interface IStageSceneInstance
    {
        Transform PlayerTransform { get; }
        SkillInitializer SkillInitializer { get; }
    }
}
