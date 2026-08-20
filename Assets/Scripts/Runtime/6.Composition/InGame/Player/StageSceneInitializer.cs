using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;
using KillChord.Runtime.View.InGame.Player;

namespace KillChord.Runtime.Composition.InGame.Player
{
    /// <summary>
    ///     ステージシーン内で共有する参照を登録するクラス。
    /// </summary>
    public class StageSceneObjects : MonoBehaviour, IStageSceneInstance
    {
        [SerializeField, Tooltip("プレイヤーの生成位置です。")]
        private PlayerSpawnPoint _playerSpawnPoint;

        /// <summary> プレイヤーの生成位置です。 </summary>
        public Transform PlayerSpawnPointTransform => _playerSpawnPoint != null ? _playerSpawnPoint.transform : null;

        /// <summary> サービスロケーターへシーン参照を登録する。 </summary>
        private void Awake()
        {
            ServiceLocator.RegisterInstance<IStageSceneInstance>(this, LocateType.Locator);
        }

        private void OnDestroy()
        {
            ServiceLocator.UnregisterInstance<IStageSceneInstance>(this);
        }
    }
}
