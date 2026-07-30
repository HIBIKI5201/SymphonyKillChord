namespace KillChord.Runtime.Domain.InGame.Buff
{
    /// <summary>
    ///     バフのタイプクラスをまとめたデータ。
    /// </summary>
    public readonly struct BuffMetaData
    {
        public BuffMetaData(BuffExecuteTiming timing, bool isPersistent = false)
        {
            _executeTimingType = timing;
            _isPersistent = isPersistent;
        }

        public BuffExecuteTiming GetActivationType() => _executeTimingType;

        /// <summary> 発動後もリストから削除されない永続バフかどうかを取得する。 </summary>
        public bool IsPersistent() => _isPersistent;

        private readonly BuffExecuteTiming _executeTimingType;
        private readonly bool _isPersistent;
    }
}
