using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;
using KillChord.Runtime.InfraStructure;
using KillChord.Runtime.View;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    /// <summary>
    ///     バトルで使用するクラスのインスタンスを生成し、依存関係を解決して結びつけるクラス。
    /// </summary>
    public sealed class BattleCompositionInitializer : MonoBehaviour
    {
        [Header("キャラクターデータ")]
        [SerializeField] private CharacterData _playerData;
        [SerializeField] private CharacterData _enemyData;

        [Header("アタックパイプライン")]
        [SerializeField] private AttackPipelineAsset _normalPipelineAsset;
        [SerializeField] private AttackPipelineAsset _skillAPipelineAsset;
        [SerializeField] private AttackPipelineAsset _skillBPipelineAsset;
        [SerializeField] private AttackPipelineAsset _ultimatePipelineAsset;

        [Header("View")]
        [SerializeField] private PlayerAttackInputView _playerAttackInputView;
        [SerializeField] private AttackResultView _attackResultView;

        private void Awake()
        {
            CharacterFactory characterFactory = new CharacterFactory();

            CharacterEntity player = characterFactory.Create(_playerData);
            CharacterEntity enemy = characterFactory.Create(_enemyData);

            Dictionary<AttackId, AttackPipeline> pipelines = new Dictionary<AttackId, AttackPipeline>
            {
                { AttackId.Normal, _normalPipelineAsset.Create() },
                { AttackId.SkillA, _skillAPipelineAsset.Create() },
                { AttackId.SkillB, _skillBPipelineAsset.Create() },
                { AttackId.Ultimate, _ultimatePipelineAsset.Create() },
            };

            IAttackPipelineResolver attackPipelineResolver = new AttackPipelineResolver(pipelines);
            AttackExecutor attackExecutor = new AttackExecutor(attackPipelineResolver);

            AttackCommandState attackCommandState = new AttackCommandState();
            AttackBattleState attackBattleState = new AttackBattleState();
            attackBattleState.Setup(player, enemy);

            AttackResultViewModel attackResultViewModel = new AttackResultViewModel();
            AttackResultPresenter attackResultPresenter = new AttackResultPresenter(attackResultViewModel);

            AttackController attackController = new AttackController(
                attackExecutor,
                attackResultPresenter,
                attackCommandState,
                attackBattleState);

            _playerAttackInputView.Initialize(attackController);
            _attackResultView.Bind(attackResultViewModel);
        }
    }
}