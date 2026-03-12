using DevelopProducts.Persistent.Application;
using TMPro;
using UnityEngine;

namespace DevelopProducts.Persistent.View
{
    /// <summary>
    ///     入力履歴のデバッグ表示を行うクラス。
    /// </summary>
    public class InputBufferDebugView : MonoBehaviour
    {
        public void Initialize(IInputBufferReader inputBufferReader) 
        {
            _inputBufferReader = inputBufferReader;
        }

        [SerializeField] private TMP_Text _text;
        [SerializeField] private int _previewCount;

        private IInputBufferReader _inputBufferReader;

        private void Update()
        {
            if (_inputBufferReader == null || _text == null)
            {
                return;
            }

            int count = Mathf.Min(_inputBufferReader.Count, _previewCount);

            System.Text.StringBuilder sb = new();

            for (int i = 0; i < count; i++)
            {
                int offset = count - 1 - i;
                sb.AppendLine(_inputBufferReader.GetLast(offset).ToString());
            }

            _text.text = sb.ToString();
        }
    }
}
