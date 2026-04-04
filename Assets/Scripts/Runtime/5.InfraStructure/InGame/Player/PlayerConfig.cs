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
            => new(
                Mathf.Max(0f, _moveSpeed),
                Mathf.Max(0f, _dodgeSpeed),
                Mathf.Max(0f, _dodgeDuration),
                Mathf.Max(0f, _dodgeCooldown)
                );

        [SerializeField] private float _moveSpeed;
        [Space]
        [SerializeField] private float _dodgeSpeed;
        [SerializeField] private float _dodgeDuration;
        [SerializeField] private float _dodgeCooldown;
    }
}
