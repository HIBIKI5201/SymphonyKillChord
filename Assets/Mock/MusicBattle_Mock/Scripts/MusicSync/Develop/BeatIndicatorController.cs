using UnityEngine;

namespace Mock.MusicBattle.MusicSync
{
    public class BeatIndicatorController : MonoBehaviour
    {
        [SerializeField] private MusicActionHandler _musicActionHandler;
        [SerializeField] private float zoomSpeed = 0.016f;
        void Start()
        {
            _musicActionHandler.OnBeat += BeatAction;
        }

        private void FixedUpdate()
        {
            transform.localScale -= Vector3.one * zoomSpeed;
            if (transform.localScale.x < 0f)
            {
                transform.localScale = Vector3.zero;
            }
        }

        private void BeatAction()
        {
            transform.localScale = Vector3.one;
        }
    }
}
