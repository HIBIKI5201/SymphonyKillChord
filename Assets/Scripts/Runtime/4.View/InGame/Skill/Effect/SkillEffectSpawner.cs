using KillChord.Runtime.Adaptor.InGame.Skill.Effect;
using KillChord.Runtime.View.InGame.Skill.Effect.Placement;
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

            // 装備状況に関わらず使用する共通エフェクトを先に生成する。
            RegisterDefinitions(_catalog.CommonDefinitions);

            if (equippedSkillIds == null)
            {
                return;
            }

            for (int i = 0; i < equippedSkillIds.Count; i++)
            {
                if (!_catalog.TryGetDefinitions(equippedSkillIds[i], out IReadOnlyList<SkillEffectDefinitionConfig> definitions))
                {
                    continue;
                }

                RegisterDefinitions(definitions);
            }
        }

        /// <summary>
        ///     指定IDのスキルエフェクトを再生する。
        /// </summary>
        /// <param name="effectId"> 再生するエフェクトのIDです。 </param>
        /// <param name="context"> エフェクトの参照点です。 </param>
        /// <returns> 再生に成功した場合はハンドル、失敗した場合はnull。 </returns>
        public ISkillEffectHandle Play(SkillEffectId effectId, in SkillEffectContext context)
        {
            if (!_pools.TryGetValue(effectId, out SkillEffectPoolEntry entry))
            {
                Debug.LogError($"[{nameof(SkillEffectSpawner)}] 事前生成されていないエフェクトIDです。 Id: {effectId}", this);
                return null;
            }

            SkillEffectInstance instance = entry.Pool.Get();
            if (instance.Play(entry.Placement, context, entry.ReleaseHandler))
            {
                _activeInstances.Add(instance);
                return instance;
            }

            // 配置解決に失敗した場合は再生せずに即座に返却する。
            entry.Pool.Release(instance);
            return null;
        }

        /// <summary>
        ///     指定スキルに紐づくスキルエフェクトをすべて再生する。
        /// </summary>
        /// <param name="skillId"> 再生するスキルのIDです。 </param>
        /// <param name="context"> エフェクトの参照点です。 </param>
        public void PlaySkillEffects(int skillId, in SkillEffectContext context)
        {
            if (_catalog == null
                || !_catalog.TryGetDefinitions(skillId, out IReadOnlyList<SkillEffectDefinitionConfig> definitions))
            {
                return;
            }

            for (int i = 0; i < definitions.Count; i++)
            {
                SkillEffectDefinitionConfig definition = definitions[i];
                if (definition == null || !definition.IsValid)
                {
                    continue;
                }

                Play(definition.Id, context);
            }
        }

        /// <summary>
        ///     再生中のスキルエフェクトをすべて停止する。
        /// </summary>
        public void StopAll()
        {
            for (int i = _activeInstances.Count - 1; i >= 0; i--)
            {
                _activeInstances[i]?.Stop();
            }

            _activeInstances.Clear();
        }

        /// <summary>
        ///     生成済みのプールをすべて破棄する。
        /// </summary>
        public void Clear()
        {
            StopAll();
            foreach (KeyValuePair<SkillEffectId, SkillEffectPoolEntry> pair in _pools)
            {
                pair.Value.Pool.Clear();
            }

            _pools.Clear();
        }

        [SerializeField, Tooltip("スキルIDとエフェクト定義の対応表です。")]
        private SkillEffectCatalogConfig _catalog;

        [SerializeField, Tooltip("生成したエフェクトの親Transformです。未設定時は自身を使用します。")]
        private Transform _instanceRoot;

        /// <summary>
        ///     破棄時にプールを解放する。
        /// </summary>
        private void OnDestroy()
        {
            Clear();
        }

        /// <summary>
        ///     エフェクト定義一覧のプールを生成する。
        /// </summary>
        /// <param name="definitions"> 生成対象のエフェクト定義一覧です。 </param>
        private void RegisterDefinitions(IReadOnlyList<SkillEffectDefinitionConfig> definitions)
        {
            if (definitions == null)
            {
                return;
            }

            for (int i = 0; i < definitions.Count; i++)
            {
                RegisterDefinition(definitions[i]);
            }
        }

        /// <summary>
        ///     エフェクト定義1件分のプールを生成する。
        /// </summary>
        /// <param name="definition"> 生成対象のエフェクト定義です。 </param>
        private void RegisterDefinition(SkillEffectDefinitionConfig definition)
        {
            if (definition == null || !definition.IsValid)
            {
                return;
            }

            SkillEffectId effectId = definition.Id;
            if (_pools.ContainsKey(effectId))
            {
                return;
            }

            SkillEffectPoolEntry entry = new SkillEffectPoolEntry(definition, InstantiateInstance, ReleaseInstance);
            _pools.Add(effectId, entry);
            entry.Prewarm();
        }

        /// <summary>
        ///     エフェクト定義からインスタンスを生成する。
        /// </summary>
        /// <param name="definition"> 生成元のエフェクト定義です。 </param>
        /// <returns> 生成したインスタンスです。 </returns>
        private SkillEffectInstance InstantiateInstance(SkillEffectDefinitionConfig definition)
        {
            Transform parent = _instanceRoot != null ? _instanceRoot : transform;
            SkillEffectInstance instance = Instantiate(definition.Prefab, parent);
            instance.name = definition.Prefab.name;
            instance.Prewarm();
            return instance;
        }

        /// <summary>
        ///     再生完了したインスタンスをプールへ返却する。
        /// </summary>
        /// <param name="effectId"> 対象のエフェクトIDです。 </param>
        /// <param name="instance"> 返却するインスタンスです。 </param>
        private void ReleaseInstance(SkillEffectId effectId, SkillEffectInstance instance)
        {
            if (instance == null)
            {
                return;
            }

            _activeInstances.Remove(instance);
            if (_pools.TryGetValue(effectId, out SkillEffectPoolEntry entry))
            {
                entry.Pool.Release(instance);
                return;
            }

            Destroy(instance.gameObject);
        }

        private readonly Dictionary<SkillEffectId, SkillEffectPoolEntry> _pools = new();
        private readonly List<SkillEffectInstance> _activeInstances = new();

        /// <summary>
        ///     エフェクト定義1件分のプールと配置ストラテジーを保持するクラス。
        /// </summary>
        private sealed class SkillEffectPoolEntry
        {
            /// <summary>
            ///     プールエントリを生成する。
            /// </summary>
            /// <param name="definition"> 対象のエフェクト定義です。 </param>
            /// <param name="instantiator"> インスタンス生成処理です。 </param>
            /// <param name="releaser"> インスタンス返却処理です。 </param>
            public SkillEffectPoolEntry(
                SkillEffectDefinitionConfig definition,
                Func<SkillEffectDefinitionConfig, SkillEffectInstance> instantiator,
                Action<SkillEffectId, SkillEffectInstance> releaser)
            {
                _definition = definition;
                SkillEffectId effectId = definition.Id;

                // 再生完了コールバックは毎回同じデリゲートを渡し、再生ごとのアロケーションを避ける。
                ReleaseHandler = instance => releaser(effectId, instance);
                Placement = SkillEffectPlacementResolver.Resolve(definition.AttachMode);
                Pool = new ObjectPool<SkillEffectInstance>(
                    createFunc: () => instantiator(definition),
                    actionOnGet: OnGetFromPool,
                    actionOnRelease: OnReleaseToPool,
                    actionOnDestroy: OnDestroyInstance,
                    collectionCheck: true,
                    defaultCapacity: Mathf.Max(1, definition.PrewarmCount),
                    maxSize: definition.MaxPoolSize);
            }

            /// <summary> インスタンスのプールです。 </summary>
            public IObjectPool<SkillEffectInstance> Pool { get; }

            /// <summary> 配置ストラテジーです。 </summary>
            public ISkillEffectPlacement Placement { get; }

            /// <summary> 再生完了時に呼ぶ返却処理です。 </summary>
            public Action<SkillEffectInstance> ReleaseHandler { get; }

            /// <summary>
            ///     定義された数だけインスタンスを事前生成する。
            /// </summary>
            public void Prewarm()
            {
                int prewarmCount = _definition.PrewarmCount;
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

            private readonly SkillEffectDefinitionConfig _definition;
        }
    }
}
