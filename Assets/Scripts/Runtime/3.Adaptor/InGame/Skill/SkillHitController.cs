using KillChord.Runtime.Application.InGame.Battle;

namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    /// <summary>
    ///     連撃スキルのヒット進行をViewから受け取り、Applicationへ橋渡しするController。
    /// </summary>
    public sealed class SkillHitController
    {
        /// <summary>
        ///     連撃スケジューラを受け取って初期化する。
        /// </summary>
        /// <param name="hitScheduler"> 連撃を時間差で適用するスケジューラです。 </param>
        public SkillHitController(SkillHitScheduler hitScheduler)
        {
            _hitScheduler = hitScheduler;
        }

        /// <summary>
        ///     経過時間を進め、時刻に達したヒットを適用させる。
        /// </summary>
        /// <param name="deltaTime"> 経過時間です。 </param>
        public void Tick(float deltaTime)
        {
            _hitScheduler.Tick(deltaTime);
        }

        /// <summary>
        ///     予約中の連撃をすべて破棄する。
        /// </summary>
        public void Clear()
        {
            _hitScheduler.Clear();
        }

        private readonly SkillHitScheduler _hitScheduler;
    }
}
