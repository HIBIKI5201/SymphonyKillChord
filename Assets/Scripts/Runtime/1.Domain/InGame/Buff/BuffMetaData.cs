namespace KillChord.Runtime.Domain.InGame.Buff
{
    /// <summary>
    ///     バフのタイプクラスをまとめたデータ。
    /// </summary>
    public readonly struct BuffMetaData
    {
        public BuffMetaData(BuffExecuteTiming timing)
        {
            _executeTimingType = timing;
        }

        public BuffExecuteTiming GetActivationType() => _executeTimingType;

        private readonly BuffExecuteTiming _executeTimingType;
    }
}
