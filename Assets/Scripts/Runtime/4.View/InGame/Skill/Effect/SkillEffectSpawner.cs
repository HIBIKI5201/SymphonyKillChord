using KillChord.Runtime.Adaptor.InGame.PostEffect;
using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace KillChord.Runtime.View.InGame.Skill.Effect
{
    /// <summary>
    ///     スキルエフェクトをプールから生成して再生するSpawner。
    ///     シーンロード時に装備スキル分のインスタンスを事前生成し、実行時のGCを抑える。
    /// </summary>
    public sealed class SkillEffectSpawner : MonoBehaviour, ISkillEffectSpawner
    {
        /// <summary>
        ///     発動時の全画面演出に必要な依存を注入する。
        /// </summary>
        /// <param name="focusPostEffectPlayer"> 全画面Volumeの再生先です。 </param>
        public void Initialize(IFocusPostEffectPlayer focusPostEffectPlayer)
        {
            _focusPostEffectPlayer = focusPostEffectPlayer;
        }

        /// <summary>
        ///     装備スキルに応じたエフェクトのプールを事前生成する。
        /// </summary>
        /// <param name="equippedSkillIds"> 装備中スキルのID一覧です。 </param>
        public void Prewarm(IReadOnlyList<int> equippedSkillIds)
        {
            if (_catalog == null)
            {
                Debug.LogError($"[{nameof(SkillEffectSpawner)}] {nameof(SkillEffectCatalogConfig)} が未設定です。", this);
                return;
            }

            Clear();

            if (equippedSkillIds == null)
            {
                return;
            }

            for (int i = 0; i < equippedSkillIds.Count; i++)
            {
                int skillId = equippedSkillIds[i];
                if (!_catalog.TryGetPrefab(skillId, out SkillEffectInstance prefab))
                {
                    continue;
                }

                RegisterPrefab(skillId, prefab);
            }
        }

        /// <summary>
        ///     指定スキルのエフェクトを再生する。
        /// </summary>
        /// <param name="skillId"> 再生するスキルのIDです。 </param>
        /// <param name="context"> エフェクトの参照点です。 </param>
        /// <returns> 再生に成功した場合はハンドル、失敗した場合はnull。 </returns>
        public ISkillEffectHandle PlaySkillEffect(int skillId, in SkillEffectContext context)
        {
            if (!_pools.TryGetValue(skillId, out SkillEffectPoolEntry entry))
            {
                Debug.LogError($"[{nameof(SkillEffectSpawner)}] 事前生成されていないスキルIDです。 Id: {skillId}", this);
                return null;
            }

            SkillEffectInstance instance = entry.Pool.Get();

            // 再生が即座に完了しても返却処理が追跡できるよう、開始前に登録する。
            _activeInstances.Add(instance);
            if (instance.Play(context, entry.ReleaseHandler))
            {
                PlayFocusPostEffect(instance);
                return instance;
            }

            // 配置解決に失敗した場合は再生せずに即座に返却する。
            _activeInstances.Remove(instance);
            entry.Pool.Release(instance);
            return null;
        }

        /// <summary>
        ///     再生中のスキルエフェクトをすべて停止する。
        /// </summary>
        public void StopAll()
        {
            _focusPostEffectPlayer?.Stop();

            // 停止はキャンセル要求のため、返却は各インスタンスの完了処理から行われる。
            for (int i = _activeInstances.Count - 1; i >= 0; i--)
            {
                _activeInstances[i]?.Stop();
            }
        }

        /// <summary>
        ///     生成済みのプールをすべて破棄する。
        /// </summary>
        public void Clear()
        {
            StopAll();
            _activeInstances.Clear();
            foreach (KeyValuePair<int, SkillEffectPoolEntry> pair in _pools)
            {
                pair.Value.Pool.Clear();
            }

            _pools.Clear();
        }

        private const int DEFAULT_EFFECT_LAYER = 9;

        [SerializeField, Tooltip("スキルIDとエフェクトプレハブの対応表です。")]
        private SkillEffectCatalogConfig _catalog;

        [SerializeField, Tooltip("生成したエフェクトの親Transformです。未設定時は自身を使用します。")]
        private Transform _instanceRoot;

        [SerializeField, Tooltip("生成したエフェクトへ設定するレイヤーです。Volumeの適用対象から除外されます。")]
        private int _effectLayer = DEFAULT_EFFECT_LAYER;

        /// <summary>
        ///     破棄時にプールを解放する。
        /// </summary>
        private void OnDestroy()
        {
            Clear();
        }

        /// <summary>
        ///     エフェクトプレハブ1件分のプールを生成する。
        /// </summary>
        /// <param name="skillId"> プールを識別するスキルIDです。 </param>
        /// <param name="prefab"> 生成対象のエフェクトプレハブです。 </param>
        private void RegisterPrefab(int skillId, SkillEffectInstance prefab)
        {
            if (prefab == null || _pools.ContainsKey(skillId))
            {
                return;
            }

            SkillEffectPoolEntry entry = new SkillEffectPoolEntry(skillId, prefab, InstantiateInstance, ReleaseInstance);
            _pools.Add(skillId, entry);
            entry.Prewarm();
        }

        /// <summary>
        ///     エフェクトプレハブからインスタンスを生成する。
        /// </summary>
        /// <param name="prefab"> 生成元のエフェクトプレハブです。 </param>
        /// <returns> 生成したインスタンスです。 </returns>
        private SkillEffectInstance InstantiateInstance(SkillEffectInstance prefab)
        {
            Transform parent = _instanceRoot != null ? _instanceRoot : transform;
            SkillEffectInstance instance = Instantiate(prefab, parent);
            instance.name = prefab.name;

            // Volumeの適用対象から外すため、エフェクトは専用レイヤーへ揃える。
            SetLayerRecursively(instance.gameObject, _effectLayer);
            instance.Prewarm();
            return instance;
        }

        /// <summary>
        ///     再生完了したインスタンスをプールへ返却する。
        /// </summary>
        /// <param name="skillId"> 対象のスキルIDです。 </param>
        /// <param name="instance"> 返却するインスタンスです。 </param>
        private void ReleaseInstance(int skillId, SkillEffectInstance instance)
        {
            if (instance == null)
            {
                return;
            }

            _activeInstances.Remove(instance);
            if (_pools.TryGetValue(skillId, out SkillEffectPoolEntry entry))
            {
                entry.Pool.Release(instance);
                return;
            }

            Destroy(instance.gameObject);
        }

        /// <summary>
        ///     エフェクトごとに設定された秒数だけ全画面Volumeを再生する。
        /// </summary>
        /// <param name="instance"> 再生を開始したエフェクトです。 </param>
        private void PlayFocusPostEffect(SkillEffectInstance instance)
        {
            if (_focusPostEffectPlayer == null)
            {
                return;
            }

            float durationSeconds = instance.FocusPostEffectSeconds;
            if (durationSeconds <= 0f)
            {
                return;
            }

            _focusPostEffectPlayer.Play(instance.FocusPostEffectDelaySeconds, durationSeconds);
        }

        /// <summary>
        ///     指定オブジェクトとその子階層すべてへレイヤーを設定する。
        /// </summary>
        /// <param name="target"> 設定対象のオブジェクトです。 </param>
        /// <param name="layer"> 設定するレイヤー番号です。 </param>
        private static void SetLayerRecursively(GameObject target, int layer)
        {
            target.layer = layer;

            Transform targetTransform = target.transform;
            for (int i = 0; i < targetTransform.childCount; i++)
            {
                SetLayerRecursively(targetTransform.GetChild(i).gameObject, layer);
            }
        }

        private readonly Dictionary<int, SkillEffectPoolEntry> _pools = new();
        private readonly List<SkillEffectInstance> _activeInstances = new();
        private IFocusPostEffectPlayer _focusPostEffectPlayer;

        /// <summary>
        ///     エフェクトプレハブ1件分のプールを保持するクラス。
        /// </summary>
        private sealed class SkillEffectPoolEntry
        {
            /// <summary>
            ///     プールエントリを生成する。
            /// </summary>
            /// <param name="skillId"> プールを識別するスキルIDです。 </param>
            /// <param name="prefab"> 対象のエフェクトプレハブです。 </param>
            /// <param name="instantiator"> インスタンス生成処理です。 </param>
            /// <param name="releaser"> インスタンス返却処理です。 </param>
            public SkillEffectPoolEntry(
                int skillId,
                SkillEffectInstance prefab,
                Func<SkillEffectInstance, SkillEffectInstance> instantiator,
                Action<int, SkillEffectInstance> releaser)
            {
                _prefab = prefab;

                // 再生完了コールバックは毎回同じデリゲートを渡し、再生ごとのアロケーションを避ける。
                ReleaseHandler = instance => releaser(skillId, instance);
                Pool = new ObjectPool<SkillEffectInstance>(
                    createFunc: () => instantiator(prefab),
                    actionOnGet: OnGetFromPool,
                    actionOnRelease: OnReleaseToPool,
                    actionOnDestroy: OnDestroyInstance,
                    collectionCheck: true,
                    defaultCapacity: Mathf.Max(1, prefab.PrewarmCount),
                    maxSize: prefab.MaxPoolSize);
            }

            /// <summary> インスタンスのプールです。 </summary>
            public IObjectPool<SkillEffectInstance> Pool { get; }

            /// <summary> 再生完了時に呼ぶ返却処理です。 </summary>
            public Action<SkillEffectInstance> ReleaseHandler { get; }

            /// <summary>
            ///     定義された数だけインスタンスを事前生成する。
            /// </summary>
            public void Prewarm()
            {
                int prewarmCount = _prefab.PrewarmCount;
                if (prewarmCount <= 0)
                {
                    return;
                }

                // 事前生成分を一度取得してから返却し、プール内に保持させる。
                SkillEffectInstance[] buffer = new SkillEffectInstance[prewarmCount];
                for (int i = 0; i < prewarmCount; i++)
                {
                    buffer[i] = Pool.Get();
                }

                for (int i = 0; i < prewarmCount; i++)
                {
                    Pool.Release(buffer[i]);
                }
            }

            /// <summary>
            ///     プールから取り出したインスタンスを有効化する。
            /// </summary>
            /// <param name="instance"> 対象のインスタンスです。 </param>
            private void OnGetFromPool(SkillEffectInstance instance)
            {
                if (instance == null)
                {
                    return;
                }

                instance.gameObject.SetActive(true);
            }

            /// <summary>
            ///     プールへ戻すインスタンスを無効化する。
            /// </summary>
            /// <param name="instance"> 対象のインスタンスです。 </param>
            private void OnReleaseToPool(SkillEffectInstance instance)
            {
                if (instance == null)
                {
                    return;
                }

                instance.gameObject.SetActive(false);
            }

            /// <summary>
            ///     不要になったインスタンスを破棄する。
            /// </summary>
            /// <param name="instance"> 対象のインスタンスです。 </param>
            private void OnDestroyInstance(SkillEffectInstance instance)
            {
                if (instance == null)
                {
                    return;
                }

                UnityEngine.Object.Destroy(instance.gameObject);
            }

            private readonly SkillEffectInstance _prefab;
        }
    }
}
