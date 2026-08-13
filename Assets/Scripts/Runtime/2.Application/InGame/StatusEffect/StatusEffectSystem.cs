using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.StatusEffect;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.StatusEffect
{
    /// <summary>
    ///     キャラクターへ付与された状態効果を管理するシステムの実装。
    /// </summary>
    public class StatusEffectSystem : IStatusEffectSystem
    {
        public StatusEffectSystem(Func<float> timeProvider = null)
        {
            _timeProvider = timeProvider ?? GetCurrentTime;
        }

        /// <inheritdoc />
        public void Add(IStatusEffect statusEffect)
        {
            if (statusEffect == null)
            {
                throw new ArgumentNullException(nameof(statusEffect));
            }

            float currentTime = _timeProvider();

            RemoveExpiredEffects(currentTime);

            // 別効果なら状態効果と時間を追加する
            if (statusEffect.ReapplyPolicy == StatusEffectReapplyPolicy.Stack)
            {
                _statusEffects.Add(new StatusEffectRuntimeEntity(statusEffect, currentTime));
                Debug.Log($"{statusEffect.Id}を重複で付与" +
                          $"{statusEffect.ReapplyPolicy}で処理" +
                          $"{statusEffect.Duration.Seconds}秒の継続時間");
                return;
            }

            // 同じ効果が存在する場合は、再付与ポリシーに従って処理する。
            int index = FindEffectIndex(statusEffect.Id);
            if (index < 0)
            {
                _statusEffects.Add(new StatusEffectRuntimeEntity(statusEffect, currentTime));
                Debug.Log($"{statusEffect.Id}を新規で付与" +
                          $"{statusEffect.ReapplyPolicy}で処理" +
                          $"{statusEffect.Duration.Seconds}秒の継続時間");
                return;
            }

            StatusEffectRuntimeEntity runtime = _statusEffects[index];
            float beforeDuration = runtime.GetRemainingDuration(currentTime);

            // 再付与ポリシーに従って処理する。
            switch (statusEffect.ReapplyPolicy)
            {
                case StatusEffectReapplyPolicy.ExtendDuration:
                    runtime.ExtendDuration(statusEffect.Duration);
                    break;

                case StatusEffectReapplyPolicy.RefreshDuration:
                    runtime.RefreshDuration(statusEffect.Duration, currentTime);
                    break;

                case StatusEffectReapplyPolicy.Replace:
                    runtime.Replace(statusEffect, currentTime);
                    break;

                case StatusEffectReapplyPolicy.Ignore:
                    Debug.Log($"{statusEffect.Id}の再付与を無視" +
                              $"Until removed {beforeDuration}");
                    return;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(statusEffect),
                        statusEffect.ReapplyPolicy,
                        "未対応の状態再付与ルールです。");
            }

            float afterDuration = runtime.GetRemainingDuration(currentTime);
            Debug.Log($"{statusEffect.Id}の再付与を処理" +
                      $"Before: {beforeDuration}, After: {afterDuration}");
        }

        /// <inheritdoc />
        public void Remove(IStatusEffect statusEffect)
        {
            if (statusEffect == null)
            {
                return;
            }

            for (int i = _statusEffects.Count - 1; i >= 0; i--)
            {
                // 参照が同じかどうかを確認するためにReferenceEqualsを使用して、
                // 同じインスタンスの状態効果のみを削除する
                if (!ReferenceEquals(_statusEffects[i].Effect, statusEffect))
                {
                    continue;
                }

                _statusEffects.RemoveAt(i);
                return;
            }
        }

        /// <inheritdoc />
        public void Clear()
        {
            _statusEffects.Clear();
        }

        /// <inheritdoc />
        public AttackResult ApplyIncomingDamageModifiers(
            IAttacker attacker,
            IDefender defender,
            AttackResult attackResult)
        {
            float currentTime = _timeProvider();

            // 現在の時間を取得して、期限切れの状態効果を削除する
            RemoveExpiredEffects(currentTime);

            AttackResult modifiedResult = attackResult;

            // 状態効果のリストをループして、各実装しているかどうかを確認する
            for (int i = 0; i < _statusEffects.Count; i++)
            {
                if (_statusEffects[i].Effect is not IIncomingDamageModifier modifier)
                {
                    continue;
                }

                // 実装している場合,メソッドを呼び出してダメージを修正する
                modifiedResult = modifier.ModifyIncomingDamage(attacker, defender, modifiedResult);
            }

            return modifiedResult;
        }

        /// <inheritdoc />
        public AttackResult ApplyOutgoingDamageModifiers(
            IAttacker attacker,
            IDefender defender,
            AttackResult attackResult)
        {
            float currentTime = _timeProvider();

            RemoveExpiredEffects(currentTime);

            AttackResult modifiedResult = attackResult;

            // incomingと同様に、状態効果のリストをループして、各実装しているかどうかを確認する
            for (int i = 0; i < _statusEffects.Count; i++)
            {
                if (_statusEffects[i].Effect is not IOutgoingDamageModifier modifier)
                {
                    continue;
                }

                modifiedResult = modifier.ModifyOutgoingDamage(attacker, defender, modifiedResult);
            }

            return modifiedResult;
        }

        /// <summary>
        ///     Idが同じ状態効果をリストから検索し、インデックスを返す。
        ///     見つからない場合は-1を返す。
        /// </summary>
        /// <param name="id"> 検索する状態効果のID。 </param>
        /// <returns> 状態効果のインデックス、見つからない場合は-1。 </returns>
        private int FindEffectIndex(StatusEffectId id)
        {
            for (int i = 0; i < _statusEffects.Count; i++)
            {
                if (_statusEffects[i].Effect.Id == id)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        ///     現在の時間を取得し、期限切れの状態効果を削除する。
        /// </summary>
        /// <param name="currentTime"> 現在の時間。 </param>
        private void RemoveExpiredEffects(float currentTime)
        {
            for (int i = _statusEffects.Count - 1; i >= 0; i--)
            {
                if (_statusEffects[i].IsExpired(currentTime))
                {
                    Debug.Log($"{_statusEffects[i].Effect.Id}の状態効果が期限切れのため削除されました。");
                    _statusEffects.RemoveAt(i);
                }
            }
        }

        /// <inheritdoc />
        public void NotifyDamageDealt(in DamageDealtContext context)
        {
            float currentTime = _timeProvider();

            RemoveExpiredEffects(currentTime);

            for (int i = 0; i < _statusEffects.Count; i++)
            {
                if (_statusEffects[i].Effect is IDamageDealtHandler handler)
                {
                    handler.OnDamageDealt(context);
                }
            }
        }

        /// <summary>
        ///     現在の時間を取得する。
        /// </summary>
        /// <returns> 現在の時間。 </returns>
        private static float GetCurrentTime()
        {
            return Time.time;
        }

        private readonly Func<float> _timeProvider;
        private readonly List<StatusEffectRuntimeEntity> _statusEffects = new();
    }
}
