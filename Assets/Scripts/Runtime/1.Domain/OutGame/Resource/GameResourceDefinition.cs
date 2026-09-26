namespace KillChord.Runtime.Domain.OutGame.Resource
{
    /// <summary>
    ///     ゲーム内リソースの定義情報を表すクラスです。
    /// </summary>
    public sealed class GameResourceDefinition
    {
        /// <summary>
        ///     GameResourceDefinition の新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="id"> リソースのIDです。 </param>
        /// <param name="displayName"> 画面に表示するリソース名です。 </param>
        public GameResourceDefinition(GameResourceId id, string displayName)
        {
            _id = id;
            _displayName = displayName ?? string.Empty;
        }

        /// <summary> リソースのIDです。 </summary>
        public GameResourceId Id => _id;

        /// <summary> 画面に表示するリソース名です。 </summary>
        public string DisplayName => _displayName;

        private readonly GameResourceId _id;
        private readonly string _displayName;
    }
}
