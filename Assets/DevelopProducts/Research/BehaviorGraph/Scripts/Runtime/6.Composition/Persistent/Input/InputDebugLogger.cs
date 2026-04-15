using DevelopProducts.BehaviorGraph.Runtime.Composition;
using DevelopProducts.BehaviorGraph.Runtime.Domain.Persistent.Input;
using UnityEngine;

namespace DevelopProducts.BehaviorGraph.Runtime.View
{
    /// <summary>
    ///     入力バッファの内容をデバッグ出力するクラス。
    /// </summary>
    public class InputDebugLogger : MonoBehaviour
    {
        [SerializeField] private InputComposition _root;

        private void Start()
        {
            InputBufferingQueue buffer = _root.GetBufferedInputBuffer;
            buffer.OnBuffered += OnBuffered;
        }

        private void OnBuffered(BufferedInput input)
        {
            Debug.Log($"[INPUT] {input}");
        }
    }
}
