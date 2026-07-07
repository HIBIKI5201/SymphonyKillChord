using System;
using System.Security.Cryptography;
using UnityEngine;

namespace DevelopProducts.SaveSystem
{
    /// <summary>
    ///     セーブデータをAES-CBCで暗号化・復号するユーティリティ。
    ///     目的は覗き見防止であり、改ざん耐性は提供しない（鍵は端末ローカルにあるため）。
    ///     鍵は端末ごとに初回ランダム生成してローカル(PlayerPrefs)に保存する。
    /// </summary>
    public static class SaveCryptoUtility
    {
        /// <summary>
        ///     バイト列をAES-CBCで暗号化する。
        /// </summary>
        /// <param name="plaintext">暗号化する平文。</param>
        /// <returns>[IV][暗号文]を連結したバイト列。</returns>
        public static byte[] Encrypt(byte[] plaintext)
        {
            //  AESの実装を生成する（既定でCBCモード＋PKCS7パディング）。
            using (Aes aes = Aes.Create())
            {
                //  暗号化に使う鍵を設定する。
                aes.Key = GetKey();

                //  保存ごとにIV(初期化ベクトル)をランダム生成する。
                //  IVは毎回違う使い捨ての値で、同じ平文でも毎回違う暗号文にするために使う。
                aes.GenerateIV();
                byte[] iv = aes.IV;

                //  平文を暗号化して暗号文を得る。パディング処理もここで行われる。
                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    byte[] cipher = encryptor.TransformFinalBlock(plaintext, 0, plaintext.Length);

                    //  [IV][暗号文]の順番に連結して返す。
                    byte[] result = new byte[IV_SIZE + cipher.Length];
                    Buffer.BlockCopy(iv, 0, result, 0, IV_SIZE);
                    Buffer.BlockCopy(cipher, 0, result, IV_SIZE, cipher.Length);
                    return result;
                }
            }
        }

        /// <summary>
        ///     Encryptで生成したバイト列を復号する。
        /// </summary>
        /// <param name="encryptedBytes">[IV][暗号文]のバイト列。</param>
        /// <returns>復号した平文。</returns>
        /// <exception cref="ArgumentException">バイト列の長さが不正なとき。</exception>
        public static byte[] Decrypt(byte[] encryptedBytes)
        {
            //  最低でもIV分の長さがなければデータとして壊れている。
            if (encryptedBytes == null || encryptedBytes.Length < IV_SIZE)
            {
                throw new ArgumentException("不正な暗号データです。", nameof(encryptedBytes));
            }

            using (Aes aes = Aes.Create())
            {
                aes.Key = GetKey();

                //  先頭のIVを取り出して設定する。
                byte[] iv = new byte[IV_SIZE];
                Buffer.BlockCopy(encryptedBytes, 0, iv, 0, IV_SIZE);
                aes.IV = iv;

                //  IVより後ろが暗号文本体。
                int cipherLength = encryptedBytes.Length - IV_SIZE;
                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                {
                    //  暗号文を復号して平文を返す。
                    return decryptor.TransformFinalBlock(encryptedBytes, IV_SIZE, cipherLength);
                }
            }
        }

        /// <summary>AES-CBCのIV(初期化ベクトル)サイズ。ブロック長と同じ16バイト。</summary>
        private const int IV_SIZE = 16;
        /// <summary>暗号鍵のサイズ（32バイト）。</summary>
        private const int ENC_KEY_SIZE = 32;
        /// <summary>鍵を保存するPlayerPrefsのキー名。</summary>
        private const string KEY_PREF = "SaveKey_v2";

        /// <summary>キャッシュした暗号鍵。</summary>
        private static byte[] _encryptionKey;
        /// <summary>鍵の初期化を排他するためのロックオブジェクト。</summary>
        private static readonly object _keyLock = new object();

        /// <summary>
        ///     端末ローカルの鍵を読み込む。無ければ、または壊れていれば新規生成して保存する。
        /// </summary>
        private static void EnsureKeys()
        {
            //  既に読み込み済みなら何もしない
            if (_encryptionKey != null)
            {
                return;
            }

            lock (_keyLock)
            {
                //  再確認
                if (_encryptionKey != null)
                {
                    return;
                }

                byte[] encryptionKey = TryLoadKey();
                if (encryptionKey == null)
                {
                    encryptionKey = new byte[ENC_KEY_SIZE];
                    RandomNumberGenerator.Fill(encryptionKey);

                    //  保存。壊れた値であれば上書きされる
                    PlayerPrefs.SetString(KEY_PREF, Convert.ToBase64String(encryptionKey));
                    PlayerPrefs.Save();
                }

                _encryptionKey = encryptionKey;
            }
        }
        /// <summary>
        ///     PlayerPrefsから鍵を読み込む。未保存・形式不正・長さ不正の場合はnullを返す。
        /// </summary>
        /// <returns>有効な鍵。無効ならnull。</returns>
        private static byte[] TryLoadKey()
        {
            if (!PlayerPrefs.HasKey(KEY_PREF))
            {
                return null;
            }

            //  保存されている暗号鍵のバイト列のコピー
            byte[] stored;

            try
            {
                //  文字をバイト列に復元
                stored = Convert.FromBase64String(PlayerPrefs.GetString(KEY_PREF));
            }
            catch (FormatException)
            {
                //  Base64として壊れている。復元不能なので破棄対象。
               Debug.LogWarning("保存鍵の形式が不正です。鍵を再生成します");
                return null;
            }

            if (stored.Length != ENC_KEY_SIZE)
            {
                //  長さが想定外。復元不能なので破棄対象とする。
                Debug.LogWarning("保存鍵の長さが不正です。鍵を再生成します。");
                return null;
            }

            return stored;
        }

        /// <summary>
        ///     暗号鍵を取得する。
        /// </summary>
        /// <returns>暗号鍵。</returns>
        private static byte[] GetKey()
        {
            EnsureKeys();
            return _encryptionKey;
        }
    }
}
