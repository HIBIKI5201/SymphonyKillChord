using KillChord.Runtime.Composition.InGame.Bootstrap;
using KillChord.Runtime.Composition.InGame.Player;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.OutGame.SkillBuild;
using KillChord.Runtime.Utility.Identity;
using KillChord.Runtime.View.InGame.Skill.Effect;
using SymphonyFrameWork.System.ServiceLocate;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Skill.Effect
{
    /// <summary>
    ///     スキルエフェクトのプールをシーンロード時に構築して公開するモジュール。
    /// </summary>
    public sealed class SkillEffectInitializer : InGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(SkillEffectInitializer);

        /// <summary> 実行順です。スキルモジュールより先に構築します。 </summary>
        public override int Order => MODULE_ORDER;

        /// <summary>
        ///     Spawnerを公開するContainerを登録する。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Build()
        {
            if (_skillEffectSpawner == null)
            {
                Debug.LogError($"[{nameof(SkillEffectInitializer)}] {nameof(SkillEffectSpawner)} が未設定です。", this);
                return false;
            }

            _moduleContainer = new SkillEffectModuleContainer(_skillEffectSpawner);
            ServiceLocator.RegisterInstance(_moduleContainer);
            _isRegistered = true;
            return true;
        }

        /// <summary>
        ///     装備スキルを解決し、エフェクトのインスタンスを事前生成する。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Ready()
        {
            _skillEffectSpawner.Prewarm(ResolveEquippedSkillIds());
            return true;
        }

        /// <summary>
        ///     プールを破棄し、登録済みサービスを解除する。
        /// </summary>
        public override void Shutdown()
        {
            if (_skillEffectSpawner != null)
            {
                _skillEffectSpawner.Clear();
            }

            _equippedSkillIdBuffer.Clear();

            if (!_isRegistered)
            {
                return;
            }

            ServiceLocator.UnregisterInstance<SkillEffectModuleContainer>();
            _moduleContainer = null;
            _isRegistered = false;
        }

        private const int MODULE_ORDER = 440;

        [SerializeField, Tooltip("スキルエフェクトのプールを管理するSpawnerです。")]
        private SkillEffectSpawner _skillEffectSpawner;

        [SerializeField, SourceDataCollection("Skill")]
        [Tooltip("テスト用の装備スキルID一覧です。改造画面・Player側設定のどちらも解決できない場合に使用します。")]
        private DataID[] _fallbackEquippedSkills;

        /// <summary>
        ///     事前生成対象となる装備スキルID一覧を解決する。
        /// </summary>
        /// <returns> 装備スキルのID一覧です。 </returns>
        private IReadOnlyList<int> ResolveEquippedSkillIds()
        {
            _equippedSkillIdBuffer.Clear();

            // 改造画面を経由した場合は、そのビルド内容を最優先で使用する。
            if (ServiceLocator.TryGetInstance(out SkillBuildDefinition buildDefinition)
                && buildDefinition.EquippedSkills != null)
            {
                IReadOnlyList<EquippedSkill> equippedSkills = buildDefinition.EquippedSkills;
                for (int i = 0; i < equippedSkills.Count; i++)
                {
                    if (!equippedSkills[i].HasSkill)
                    {
                        continue;
                    }

                    AddSkillId(equippedSkills[i].SkillTemplate.Id.Value);
                }
            }

            if (_equippedSkillIdBuffer.Count > 0)
            {
                return _equippedSkillIdBuffer;
            }

            // 改造画面を経由していない場合は、Player側のテスト装備設定を使用する。
            PlayerModuleContainer playerModuleContainer = ServiceLocator.GetInstance<PlayerModuleContainer>();
            SkillId[] playerSkillIds = playerModuleContainer?.PlayerInitializer?.EquippedSkillIds;
            if (playerSkillIds != null)
            {
                for (int i = 0; i < playerSkillIds.Length; i++)
                {
                    AddSkillId(playerSkillIds[i].Value);
                }
            }

            if (_equippedSkillIdBuffer.Count > 0)
            {
                return _equippedSkillIdBuffer;
            }

            if (_fallbackEquippedSkills == null)
            {
                return _equippedSkillIdBuffer;
            }

            for (int i = 0; i < _fallbackEquippedSkills.Length; i++)
            {
                AddSkillId(_fallbackEquippedSkills[i].Id);
            }

            return _equippedSkillIdBuffer;
        }

        /// <summary>
        ///     重複を避けて装備スキルIDを追加する。
        /// </summary>
        /// <param name="skillId"> 追加するスキルIDです。 </param>
        private void AddSkillId(int skillId)
        {
            if (skillId == 0 || _equippedSkillIdBuffer.Contains(skillId))
            {
                return;
            }

            _equippedSkillIdBuffer.Add(skillId);
        }

        private readonly List<int> _equippedSkillIdBuffer = new();
        private SkillEffectModuleContainer _moduleContainer;
        private bool _isRegistered;
    }
}
