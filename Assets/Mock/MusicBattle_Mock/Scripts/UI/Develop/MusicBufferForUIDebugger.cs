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
        #region パブリックプロパティ
        /// <summary> 現在のBPM。 </summary>
        public double CurrentBpm => 120;
        /// <summary> 1拍の長さ（秒）。 </summary>
        public double BeatLength => 60 / CurrentBpm;
        /// <summary> 現在の拍数。 </summary>
        public double CurrentBeat => Time.time / BeatLength;
        /// <summary> BGMの固有拍子（デバッグ用のため固定値）。 </summary>
        public double PropTimeSignature => 4; // デバッグ用のため固定値
        #endregion
        // PUBLIC_METHODS
        #region インスペクター表示フィールド
        /// <summary> ノーツを生成するアクション名。 </summary>
        [SerializeField, Tooltip("ノーツを生成するアクション名。")]
        private string _createNotesActionName = "Attack";
        #endregion
        #region Unityライフサイクルメソッド
        /// <summary>
        ///     スクリプトインスタンスがロードされたときに呼び出されます。
        ///     PlayerInputからのアクションを購読し、HUDを初期化します。
        /// </summary>
        private void Awake()
        {
            PlayerInput playerInput = GetComponent<PlayerInput>();
            InputAction action = playerInput.actions[_createNotesActionName];

            IngameHUDManager hud = FindAnyObjectByType<IngameHUDManager>();
            hud.Initialize(this); // IMusicBufferを渡す
            action.started += Action_started;

            destroyCancellationToken.Register(() => action.started -= Action_started);

            // アクションが開始されたときにHUDにノーツを生成するローカル関数。
            void Action_started(InputAction.CallbackContext obj)
            {
                hud.CreateNote((float)(CurrentBeat / 4d), 4);
            }
        }
        #endregion
        #region パブリックインターフェースメソッド
        /// <summary>
        ///     小節タイミング情報を基いて、全体拍数を算出する（デバッグ用のため簡略化）。
        /// </summary>
        /// <param name="barTimingInfo">小節タイミング情報。</param>
        /// <returns>全体拍数。</returns>
        public double ConvertBarTimingInfoToBeat(BarTimingInfo barTimingInfo)
        {
            // デバッグ用のため、簡略化した計算。
            // 実際のCriMusicBufferのロジックとは異なる。
            return CurrentBeat + barTimingInfo.TargetBeat;
        }
        #endregion
    }
}

