using KillChord.Runtime.Application;
using KillChord.Runtime.Composition;
using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.View
{
    public class InputDebugLogger : MonoBehaviour
    {
        [SerializeField] private InputComposition _root;

        private void Start()
        {
            BufferedInputBuffer buffer = _root.GetBufferedInputBuffer;
            buffer.OnBuffered += OnBuffered;
        }

        private void OnBuffered(BufferedInput input)
        {
            Debug.Log($"[INPUT] {input}");
        }
    }
}
