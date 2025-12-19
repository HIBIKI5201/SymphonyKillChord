using System.Linq;
using UnityEngine;

namespace Mock.MusicBattle.MusicSync
{
    /// <summary>
    ///     プレイヤーの音楽同期テストクラス（開発用）。
    /// </summary>
    public class TestPlayer : MonoBehaviour
    {
        // CONSTRUCTOR
        // PUBLIC_EVENTS
        // PUBLIC_PROPERTIES
        // INTERFACE_PROPERTIES
        // PUBLIC_CONSTANTS
        // PUBLIC_METHODS
        // PUBLIC_INTERFACE_METHODS
        // PUBLIC_ENUM_DEFINITIONS
        // PUBLIC_CLASS_DEFINITIONS
        // PUBLIC_STRUCT_DEFINITIONS
        // CONSTANTS
        #region インスペクター表示フィールド
        /// <summary> 音楽同期マネージャーの参照。 </summary>
        [SerializeField, Tooltip("音楽同期マネージャーの参照。")]
        private MusicSyncManager _musicSyncManager;
        /// <summary> 音楽UIの参照。 </summary>
        [SerializeField, Tooltip("音楽UIの参照。")]
        private MusicUI _musicUI;
        /// <summary> 拍子リスト。 </summary>
        [SerializeField, Tooltip("拍子リスト。")]
        private float[] _timeSignatures = {16f, 12f, 8f, 6f, 4f, 3f, 2f, 1f};
        /// <summary> ノートの色リスト。 </summary>
        [SerializeField, Tooltip("ノートの色リスト。")]
        private Color[] _noteColor = { Color.red, Color.orange, Color.yellow, Color.green, Color.cyan, Color.blue, Color.purple, Color.white };
        #endregion

        // PRIVATE_FIELDS
        #region Unityライフサイクルメソッド
        /// <summary>
        ///     フレームごとに呼び出されます。
        ///     スペースキーの入力に応じてノーツを生成します。
        /// </summary>
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                float timeSig = _musicSyncManager.GetInputTimeSignature();
                Debug.Log($"プレイヤーの入力拍子: {timeSig}です。");

                // ノート作成と記録
                int detectedTimeSignatureIndex = 0;
                for(int i = 0; i < _timeSignatures.Length; i++)
                {
                    if (Mathf.Approximately(_timeSignatures[i], timeSig))
                    {
                        detectedTimeSignatureIndex = i;
                        break;
                    }
                }
                _musicUI.CreateNote(_noteColor[detectedTimeSignatureIndex]);
            }
        }
        #endregion

        // EVENT_HANDLER_METHODS
        // PROTECTED_INTERFACE_VIRTUAL_METHODS
        // PRIVATE_METHODS
        // PRIVATE_ENUM_DEFINITIONS
        // PRIVATE_CLASS_DEFINITIONS
        // PRIVATE_STRUCT_DEFINITIONS
    }
}
