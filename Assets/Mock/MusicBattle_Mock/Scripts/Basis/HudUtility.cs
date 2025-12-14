using Mock.MusicBattle.MusicSync;
using Mock.MusicBattle.Player;
using Mock.MusicBattle.UI;
using System.Threading;

namespace Mock.MusicBattle.Basis
{
    public static class HudUtility
    {
        public static void Init(IngameHUDManager hud, PlayerManager player,
                        CriMusicBuffer musicBuffer, InputBuffer inputBuffer
            , CancellationToken destroyToken)
        {
            hud.InitializePlayerHealthBar(player.HealthEntity);
            hud.Initialize(musicBuffer);
            player.OnAttacked += Action_started;

            destroyToken.Register(() => player.OnAttacked -= Action_started);

            void Action_started(float signature)
            {
                hud.CreateNote((float)(musicBuffer.CurrentBeat / 4d), signature);
            }
        }

    }
}
