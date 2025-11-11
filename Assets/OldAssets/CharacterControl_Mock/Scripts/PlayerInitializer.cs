using UnityEngine;

namespace Mock.CharacterControl
{
    /// <summary>
    ///     プレイヤーを初期化するクラス。
    /// </summary>
    public class PlayerInitializer : MonoBehaviour
    {
        [SerializeField]
        private PlayerStatus _status;
        [SerializeField]
        private InputBuffer _inputBuffer;
        [SerializeField]
        private SymphonyAnimeController _symphonyAnimeController;

        private PlayerManager _playerManager;
        private void Awake()
        {
            if (_symphonyAnimeController == null) { return; }
            _playerManager = new(_status,
                _symphonyAnimeController);

            if (_inputBuffer == null) { return; }
            _playerManager.InputRegister(_inputBuffer);
        }
    }
}
