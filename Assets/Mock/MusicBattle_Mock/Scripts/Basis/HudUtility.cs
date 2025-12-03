using Mock.MusicBattle.Basis;
using Mock.MusicBattle.Character;
using Mock.MusicBattle.MusicSync;
using Mock.MusicBattle.Player;
using Mock.MusicBattle.UI;
using System.Threading;
using UnityEngine;

namespace Mock.MusicBattle
{
    public static class HudUtility
    {
        public static void AddEnemyHealthBar(
        IngameHUDManager hud,
        Transform enemyTransform,
        HealthEntity _health)
        {
            _ = hud.AddEnemyHealthBar(_health, enemyTransform);
           
        }

        public static void ApplyDamage(
            IngameHUDManager hud,
            HealthEntity health,
            Transform enemyTransform,
            float damage)
        {
            health.TakeDamage(damage);
            hud.ShowDamageText(damage, enemyTransform.position);
        }
        public static void Init(IngameHUDManager hud, PlayerManager player,
                        CriMusicBuffer musicBuffer, InputBuffer inputBuffer
            , CancellationToken destroyToken)
        {
            hud.InitializePlayerHealthBar(player.HealthEntityPlayer);
            hud.Initialize(musicBuffer);
            inputBuffer.AttackAction.Started += Action_started;

            destroyToken.Register(() => inputBuffer.AttackAction.Started -= Action_started);

            void Action_started(float a)
            {
                hud.CreateNote((float)(musicBuffer.CurrentBeat / 4d));
            }
        }

    }
}
