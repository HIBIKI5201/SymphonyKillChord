using UnityEngine;

namespace KillChord.Develop
{
    [DefaultExecutionOrder(-1000)]
    [AddComponentMenu("KillChord/Debug/Common/DestroyOnAwake")]
    public class DestroyOnAwake : MonoBehaviour
    {
        private void Awake() => Destroy(gameObject);
    }
}
