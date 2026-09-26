using Unity.Entities;

namespace KillChord.Runtime.View.InGame.Stage.Barrage
{
    /// <summary>
    ///     ポーズ中に弾幕システムグループの更新を丸ごと止めます。
    /// </summary>
    /// <remarks>
    ///     各システムでDeltaTimeを分岐させる方式は漏れが起きやすいため、
    ///     グループ単位で更新自体を止めています。
    /// </remarks>
    public sealed class BarragePauseRateManager : IRateManager
    {
        /// <summary> ポーズ中かどうかです。 </summary>
        public bool IsPaused { get; private set; }

        /// <summary> 固定タイムステップは使用しないため常に0です。 </summary>
        public float Timestep
        {
            get => 0f;

            // 固定タイムステップを持たないため設定値は無視する。
            set { }
        }

        /// <summary>
        ///     ポーズ状態を設定します。
        /// </summary>
        /// <param name="isPaused"> ポーズ中にする場合はtrueです。 </param>
        public void SetPaused(bool isPaused)
        {
            IsPaused = isPaused;
        }

        /// <summary>
        ///     グループ配下のシステムを更新すべきか判定します。
        /// </summary>
        /// <param name="group"> 判定対象のシステムグループです。 </param>
        /// <returns> 更新する場合はtrueです。 </returns>
        public bool ShouldGroupUpdate(ComponentSystemGroup group)
        {
            // falseを返すまで繰り返し呼ばれる契約のため、1フレームにつき1回で打ち切る。
            if (_didUpdate)
            {
                _didUpdate = false;
                return false;
            }

            // ポーズ中はグループ配下のシステムを一切更新しない。
            // 時間をPushしていないためPopも不要で、World側の時間スタックには触れない。
            if (IsPaused) { return false; }

            _didUpdate = true;
            return true;
        }

        private bool _didUpdate;
    }
}
