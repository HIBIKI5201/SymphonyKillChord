using UnityEngine;

namespace KillChord.Develop
{
    [DefaultExecutionOrder(-1000)]
    public class DestroyOnAwake : MonoBehaviour
    {
        private void Awake() => Destroy(gameObject);
    }
}
