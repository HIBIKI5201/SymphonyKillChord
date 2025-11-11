using UnityEngine;

namespace Mock.CharacterControl
{
    /// <summary>
    ///     プレイヤーの管理クラス。
    /// </summary>
    public class PlayerManager
    {
        public PlayerManager(SymphonyAnimeController animeController)
        {
            _animeController = animeController;
        }

        public void Update()
        {

        }

        private readonly SymphonyAnimeController _animeController;
    }
}
