using System;
using System.Security.Cryptography;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.Persistent.Savedata
{
    /// <summary>
    ///     セーブデータをAES-256-CBCで暗号化・復号する。
    ///     目的は覗き見防止であり、改ざん耐性は提供しない（鍵は端末ローカルにあるため）。
    ///     鍵は端末ごとに初回ランダム生成し、PlayerPrefsへ保存する。
    /// </summary>
    internal static class SaveDataCrypto
    {
        /// <summary>
        ///     バイト列を暗号化する。
        /// </summary>
        /// <param name="plaintext"> 暗号化する平文。 </param>
        /// <returns> [IV][暗号文]を連結したバイト列。 </returns>
        public static byte[] Encrypt(byte[] plaintext)
        {
            using Aes aes = Aes.Create();
            aes.Key = GetKey();

            // 保存ごとにIVを作り直し、同じ平文でも毎回違う暗号文にする。
            aes.GenerateIV();
            byte[] iv = aes.IV;

            using ICryptoTransform encryptor = aes.CreateEncryptor();
            byte[] cipher = encryptor.TransformFinalBlock(plaintext, 0, plaintext.Length);

            byte[] result = new byte[IV_SIZE + cipher.Length];
            Buffer.BlockCopy(iv, 0, result, 0, IV_SIZE);
            Buffer.BlockCopy(cipher, 0, result, IV_SIZE, cipher.Length);
            return result;
        }

        /// <summary>
        ///     <see cref="Encrypt"/> で作ったバイト列を復号する。
        /// </summary>
        /// <param name="encryptedBytes"> [IV][暗号文]のバイト列。 </param>
        /// <returns> 復号した平文。 </returns>
        /// <exception cref="ArgumentException"> バイト列の長さが不正なとき。 </exception>
        /// <exception cref="CryptographicException"> 鍵が違う・データが壊れているなどで復号できないとき。 </exception>
        public static byte[] Decrypt(byte[] encryptedBytes)
        {
            if (encryptedBytes == null || encryptedBytes.Length <= IV_SIZE)
            {
                throw new ArgumentException("不正な暗号データです。", nameof(encryptedBytes));
            }

            using Aes aes = Aes.Create();
            aes.Key = GetKey();

            byte[] iv = new byte[IV_SIZE];
            Buffer.BlockCopy(encryptedBytes, 0, iv, 0, IV_SIZE);
            aes.IV = iv;

            using ICryptoTransform decryptor = aes.CreateDecryptor();
            return decryptor.TransformFinalBlock(encryptedBytes, IV_SIZE, encryptedBytes.Length - IV_SIZE);
        }

        /// <summary> AES-CBCのIVのサイズ。ブロック長と同じ16バイト。 </summary>
        private const int IV_SIZE = 16;

        /// <summary> 暗号鍵のサイズ（AES-256の32バイト）。 </summary>
        private const int KEY_SIZE = 32;

        /// <summary> 鍵を保存するPlayerPrefsのキー名。 </summary>
        private const string KEY_PREF = "KillChord.SaveDataKey";

        /// <summary> キャッシュした暗号鍵。 </summary>
        private static byte[] _key;

        /// <summary> 鍵の初期化を排他するためのロック。 </summary>
        private static readonly object KEY_LOCK = new object();

        /// <summary>
        ///     暗号鍵を取得する。無い、または壊れている場合は新しく作って保存する。
        /// </summary>
        private static byte[] GetKey()
        {
            if (_key != null)
            {
                return _key;
            }

            lock (KEY_LOCK)
            {
                if (_key != null)
                {
                    return _key;
                }

                byte[] key = TryLoadKey();
                if (key == null)
                {
                    key = new byte[KEY_SIZE];
                    RandomNumberGenerator.Fill(key);
                    PlayerPrefs.SetString(KEY_PREF, Convert.ToBase64String(key));
                    PlayerPrefs.Save();
                }

                _key = key;
                return _key;
            }
        }

        /// <summary>
        ///     PlayerPrefsから鍵を読み込む。未保存・形式不正・長さ不正の場合はnullを返す。
        /// </summary>
        private static byte[] TryLoadKey()
        {
            if (!PlayerPrefs.HasKey(KEY_PREF))
            {
                return null;
            }

            byte[] stored;
            try
            {
                stored = Convert.FromBase64String(PlayerPrefs.GetString(KEY_PREF));
            }
            catch (FormatException)
            {
                Debug.LogWarning($"[{nameof(SaveDataCrypto)}] 保存された鍵の形式が不正です。鍵を作り直します。");
                return null;
            }

            if (stored.Length != KEY_SIZE)
            {
                Debug.LogWarning($"[{nameof(SaveDataCrypto)}] 保存された鍵の長さが不正です。鍵を作り直します。");
                return null;
            }

            return stored;
        }
    }
}
