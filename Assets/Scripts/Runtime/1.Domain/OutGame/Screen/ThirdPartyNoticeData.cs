using System;

namespace KillChord.Runtime.Domain.OutGame.Screen
{
    /// <summary>
    ///     クレジットの「使用アセット」に載せる、サードパーティ素材1件分の表記を保持する値オブジェクト。
    /// </summary>
    public readonly struct ThirdPartyNoticeData
    {
        /// <summary>
        ///     サードパーティ素材の表記を初期化します。
        /// </summary>
        /// <param name="name"> 素材名です。 </param>
        /// <param name="holder"> 権利者・著作表記です。無い場合は空文字列です。 </param>
        /// <param name="license"> ライセンス名です。無い場合は空文字列です。 </param>
        /// <param name="text"> 著作表記や許諾表示の本文です。無い場合は空文字列です。 </param>
        /// <exception cref="ArgumentException"> 素材名が null または空の場合に発生します。 </exception>
        public ThirdPartyNoticeData(string name, string holder, string license, string text)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("素材名が null または空です。", nameof(name));
            }

            Name = name;
            Holder = holder ?? string.Empty;
            License = license ?? string.Empty;
            Text = text ?? string.Empty;
        }

        /// <summary> 素材名を取得します。 </summary>
        public string Name { get; }

        /// <summary> 権利者・著作表記を取得します。 </summary>
        public string Holder { get; }

        /// <summary> ライセンス名を取得します。 </summary>
        public string License { get; }

        /// <summary> 著作表記や許諾表示の本文を取得します。 </summary>
        public string Text { get; }
    }
}
