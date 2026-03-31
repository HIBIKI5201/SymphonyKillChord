namespace KillChord.Runtime.Utility
{
    /// <summary>
    ///     スクリプトの実行順序を管理する定数クラス。
    /// </summary>
    public static class ExecutionOrderConst
    {
        /// <summary>
        /// Composition層のInitializer向け
        /// </summary>
        public const int INITIALIZATION = -100;

        /// <summary>
        /// View層のロジック向け　例：Player Enemy
        /// </summary>
        public const int MOVEMENT = 100;

        /// <summary>
        /// View層のCamera向け
        /// </summary>
        public const int CAMERA_FOLLOW = 200;

        /// <summary>
        /// View層のHUD向け
        /// </summary>
        public const int HUD = 300;
    }
}
