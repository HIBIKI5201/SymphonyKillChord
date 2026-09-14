using KillChord.Runtime.View.Persistent.Music;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Character
{
    /// <summary>
    ///     再利用可能なSE再生のViewです。
    ///     再生元のGameObjectが即座に非アクティブ化されても
    ///     再生中のSEが途切れないよう、複製したSoundEffectSourceを外部で保持して再生します。
    /// </summary>
    public class ReusableSoundEffectView : MonoBehaviour
    {
        /// <summary>
        ///     SEを指定位置で再生します。
        /// </summary>
        /// <param name="position"> 再生する位置。 </param>
        public void PlayAt(Vector3 position)
        {
            if (!HasTemplate)
            {
                return;
            }

            SoundEffectSource instance = RentInstance();
            instance.transform.position = position;
            instance.gameObject.SetActive(true);
            instance.Play();

            StartCoroutine(ReleaseAfterPlaybackAsync(instance));
        }

        [SerializeField, Tooltip("再利用生成元として使うSoundEffectSourceです。")]
        private SoundEffectSource _soundEffectSourceTemplate;

        [SerializeField, Tooltip("再生開始からプールへ戻すまでの待機秒数です。再生するSEクリップの長さ以上の値を設定してください。")]
        private float _releaseDelaySeconds = 5f;

        private readonly Stack<SoundEffectSource> _pool = new();

        /// <summary>
        ///     テンプレートが設定されているかです。
        /// </summary>
        private bool HasTemplate => _soundEffectSourceTemplate != null;

        /// <summary>
        ///     テンプレートを初期状態（非表示）へ戻します。
        /// </summary>
        private void Awake()
        {
            if (HasTemplate)
            {
                _soundEffectSourceTemplate.gameObject.SetActive(false);
            }
        }

        /// <summary>
        ///     プールから再利用可能なインスタンスを取得します。無ければ複製します。
        /// </summary>
        /// <returns> 再生に使用するインスタンスです。 </returns>
        private SoundEffectSource RentInstance()
        {
            if (_pool.Count > 0)
            {
                return _pool.Pop();
            }

            return Instantiate(_soundEffectSourceTemplate, transform);
        }

        /// <summary>
        ///     再生完了想定時間の経過後、インスタンスをプールへ戻します。
        /// </summary>
        /// <param name="instance"> 対象インスタンスです。 </param>
        private IEnumerator ReleaseAfterPlaybackAsync(SoundEffectSource instance)
        {
            yield return new WaitForSeconds(_releaseDelaySeconds);

            instance.gameObject.SetActive(false);
            _pool.Push(instance);
        }
    }
}
