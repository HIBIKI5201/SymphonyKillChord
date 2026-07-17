using UnityEngine;

namespace KillChord.Editor.SourceDataProvider
{
    /// <summary>
    ///     DataIDセレクターへ表示する登録済みデータを保持します。
    /// </summary>
    internal readonly struct SourceDataIDOption
    {
        /// <summary>
        ///     登録済みデータの情報を初期化します。
        /// </summary>
        /// <param name="id"> 人間可読な文字列IDです。 </param>
        /// <param name="hashId"> 焼き込み済み数値IDです。 </param>
        /// <param name="source"> 登録元のUnityオブジェクトです。 </param>
        public SourceDataIDOption(string id, int hashId, Object source)
        {
            Id = id;
            HashId = hashId;
            Source = source;
        }

        /// <summary> 人間可読な文字列IDです。 </summary>
        public string Id { get; }

        /// <summary> 焼き込み済み数値IDです。 </summary>
        public int HashId { get; }

        /// <summary> 登録元のUnityオブジェクトです。 </summary>
        public Object Source { get; }
    }
}
