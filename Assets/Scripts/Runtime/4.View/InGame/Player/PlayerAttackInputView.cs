using KillChord.Runtime.Adaptor.InGame.Battle;
using KillChord.Runtime.Adaptor.InGame.Skill;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Player
{
    /// <summary>
    ///     プレイヤーからの攻撃入力を受け取り、AttackControllerに伝えるクラス。
    ///     テスト用。
    /// </summary>
    public sealed class PlayerAttackInputView : MonoBehaviour
    {
        private AttackController _attackController;

        public void Initialize(AttackController attackController)
        {
            _attackController = attackController;
        }

        public void OnNormalAttack()
        {
            _attackController.ChangeAttack(AttackCommandType.Normal);
            _attackController.ExecuteAttack();
        }

        public void OnSkillA()
        {
            _attackController.ChangeAttack(AttackCommandType.SkillA);
            _attackController.ExecuteAttack();
        }

        public void OnSkillB()
        {
            _attackController.ChangeAttack(AttackCommandType.SkillB);
            _attackController.ExecuteAttack();
        }

        public void OnUltimate()
        {
            _attackController.ChangeAttack(AttackCommandType.Ultimate);
            _attackController.ExecuteAttack();
        }
    }
}