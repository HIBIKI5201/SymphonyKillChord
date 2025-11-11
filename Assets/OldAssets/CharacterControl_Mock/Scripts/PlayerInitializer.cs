using UnityEngine;

namespace Mock.CharacterControl
{
    /// <summary>
    ///     プレイヤーを初期化するクラス。
    /// </summary>
    public class PlayerInitializer : MonoBehaviour
    {
        [SerializeField]
        private SymphonyAnimeController _symphonyAnimeController;

        private PlayerManager _playerManager;
        private void Awake()
        {
            _playerManager = new(_symphonyAnimeController);
        }

        private void Update()
        {
            _playerManager?.Update();
        }
    }
}
