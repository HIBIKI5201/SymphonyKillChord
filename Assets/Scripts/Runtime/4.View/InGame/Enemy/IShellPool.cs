namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     砲弾のObject Poolにアクセスするインタフェース。
    /// </summary>
    public interface IShellPool
    {
        /// <summary>
        ///     Object Poolから砲弾インスタンスを取り出す。
        /// </summary>
        /// <returns></returns>
        public IShellLifeCycle GetShell();
    }
}
