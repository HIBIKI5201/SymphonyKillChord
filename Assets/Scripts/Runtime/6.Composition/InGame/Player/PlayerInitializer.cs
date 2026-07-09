using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Adaptor.InGame.Battle;
using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Adaptor.InGame.Player;
using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.Adaptor.InGame.Target;
using KillChord.Runtime.Adaptor.InGame.UI;
using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Application.InGame.Player;
using KillChord.Runtime.Application.InGame.Skill;
using KillChord.Runtime.Composition.InGame.Skill;
using KillChord.Runtime.Composition.InGame.UI;
using KillChord.Runtime.Composition.Persistent.Camera;
using KillChord.Runtime.Composition.Persistent.Input;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Player;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.OutGame.SkillBuild;
using KillChord.Runtime.Domain.Player;
using KillChord.Runtime.InfraStructure;
using KillChord.Runtime.InfraStructure.InGame.Character;
using KillChord.Runtime.InfraStructure.InGame.Player;
using KillChord.Runtime.InfraStructure.InGame.Skill;
using KillChord.Runtime.InfraStructure.Player;
using KillChord.Runtime.Utility.Collections;
using KillChord.Runtime.View;
using KillChord.Runtime.View.InGame.Battle;
using KillChord.Runtime.View.InGame.Player;
using KillChord.Runtime.View.InGame.Skill;
using KillChord.Runtime.View.InGame.UI;
using KillChord.Runtime.View.Persistent.Input;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
#endif

namespace KillChord.Runtime.Composition.InGame.Player
{
    /// <summary>
    ///     プレイヤーに関するクラスの生成と依存関係の解決を行う初期化クラス。
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrderConst.INITIALIZATION)]
    public sealed class PlayerInitializer : MonoBehaviour
    {
        [SerializeField] private PlayerConfig _playerConfig;
        [SerializeField] private PlayerView _player;
        [SerializeField] private SkillRepository _skillRepository;
        [SerializeField] private SkillView[] _skillVisuals;
        [SerializeField] private SkillInputProgressViewConfigAsset _inputProgressViewConfigAsset;
        [SerializeField] private CharacterAnimationView _characterAnimationView;
        [SerializeField] private CharacterAnimationCatalogAsset _characterAnimationCatalogAsset;

        [Space]
        [Header("キャラクターデータ（テスト用）")]
        [SerializeField]
        private CharacterData _playerData;
        [Header("装備中スキル（テスト用）")]
        [SerializeField] private SkillDataAsset[] _equippedSkills;

        private Action<int> _onOneShotEndedHandler;
        private CharacterEntity _playerEntity;
        private MissionEventController _missionEventController;
        private InGameHudInitializer _inGameHudInitializer;
        private SkillInputProgressUIInitializer _skillInputProgressUIInitializer;

        private void Awake()
        {
            ServiceLocator.RegisterInstance(this, LocateType.Locator);
        }

        public CharacterEntity PlayerEntity => _playerEntity;

        public void Initialize(InputComposition inputComposition)
        {
            if (_player == null)
                Debug.LogError($"{nameof(PlayerView)}がNullです", this);

            _inGameHudInitializer = ServiceLocator.GetInstance<InGameHudInitializer>();
            if (_inGameHudInitializer == null)
            {
                Debug.LogError($"{nameof(InGameHudInitializer)}が見つかりません。シーン内に配置されていることを確認してください。", this);
                return;
            }

            _playerEntity = CharacterFactory.Create(_playerData);
            _playerEntity.OnDamageAvoided += HandleDamageAvoided;

            _missionEventController = ServiceLocator.GetInstance<MissionEventController>();
            if (_missionEventController != null)
            {
                _playerEntity.OnDied += HandlePlayerDied;
            }

            MusicSyncState musicSyncState = ServiceLocator.GetInstance<MusicSyncState>();
            if (musicSyncState == null)
            {
                Debug.LogError($"{nameof(MusicSyncState)}が見つかりません。ServiceLocatorに登録されているか確認してください。", this);
                return;
            }

            _skillInputProgressUIInitializer = ServiceLocator.GetInstance<SkillInputProgressUIInitializer>();
            if (_skillInputProgressUIInitializer == null)
            {
                Debug.LogError($"{nameof(SkillInputProgressUIInitializer)}が見つかりません。ServiceLocatorに登録されているか確認してください。", this);
                return;
            }

            List<int> skillIdList = new List<int>();
            // SerializeFieldの装備スキルからskillId配列を作成する
            if (_equippedSkills != null && _equippedSkills.Length > 0)
            {
                for (int i = 0; i < _equippedSkills.Length; i++)
                {
                    if (_equippedSkills[i] != null)
                    {
                        skillIdList.Add(_equippedSkills[i].Id);
                    }
                }
            }
            int[] skillIds = null;
            if (skillIdList.Count > 0)
            {
                skillIds = skillIdList.ToArray();
            }

            PlayerMoveParameter parameter = _playerConfig.ToDomain();

            var ct = ServiceLocator.GetInstance<ICameraTransform>().Transform;
            var inputView = ServiceLocator.GetInstance<PlayerInputView>();


            TargetSystemController targetingSystem = ServiceLocator.GetInstance<TargetSystemController>();
            if (targetingSystem == null)
            {
                Debug.LogError($"{nameof(TargetSystemController)}が見つかりません。シーン内に配置されていることを確認してください。", this);
                return;
            }

            IMusicSyncService musicSyncService = ServiceLocator.GetInstance<IMusicSyncService>();
            if (musicSyncService == null)
            {
                Debug.LogError($"{nameof(IMusicSyncService)}が見つかりません。MusicSyncInitializerが先に実行されているか確認してください。");
                return;
            }

            SkillResultViewModel skillResultViewModel = new SkillResultViewModel();
            Debug.Log($"{skillResultViewModel}作成。");
            SkillResultPresenter skillResultPresenter = new SkillResultPresenter(skillResultViewModel);
            Debug.Log($"{skillResultPresenter}作成。");

            if (_inputProgressViewConfigAsset == null)
            {
                Debug.LogError($"{nameof(SkillInputProgressViewConfigAsset)}がNullです", this);
                return;
            }

            // 仮でシーン内のSkillResultViewを見つけて、ViewModelをバインド
            SkillResultView skillResultView = FindAnyObjectByType<SkillResultView>();
            skillResultView?.Bind(skillResultViewModel);

            SkillCheckService skillCheckService = new SkillCheckService();

            SkillUsecase skillUsecase = new SkillUsecase(targetingSystem, _playerEntity);
            SkillController skillController = new SkillController(musicSyncService);
            skillController.Initialize(BuildSkillExecutionControllers(musicSyncState, skillResultPresenter, skillCheckService, skillUsecase, musicSyncService));

            AttackResultViewModel attackResultViewModel = new AttackResultViewModel();
            AttackResultPresenter attackResultPresenter = new AttackResultPresenter(attackResultViewModel);

            PlayerBattleState playerBattleState = new PlayerBattleState(_playerEntity);
            AttackIntervalEvaluator attackIntervalEvaluator = new AttackIntervalEvaluator(_playerEntity.AttackIntervalEntity);

            PlayerAttackController playerAttackController = new PlayerAttackController(attackResultPresenter,
                playerBattleState, skillController, targetingSystem, attackIntervalEvaluator, musicSyncService, musicSyncState, (float)parameter.AttackRotationSpeed, (float)parameter.AttackCooldown.Value, (int)_playerEntity.BaseDamage.Value);

            IHealthHudViewModel healthHudViewModel = new HealthHudViewModel(_playerEntity.CurrentHealth.Value, _playerEntity.MaxHealth.Value);
            PlayerHealthHudPresenter healthHudPresenter = new PlayerHealthHudPresenter(_playerEntity, healthHudViewModel);

            var animationComposition = _player.gameObject.AddComponent<AnimationComposition>();
            var animController = animationComposition.Init(_characterAnimationView, _characterAnimationCatalogAsset, musicSyncState, out CharacterAnimationIndices animationIndices);

            PlayerDodgeMovementApplication dodge = new(parameter);
            dodge.OnDodgeStarted += (float duration) => _playerEntity.SetInvincible(true);
            dodge.OnDodgeEnded += () => _playerEntity.SetInvincible(false);

            _onOneShotEndedHandler = index =>
            {
                if (index == animationIndices.Dodge)
                {
                    playerAttackController.StartAttackCooldown();
                }
            };

            _characterAnimationView.OnOneShotEnded += _onOneShotEndedHandler;

            PlayerMovementApplication move = new(parameter);
            PlayerApplication application = new(move, dodge);

            PlayerController playerMovementController = new(application, inputComposition.GetBufferedInputBuffer);

            _player.Initialize(playerMovementController, playerAttackController, animController, animationIndices, ct, inputView, healthHudPresenter);


            _inGameHudInitializer.InitializePlayerHpHud(healthHudViewModel);

#if UNITY_EDITOR
            _player.gameObject
                .AddComponent<PlayerMoveParameterDebug>()
                .SetPlayerMoveParameter(parameter);
#endif
        }

        /// <summary>
        ///     装備中1スキル分のモジュール一式を構築する。
        /// </summary>
        /// <param name="musicSyncState"></param>
        /// <param name="skillResultPresenter"></param>
        /// <param name="checkService"></param>
        /// <param name="usecase"></param>
        /// <param name="musicSyncService"></param>
        /// <returns></returns>
        private SkillExecutionController[] BuildSkillExecutionControllers(MusicSyncState musicSyncState, SkillResultPresenter skillResultPresenter, SkillCheckService checkService, SkillUsecase usecase, IMusicSyncService musicSyncService)
        {
            if (_equippedSkills == null || _equippedSkills.Length == 0)
            {
                return System.Array.Empty<SkillExecutionController>();
            }

            List<SkillData> skillData = new List<SkillData>();

            if (ServiceLocator.TryGetInstance(out SkillBuildDefinition buildDefinition))
            {
                // SkillBuildDefinitionからskillId配列を作成する
                if (buildDefinition.EquippedSkills != null && buildDefinition.EquippedSkills.Count > 0)
                {
                    for (int i = 0; i < buildDefinition.EquippedSkills.Count; i++)
                    {
                        if (buildDefinition.EquippedSkills[i].HasSkill)
                        {
                            skillData.Add(buildDefinition.EquippedSkills[i].SkillData);
                        }
                    }
                }
            }

            List<SkillExecutionController> executionControllers = new List<SkillExecutionController>(skillData.Count);
            for (int i = 0; i < skillData.Count; i++)
            {
                if (skillData[i] == null) { continue; }

                SkillData data = skillData[i];
                SkillDefinition definition = data.ToSkillDefinition(musicSyncState.Bpm);
                SkillCooldownState cooldownState = new SkillCooldownState(definition);
                SkillView view = _skillVisuals.FirstOrDefault(n => n.Id == definition.Id.Value);
                if (view == null) { continue; }
                SkillRhythmState rhythmState = new SkillRhythmState(definition.SkillPattern.Signatures.Length * 2);

                SkillInputProgressController progressController = BuildSkillProgressModules(definition);

                SkillExecutionController executionController = new SkillExecutionController(skillResultPresenter, progressController, cooldownState, usecase, checkService, view, definition, rhythmState);
                executionControllers.Add(executionController);
            }
            return executionControllers.ToArray();
        }

        /// <summary>
        ///     装備中1スキル分の入力進捗UIモジュール一式を構築する。
        /// </summary>
        /// <param name="definition"></param>
        /// <returns></returns>
        private SkillInputProgressController BuildSkillProgressModules(SkillDefinition definition)
        {
            ISkillInputProgressRowView rowView = _skillInputProgressUIInitializer.CreateInputProgressRow(definition);
            SkillInputProgressPresenter presenter = new SkillInputProgressPresenter(rowView);
            SkillInputProgressState state = new SkillInputProgressState(definition);
            SkillInputProgressController controller = new SkillInputProgressController(state, presenter);
            return controller;
        }

        private void HandleDamageAvoided(Damage damage)
        {
            _player?.PlayDodgeSuccessFeedback();
        }

        private void HandlePlayerDied(CharacterEntity _)
        {
            _missionEventController?.NotifyPlayerDead();
        }

        private void OnDestroy()
        {
            if (_characterAnimationView != null && _onOneShotEndedHandler != null)
            {
                _characterAnimationView.OnOneShotEnded -= _onOneShotEndedHandler;
                _onOneShotEndedHandler = null;
            }

            ServiceLocator.UnregisterInstance(this);

            if (_playerEntity != null)
            {
                _playerEntity.OnDied -= HandlePlayerDied;
                _playerEntity.OnDamageAvoided -= HandleDamageAvoided;

            }
        }
    }
}
