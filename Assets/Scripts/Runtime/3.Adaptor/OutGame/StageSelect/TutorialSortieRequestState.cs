namespace KillChord.Runtime.Adaptor.OutGame.StageSelect
{
    /// <summary>
    ///     シーンを跨いだチュートリアル自動出撃要求を保持します。
    /// </summary>
    public sealed class TutorialSortieRequestState
    {
        /// <summary> チュートリアル自動出撃が要求されている場合はtrueです。 </summary>
        public bool IsRequested => _isRequested;

        /// <summary>
        ///     チュートリアル自動出撃を要求します。
        /// </summary>
        public void Request()
        {
            _isRequested = true;
        }

        /// <summary>
        ///     保持中の要求を一度だけ取得します。
        /// </summary>
        /// <returns> 要求が存在した場合はtrueです。 </returns>
        public bool TryConsume()
        {
            if (!_isRequested)
            {
                return false;
            }

            _isRequested = false;
            return true;
        }

        private bool _isRequested;
    }
}
