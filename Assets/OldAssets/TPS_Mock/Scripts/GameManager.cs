using UnityEngine;

namespace Mock.TPS
{
    /// <summary>
    ///     ゲーム全体の管理クラス。
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        private InputBuffer _inputBuffer;

        [SerializeField]
        private CameraManager _cameraManager;
        [SerializeField]
        private PlayerManager _playerManager;

        [SerializeField]
        private HealthbarManager _healthbarManager;

        [SerializeField]
        private EnemyContainer _enemyContainer;

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;

            _cameraManager.Init(_inputBuffer, _enemyContainer);
            _playerManager.Init(_inputBuffer, _cameraManager, _healthbarManager);
        }
    }
}