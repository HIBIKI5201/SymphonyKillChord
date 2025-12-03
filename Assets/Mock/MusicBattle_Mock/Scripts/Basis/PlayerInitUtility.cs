using Mock.MusicBattle.Battle;
using Mock.MusicBattle.Camera;
using Mock.MusicBattle.Player;
using Unity.Cinemachine;

namespace Mock.MusicBattle.Basis
{
    /// <summary>プレイヤーの初期化ユーティリティクラス</summary>
    public static class PlayerInitUtility
    {
        /// <summary>プレイヤーの初期化をする。</summary>
        public static void InitPlayer(
            PlayerManager playerManager,
            InputBuffer inputBuffer,
            CameraManager cameraManager,
            CinemachineCamera cinemachineCamera,
            LockOnManager lockOnManager
            )
        {
            cameraManager.Init(inputBuffer, lockOnManager);
            playerManager.Init(inputBuffer, cinemachineCamera);
        }
    }
}
