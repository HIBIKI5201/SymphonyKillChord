using KillChord.Runtime.Utility;
using UnityEngine;

namespace KillChord.Develop
{
    public class RingBufferTest : MonoBehaviour
    {
        RingBuffer<int> buffer = new RingBuffer<int>(5);

        private void Start()
        {
            Debug.Log(string.Join(" ", buffer.AsReadonlySpan().ToArray()));
            buffer.Enqueue(1);
            buffer.Enqueue(2);
            buffer.Enqueue(3);
            buffer.Enqueue(4);
            buffer.Enqueue(5);

            Debug.Log(string.Join(" ", buffer.AsReadonlySpan().ToArray()));
            buffer.Enqueue(6);
            Debug.Log(string.Join(" ", buffer.AsReadonlySpan().ToArray()));
            buffer.Enqueue(7);
            Debug.Log(string.Join(" ", buffer.AsReadonlySpan().ToArray()));
            buffer.Clear();
            Debug.Log(string.Join(" ", buffer.AsReadonlySpan().ToArray()));
            buffer.Enqueue(7);
            Debug.Log(string.Join(" ", buffer.AsReadonlySpan().ToArray()));
            buffer.Clear();
            Debug.Log(string.Join(" ", buffer.AsReadonlySpan().ToArray()));
        }
    }
}