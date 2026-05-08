using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Player;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Player
{
    /// <summary>
    ///     プレイヤーの移動や回避に関する設定値を保持するScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(PlayerConfig), menuName = "KillChord/InGame/PlayerConfig")]
    public sealed class PlayerConfig : ScriptableObject
    {
        public PlayerMoveParameter ToDomain()
            => new PlayerMoveParameter(
                new MoveSpeed(_moveSpeed),
                new DodgeSpeed(_dodgeSpeed),
                new DodgeDuration(_dodgeDuration),
                new DodgeCooldown(_dodgeCooldown)
            );

        [SerializeField] private float _moveSpeed;
        [Space]
        [SerializeField] private float _dodgeSpeed;
        [SerializeField] private float _dodgeDuration;
        [SerializeField] private float _dodgeCooldown;
    }
}
