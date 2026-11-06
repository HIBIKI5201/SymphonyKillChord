using KillChord.Runtime.Domain.InGame.Battle;

namespace KillChord.Runtime.Application.InGame.Battle
{
    /// <summary>
    ///     攻撃パイプラインの実行を外から観測するための契約。
    ///     <para> エディタと開発ビルドでだけ、ServiceLocatorに登録されている間に呼ばれる。 </para>
    ///     <para> 実装はデバッグ用のアセンブリに置き、ゲームの処理結果を変えてはならない。 </para>
    /// </summary>
    public interface IAttackPipelineTracer
    {
        /// <summary>
        ///     攻撃パイプラインの実行開始を記録する。
        /// </summary>
        /// <param name="context"> 最初のステップに渡す攻撃処理の文脈。 </param>
        /// <param name="stepCount"> このパイプラインのステップ数。 </param>
        void RecordAttackStarted(in AttackStepContext context, int stepCount);

        /// <summary>
        ///     1つのステップの実行結果を記録する。
        /// </summary>
        /// <param name="stepIndex"> 0から数えたステップの位置。 </param>
        /// <param name="step"> 実行したステップ。 </param>
        /// <param name="before"> ステップに渡した文脈。 </param>
        /// <param name="after"> ステップが返した文脈。 </param>
        void RecordStepExecuted(
            int stepIndex,
            IAttackStep step,
            in AttackStepContext before,
            in AttackStepContext after);

        /// <summary>
        ///     攻撃パイプラインの実行完了を記録する。
        /// </summary>
        /// <param name="result"> パイプラインが返した攻撃結果。 </param>
        void RecordAttackCompleted(in AttackResult result);
    }
}
