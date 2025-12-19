using Mock.MusicBattle.Battle;
using Mock.MusicBattle.MusicSync;
using Mock.MusicBattle.Player;
using Mock.MusicBattle.UI;
using System.Threading;

namespace Mock.MusicBattle.Basis
{
    /// <summary>
    ///     HUD関連のユーティリティクラス。
    /// </summary>
    public static class HudUtility
    {
        /// <summary>
        ///     HUDの初期化を行う。
        /// </summary>
        /// <param name="hud">IngameHUDManagerのインスタンス。</param>
        /// <param name="player">PlayerManagerのインスタンス。</param>
        /// <param name="musicBuffer">CriMusicBufferのインスタンス。</param>
        /// <param name="inputBuffer">InputBufferのインスタンス。</param>
        /// <param name="lockOnManager">LockOnManagerのインスタンス。</param>
        /// <param name="destroyToken">破棄時にキャンセルされるCancellationToken。</param>
        public static void Init(IngameHUDManager hud, PlayerManager player,
                        CriMusicBuffer musicBuffer, InputBuffer inputBuffer,
                        LockOnManager lockOnManager,
                        CancellationToken destroyToken)
        {
            hud.InitializePlayerHealthBar(player.HealthEntity);
            hud.InitializeLockOnCursor(lockOnManager);
            hud.Initialize(musicBuffer);
            player.OnAttacked += Action_started;

            destroyToken.Register(() => player.OnAttacked -= Action_started);

            /// <summary>
            ///     攻撃アクションが開始されたときにHUDにノーツを作成するローカル関数。
            /// </summary>
            /// <param name="signature">攻撃時の拍子。</param>
            void Action_started(float signature)
            {
                hud.CreateNote((float)(musicBuffer.CurrentBeat / 4d), signature);
            }
        }
    }
}
