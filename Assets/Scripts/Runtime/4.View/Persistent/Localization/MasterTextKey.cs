using System.Security.Cryptography;
using System.Text;

namespace KillChord.Runtime.View.Persistent.Localization
{
    /// <summary>
    ///     マスターデータの日本語の文から、訳文テーブル（MasterText）のキーを作ります。
    ///     キーは日本語の文のハッシュなので、日本語を書き換えるとキーも変わり、その文は未翻訳に戻ります。
    /// </summary>
    public static class MasterTextKey
    {
        /// <summary> マスターデータの訳文を入れるString Table Collection名です。 </summary>
        public const string TABLE_NAME = "MasterText";

        /// <summary>
        ///     日本語の文からキーを作ります。改行はLFにそろえ、前後の空白は除いてから計算します。
        /// </summary>
        /// <param name="source"> マスターデータの日本語の文です。 </param>
        /// <returns> 「mt.」とSHA-256の先頭16桁（小文字の16進数）をつないだキーです。空の文は空文字を返します。 </returns>
        public static string Create(string source)
        {
            string normalized = Normalize(source);
            if (normalized.Length == 0)
            {
                return string.Empty;
            }

            using SHA256 sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(normalized));
            var builder = new StringBuilder(KEY_PREFIX.Length + HASH_BYTE_COUNT * 2);
            builder.Append(KEY_PREFIX);
            for (int i = 0; i < HASH_BYTE_COUNT; i++)
            {
                builder.Append(hash[i].ToString("x2"));
            }

            return builder.ToString();
        }

        /// <summary>
        ///     キーの計算に使う形へ文をそろえます。改行をLFにし、前後の空白を除きます。
        /// </summary>
        /// <param name="source"> マスターデータの日本語の文です。 </param>
        /// <returns> そろえた文です。nullは空文字にします。 </returns>
        public static string Normalize(string source)
        {
            return (source ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n').Trim();
        }

        private const string KEY_PREFIX = "mt.";
        private const int HASH_BYTE_COUNT = 8;
    }
}
