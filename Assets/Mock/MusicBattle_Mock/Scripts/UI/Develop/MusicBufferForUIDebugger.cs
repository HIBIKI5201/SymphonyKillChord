using Mock.MusicBattle.MusicSync;
using Mock.MusicBattle.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Mock.MusicBattle.Develop
{
    /// <summary>
    ///     音楽バッファのモック実装。UIデバッガー用。
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class MusicBufferForUIDebugger : MonoBehaviour, IMusicBuffer
    {
        public double CurrentBpm => 120;

        public double BeatLength => 60 / CurrentBpm;

        public double CurrentBeat => Time.time / BeatLength;

        [SerializeField]
        private string _createNotesActionName = "Attack";

        private void Awake()
        {
            PlayerInput playerInput = GetComponent<PlayerInput>();
            InputAction action = playerInput.actions[_createNotesActionName];

            IngameHUDManager hud = FindAnyObjectByType<IngameHUDManager>();
            hud.Initialize(this);
            action.started += Action_started;

            destroyCancellationToken.Register(() => action.started -= Action_started);

            void Action_started(InputAction.CallbackContext obj)
            {
                hud.CreateNote((float)(CurrentBeat / 4d), 4);
            }
        }
    }
}
