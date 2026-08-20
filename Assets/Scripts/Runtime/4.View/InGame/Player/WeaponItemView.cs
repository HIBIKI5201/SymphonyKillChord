using KillChord.Runtime.View.Persistent.Music;
using System;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Player
{
    /// <summary>
    ///     武器一つのSE再生、表示切替、Effect再生などを行うクラス。
    /// </summary>
    public class WeaponItemView : MonoBehaviour
    {
        /// <summary>
        ///     攻撃によるSEやEffect、モデル切り替えを行います。
        /// </summary>
        /// <param name="clipSeconds"> クリップの長さ。 </param>
        /// <param name="ct"> CancellationToken。 </param>
        /// <returns></returns>
        public async Awaitable PlayAsync(float clipSeconds, CancellationToken ct)
        {
            ShowWeapon();
            PlayAttackSound();
            PlayEffectAsync(ct);
            await Awaitable.WaitForSecondsAsync(clipSeconds, ct);

            HideWeapon();
        }

        /// <summary>
        ///     武器を非表示にします。
        /// </summary>
        public void HideWeapon()
        {
            if (_weaponModel == null)
            {
                return;
            }

            _weaponModel.SetActive(false);
        }

        [SerializeField, Tooltip("攻撃中だけ表示する武器モデル。")]
        private GameObject _weaponModel;

        [SerializeField, Tooltip("攻撃SE用Source。")]
        private SoundEffectSource _attackSoundSource;

        [SerializeField, Tooltip("攻撃Effect。")]
        private ParticleSystem _attackEffect;

        [SerializeField, Min(0f), Tooltip("攻撃Effectを再生するまでの遅延時間。")]
        private float _effectDelaySeconds;

        /// <summary>
        ///     武器を表示します。
        /// </summary>
        private void ShowWeapon()
        {
            if (_weaponModel == null)
            {
                Debug.LogError($"WeaponModel が未設定です。{name}", this);
                return;
            }

            _weaponModel.SetActive(true);
        }

        /// <summary>
        ///     攻撃SEを再生します。
        /// </summary>
        private void PlayAttackSound()
        {
            if (_attackSoundSource == null)
            {
                return;
            }
            _attackSoundSource.Play();
        }

        /// <summary>
        ///     遅延後に攻撃Effectを再生します。
        /// </summary>
        private async Awaitable PlayEffectAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_attackEffect == null)
                {
                    return;
                }

                if (_effectDelaySeconds > 0f)
                {
                    await Awaitable.WaitForSecondsAsync(_effectDelaySeconds, cancellationToken);
                }

                cancellationToken.ThrowIfCancellationRequested();
                _attackEffect.Play();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }
    }
}
