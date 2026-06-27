using System;
using System.Security.Cryptography;

namespace DevelopProducts.SaveSystem
{
    /// <summary>
    ///     セーブデータをAES-GCMで暗号化、復号化するユーティリティクラス
    /// </summary>
    public static class SaveCryptoUtility
    {
        /// <summary>
        ///     バイト列を暗号化する。
        /// </summary>
        /// <param name="plaintext">暗号化する平文。</param>
        /// <returns>暗号化されたバイト列。</returns>
        public static byte[] Encrypt(byte[] plaintext)
        {
            //  保存ごとにnonceをランダムに生成する。
            //  "Number used once"（一度だけ使う数）の略。暗号化のたびに変える使い捨ての値
            byte[] nonce = new byte[NONCE_SIZE];
            RandomNumberGenerator.Fill(nonce);//乱数で配列を埋める。

            //  plaintextを暗号化した結果。中身を見ても分からないバイト列。
            byte[] cipher = new byte[plaintext.Length];
            //  暗号化時に一緒に作る検証用データ。復号時にこれを照合して改ざん、破損していないか確認する。
            byte[] tag = new byte[TAG_SIZE];
            //  AesGcmとは暗号化と改ざん検知を行うシステム。
            using (AesGcm aes = new AesGcm(GetKey()))
            {
                aes.Encrypt(nonce, plaintext, cipher, tag);
            }
            //  [nonce][tag][cipher]の順番に連結して返す。
            byte[] result = new byte[NONCE_SIZE + TAG_SIZE + cipher.Length];

            //  nonceをresultの位置から12バイト分コピーする。
            Buffer.BlockCopy(nonce, 0, result, 0, NONCE_SIZE);
            //  tagをnonceの位置から16バイト分コピーする。
            Buffer.BlockCopy(tag, 0, result, NONCE_SIZE, TAG_SIZE);
            //  cipherをtagの位置から残り全部コピーする。
            Buffer.BlockCopy(cipher, 0, result, NONCE_SIZE + TAG_SIZE, cipher.Length);

            return result;
        }
        /// <summary>
        ///     Encryptで生成したバイト列を復号する。
        /// </summary>
        /// <param name="encryptedBytes">[nonce][tag][暗号文]のバイト列。</param>
        /// <returns>復号した平文</returns>
        /// <exception cref="ArgumentException">バイト列の長さが不正なとき。</exception>
        /// <exception cref="CryptographicException">改ざん・破損で復号に失敗したとき。</exception>
        public static byte[] Decrypt(byte[] encryptedBytes)
        {
            if (encryptedBytes == null || encryptedBytes.Length < NONCE_SIZE + TAG_SIZE)
            {
                throw new ArgumentException("不正な暗号データです。", nameof(encryptedBytes));
            }
            //  暗号文の長さ。
            int cipherLength = encryptedBytes.Length - NONCE_SIZE - TAG_SIZE;

            byte[] nonce = new byte[NONCE_SIZE];
            byte[] tag = new byte[TAG_SIZE];
            byte[] cipher = new byte[cipherLength];

            //  それぞれのバイト列に現在のバイト列をコピーさせる。
            Buffer.BlockCopy(encryptedBytes, 0, nonce, 0, NONCE_SIZE);
            Buffer.BlockCopy(encryptedBytes, NONCE_SIZE, tag, 0, TAG_SIZE);
            Buffer.BlockCopy(encryptedBytes, NONCE_SIZE + TAG_SIZE, cipher, 0, cipherLength);

            byte[] plain = new byte[cipherLength];

            //  データの復元を行う。
            using (AesGcm aes = new AesGcm(GetKey()))
            {
                aes.Decrypt(nonce, cipher, tag, plain);
            }

            return plain;
        }
        /// <summary>nonceのサイズ。</summary>
        private const int NONCE_SIZE = 12;
        /// <summary>認証タグのサイズ。</summary>
        private const int TAG_SIZE = 16;
        /// <summary> AES-256の暗号鍵（32バイト）。※暫定のハードコード。 </summary>
        private static readonly byte[] _key =
        {
            0xEE, 0x22, 0xED, 0x3B, 0x06, 0xAE, 0xD8, 0x1A,
            0xD2, 0xDF, 0xF0, 0xC3, 0x7B, 0xC8, 0xBD, 0x65,
            0xEC, 0x9E, 0xFE, 0x89, 0x71, 0xD4, 0xCB, 0xE0,
            0x78, 0x68, 0x42, 0x5C, 0x80, 0xB6, 0x9A, 0x5D,
        };
        /// <summary>
        ///     暗号鍵を取得する。
        /// </summary>
        /// <returns>暗号鍵</returns>
        private static byte[] GetKey() => _key;
    }
}
