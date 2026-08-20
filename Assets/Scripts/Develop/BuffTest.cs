using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Domain;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Buff;

namespace KillChord.Develop
{
    /// <summary>
    ///     バフシステムの動作確認に使用するテスト用バフ。
    /// </summary>
    public class BuffTest : IBuff
    {
        /// <summary>
        ///     即時発動時に攻撃力とダメージ結果を増加させる。
        /// </summary>
        /// <param name="context"> バフ実行コンテキスト。</param>
        /// <returns> 更新後のコンテキスト。</returns>
        public BuffContext ExecuteInstance(BuffContext context)
        {
            context.Attacker.ChangeBaseDamage(context.Attacker.BaseDamage * _multiplier);
            return new BuffContext(
                context.Attacker,
                context.Target,
                new AttackResult(context.AttackResult.FinalDamage * 2, context.AttackResult.IsCritical));
        }

        /// <summary>
        ///     一定時間だけ攻撃力を増加させる。
        /// </summary>
        /// <param name="context"> バフ実行コンテキスト。</param>
        /// <param name="token"> キャンセルトークン。</param>
        public async ValueTask ExecuteAsync(BuffContext context, CancellationToken token)
        {
            context.Attacker.ChangeBaseDamage(context.Attacker.BaseDamage * _multiplier);
            await Task.Delay(System.TimeSpan.FromSeconds(_waitTime), token);
            context.Attacker.ChangeBaseDamage(context.Attacker.BaseDamage / _multiplier);
        }

        /// <summary>
        ///     バフ状態を返す。
        /// </summary>
        /// <returns> バフメタデータ。</returns>
        public BuffMetaData GetState()
        {
            return _status;
        }

        private readonly BuffMetaData _status = new(BuffExecuteTiming.Attack_Logic_Before);
        private readonly float _multiplier = 2f;
        private readonly float _waitTime = 5f;
    }
}
