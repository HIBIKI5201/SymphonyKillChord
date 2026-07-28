namespace KillChord.Runtime.Domain.InGame.Mission.ClearCondition
{
    /// <summary>
    ///     内側の条件をラップするクリア条件デコレータであることを表すインタフェースです。
    ///     複数のデコレータが同じステップへ連結された場合に、チェーンをたどって探索できるようにするために使います。
    /// </summary>
    public interface IDecoratorClearCondition
    {
        /// <summary> ラップしている内側の条件です。 </summary>
        IMissionClearCondition InnerCondition { get; }
    }
}
