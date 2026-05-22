using KillChord.Runtime.Adaptor.InGame.Battle;
using KillChord.Runtime.Adaptor.InGame.Camera.Target;
using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.Adaptor.InGame.Player;
using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.Adaptor.InGame.UI;
using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Application.InGame.Camera.Target;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Application.InGame.Player;
using KillChord.Runtime.Application.InGame.Skill;
using KillChord.Runtime.Composition.InGame.Enemy;
using KillChord.Runtime.Composition.InGame.UI;
using KillChord.Runtime.Composition.Persistent.Camera;
using KillChord.Runtime.Composition.Persistent.Input;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Player;
using KillChord.Runtime.InfraStructure.InGame.Character;
using KillChord.Runtime.InfraStructure.InGame.Player;
using KillChord.Runtime.InfraStructure.InGame.Skill;
using KillChord.Runtime.InfraStructure.Player;
using KillChord.Runtime.Utility.Collections;
using KillChord.Runtime.View.InGame.Battle;
using KillChord.Runtime.View.InGame.Player;
using KillChord.Runtime.View.InGame.Skill;
using KillChord.Runtime.View.InGame.UI;
using KillChord.Runtime.View.Persistent.Input;
using SymphonyFrameWork.System.ServiceLocate;
using System.Collections.Generic;
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

        [Space]
        [Header("キャラクターデータ（テスト用）")]
        [SerializeField]
        private CharacterData _playerData;
        [Header("装備中スキル（テスト用）")]
        [SerializeField] private SkillDataAsset[] _equippedSkills;

        private CharacterEntity _playerEntity;
        private MissionEventController _missionEventController;
        private InGameHudInitializer _inGameHudInitializer;

        private void Awake()
        {
            ServiceLocator.RegisterInstance(this);
        }

        public CharacterEntity PlayerEntity => _playerEntity;

        public void Initialize(
            TargetManager targetManager,
            TargetEntityRegistry targetEntityRegistry,
            InputComposition inputComposition)
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

            _missionEventController = ServiceLocator.GetInstance<MissionEventController>();
            if (_missionEventController != null)
            {
                _playerEntity.OnDied += HandlePlayerDied;
            }

            // SerializeFieldの装備スキルからskillId配列を作成する
            List<int> skillIdList = new List<int>();
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

            PlayerDodgeMovementApplication dodge = new(parameter);
            dodge.OnDodgeStarted += (float duration) => _playerEntity.SetInvincible(true);
            dodge.OnDodgeEnded += () => _playerEntity.SetInvincible(false);

            PlayerMovementApplication move = new(parameter);
            PlayerApplication application = new(move, dodge);

            PlayerController playerMovementController = new(application, inputComposition.GetBufferedInputBuffer);
            var ct = ServiceLocator.GetInstance<ICameraTransform>().Transform;
            var inputView = ServiceLocator.GetInstance<PlayerInputView>();


            TargetSelectorController targetSelectorController = ServiceLocator.GetInstance<TargetSelectorController>();
            if (targetSelectorController == null)
            {
                Debug.LogError($"{nameof(TargetSelectorController)}が見つかりません。シーン内に配置されていることを確認してください。", this);
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
            SkillInputProgressViewconfig inputProgressViewConfig = _inputProgressViewConfigAsset.Create();

            SkillInputProgressViewModel inputProgressViewModel =
                new SkillInputProgressViewModel(inputProgressViewConfig);
            SkillInputProgressPresenter inputProgressPresenter =
                new SkillInputProgressPresenter(inputProgressViewModel);
            SkillInputProgressState inputProgressState =
                new SkillInputProgressState();
            SkillInputProgressUsecase inputProgressUsecase =
                new SkillInputProgressUsecase();
            SkillInputProgressController inputProgressController =
                new SkillInputProgressController(
                    inputProgressUsecase,
                    inputProgressState,
                    inputProgressPresenter);
            SkillInputProgressView skillInputProgressView =
                FindAnyObjectByType<SkillInputProgressView>();

            skillInputProgressView?.Bind(inputProgressViewModel);

            // 仮でシーン内のSkillResultViewを見つけて、ViewModelをバインド
            SkillResultView skillResultView = FindAnyObjectByType<SkillResultView>();
            skillResultView?.Bind(skillResultViewModel);

            SkillCheckService skillCheckService = new SkillCheckService();
            SkillController skillController = new SkillController(_skillRepository, _skillVisuals, skillIds, skillResultPresenter, inputProgressController);
            SkillUsecase skillUsecase = new SkillUsecase(musicSyncService, skillCheckService, skillController);
            skillController?.SetUsecase(skillUsecase);


            AttackResultViewModel attackResultViewModel = new AttackResultViewModel();
            AttackResultPresenter attackResultPresenter = new AttackResultPresenter(attackResultViewModel);

            PlayerBattleState playerBattleState = new PlayerBattleState(_playerEntity);
            AttackIntervalEvaluator attackIntervalEvaluator = new AttackIntervalEvaluator(_playerEntity.AttackIntervalEntity);

            PlayerAttackController playerAttackController = new PlayerAttackController(attackResultPresenter,
                playerBattleState, skillController, targetSelectorController, attackIntervalEvaluator, musicSyncService);

            IHealthHudViewModel healthHudViewModel = new HealthHudViewModel(_playerEntity.CurrentHealth.Value, _playerEntity.MaxHealth.Value);
            PlayerHealthHudPresenter healthHudPresenter = new PlayerHealthHudPresenter(_playerEntity, healthHudViewModel);

            _player.Initialize(playerMovementController, playerAttackController, ct, inputView, healthHudPresenter);

            _inGameHudInitializer.InitializePlayerHpHud(healthHudViewModel);

#if UNITY_EDITOR
            _player.gameObject
                .AddComponent<PlayerMoveParameterDebug>()
                .SetPlayerMoveParameter(parameter);
#endif
        }

        private void HandlePlayerDied(CharacterEntity _)
        {
            _missionEventController?.NotifyPlayerDead();
        }

        private void OnDestroy()
        {
            if (_playerEntity != null)
            {
                _playerEntity.OnDied -= HandlePlayerDied;
            }
        }
    }
}
