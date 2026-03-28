using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;
using KillChord.Runtime.View;

namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    ///     Viewからの攻撃コマンドの入力を受け取り、攻撃処理の実行と結果の表示を仲介するクラス。
    /// </summary>
    public class AttackController
    {
        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        /// <param name="attackExecutor"></param>
        /// <param name="presenter"></param>
        /// <param name="commandState"></param>
        /// <param name="battleState"></param>
        /// <param name="skillRepository"></param>
        /// <param name="musicSyncViewModel"></param>
        public AttackController(AttackExecutor attackExecutor,
            AttackResultPresenter presenter,
            AttackCommandState commandState,
            AttackBattleState battleState,
            ISkillRepository skillRepository,
            IMusicSyncViewModel musicSyncViewModel)
        {
            _attackExecutor = attackExecutor;
            _presenter = presenter;
            _commandState = commandState;
            _battleState = battleState;
            _skillRepository = skillRepository;
            _musicSyncViewModel = musicSyncViewModel;
        }

        /// <summary>
        ///     Viewから攻撃コマンドの種類を受け取り、AttackCommandStateに選択された攻撃を更新するメソッド。
        /// </summary>
        /// <param name="commandType"></param>
        public void ChangeAttack(AttackCommandType commandType)
        {
            _commandState.SelectAttack(commandType);
        }

        /// <summary>
        ///     現在の戦闘状態と選択された攻撃コマンドに基づいて攻撃処理を実行しする。
        /// </summary>
        public void ExecuteAttack()
        {
            AttackId attackId = _commandState.SelectedAttackId;
            AttackResult result = _attackExecutor.Execute(
                _battleState.Attacker,
                _battleState.Target,
                attackId);
            _presenter.Push(result);
            //_musicSyncViewModel.Enqueue(new(ActionType.Attack, ));
            //SkillCheckService.TryCheckSkills(_musicSyncViewModel.)
        }

        private readonly AttackExecutor _attackExecutor;
        private readonly AttackResultPresenter _presenter;
        private readonly AttackCommandState _commandState;
        private readonly AttackBattleState _battleState;
        private readonly ISkillRepository _skillRepository;
        private readonly IMusicSyncViewModel _musicSyncViewModel;
    }
}