using System;
using KillChord.Runtime.Adaptor.InGame.Animation;
using UnityEngine;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     プレイヤー固有の攻撃・回避アニメーション要求を伝達するSignal。
    /// </summary>
    public sealed class PlayerCharacterAnimationSignal : CharacterAnimationSignal, IPlayerCharacterAnimationSignal
    {
        /// <summary>
        ///     プレイヤー用Signalを初期化する。
        /// </summary>
        /// <param name="playbackMap"> 再生定義です。 </param>
        /// <param name="animationSpeedProvider"> 現在の再生速度取得処理です。 </param>
        /// <param name="timingCalculator"> 時間計算処理です。 </param>
        public PlayerCharacterAnimationSignal(
            CharacterAnimationPlaybackMap playbackMap,
            Func<float> animationSpeedProvider,
            CharacterAnimationOneShotTimingCalculator timingCalculator)
            : base(playbackMap, animationSpeedProvider, timingCalculator)
        {
        }

        /// <summary> 回避アニメーションの再生終了イベントです。 </summary>
        public event Action OnDodgeEnded;

        /// <summary>
        ///     回避アニメーションの再生を要求する。
        /// </summary>
        /// <returns> 再生時間です。 </returns>
        public float RequestDodge()
        {
            return RequestOneShot(
                PlaybackMap.Dodge,
                shouldNotifyDodgeEnded: true);
        }

        /// <summary>
        ///     プレイヤーの既定攻撃アニメーションの再生を要求する。
        /// </summary>
        /// <returns> 再生時間です。 </returns>
        public override float RequestAttack()
        {
            return RequestPlayerAttack(PlaybackMap.Attack);
        }

        /// <summary>
        ///     指定キーの攻撃アニメーションの再生を要求する。
        /// </summary>
        /// <param name="animationKey"> 置き換えたいアニメーションキー。 </param>
        /// <returns> 再生時間です。 </returns>
        public float RequestAttack(string animationKey)
        {
            if (!string.IsNullOrWhiteSpace(animationKey))
            {
                if (PlaybackMap.TryGetOneShotIndex(animationKey, out int oneShotIndex))
                {
                    return RequestPlayerAttack(oneShotIndex);
                }

                Debug.LogError(
                    $"[{nameof(PlayerCharacterAnimationSignal)}] ワンショットアニメーションキーが登録されていません。Key: {animationKey}");
            }

            return RequestPlayerAttack(PlaybackMap.Attack);
        }

        /// <summary>
        ///     攻撃BeatTypeに対応するアニメーションの再生を要求する。
        /// </summary>
        /// <param name="attackType"> 攻撃結果のBeatTypeです。 </param>
        /// <returns> 再生時間です。 </returns>
        public float RequestAttack(int attackType)
        {
            if (PlaybackMap.TryGetAttackIndex(attackType, out int attackIndex))
            {
                return RequestPlayerAttack(attackIndex);
            }

            return RequestPlayerAttack(PlaybackMap.Attack);
        }

        /// <summary>
        ///     プレイヤー攻撃用オプションを付与して再生を要求する。
        /// </summary>
        /// <param name="index"> 再生インデックスです。 </param>
        /// <returns> 現在速度を加味した再生時間です。 </returns>
        private float RequestPlayerAttack(int index)
        {
            return RequestOneShot(
                index,
                skipEnterBlendOnSameClip: true,
                canCancelByMovement: true);
        }

        /// <summary>
        ///     回避アニメーションの再生終了を購読側へ通知する。
        /// </summary>
        internal override void NotifyOneShotEnded()
        {
            OnDodgeEnded?.Invoke();
        }
    }
}
