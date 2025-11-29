using System.Linq;
using UnityEngine;

namespace Mock.MusicSyncMock
{
    public class TestPlayer : MonoBehaviour
    {
        [SerializeField] private MusicSyncManager _musicSyncManager;
        [SerializeField] private MusicUI _musicUI;
        [SerializeField] private float[] _timeSignatures = {16f, 12f, 8f, 6f, 4f, 3f, 2f, 1f};
        [SerializeField] private Color[] _noteColor = { Color.red, Color.orange, Color.yellow, Color.green, Color.cyan, Color.blue, Color.purple, Color.white };

        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                float timeSig = _musicSyncManager.GetInputTimeSignature();
                Debug.Log($"プレイヤー入力の拍子: {timeSig}");

                // ノート作成と記録
                int detectedTimeSignatureIndex = 0;
                for(int i = 0; i < _timeSignatures.Length; i++)
                {
                    if (_timeSignatures[i] == timeSig)
                    {
                        detectedTimeSignatureIndex = i;
                        break;
                    }
                }
                _musicUI.CreateNote(_noteColor[detectedTimeSignatureIndex]);
            }
        }
    }
}
