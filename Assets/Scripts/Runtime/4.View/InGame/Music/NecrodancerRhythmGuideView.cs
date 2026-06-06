using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Music
{
    public sealed class NecrodancerRhythmGuideView : MonoBehaviour
    {
        public event Action OnUpdate;

        [Tooltip("落ちてくるビート達のRectTransform")]
        [SerializeField] private RectTransform _beatRectTransform;

        void Update()
        {
            OnUpdate?.Invoke();
        }
        private void OnDestroy()
        {
            OnUpdate = null;
        }
    }
}
