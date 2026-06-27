using System;
using System.Security.Cryptography;

namespace DevelopProducts.SaveSystem
{
    /// <summary>
    ///     セーブデータをAES-CBCで暗号化・復号し、HMAC-SHA256で改ざん・破損を検出するユーティリティ。
    /// </summary>
    public static class SaveCryptoUtility
    {
        /// <summary>
        ///     バイト列をAES-CBCで暗号化し、HMAC-SHA256の認証タグ(MAC)を付与する。
        /// </summary>
        /// <param name="plaintext">暗号化する平文。</param>
        /// <returns>[IV][MAC][暗号文]を連結したバイト列。</returns>
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
                byte[] cipher;
                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    cipher = encryptor.TransformFinalBlock(plaintext, 0, plaintext.Length);
                }

                //  IVと暗号文に対してMAC(改ざん検知用の署名)を計算する（Encrypt-then-MAC）。
                byte[] mac = ComputeMac(iv, cipher);

                //  [IV][MAC][暗号文]の順番に1本のバイト列へ連結する。
                byte[] result = new byte[IV_SIZE + MAC_SIZE + cipher.Length];
                //  先頭からIVを16バイト分コピーする。
                Buffer.BlockCopy(iv, 0, result, 0, IV_SIZE);
                //  IVの後ろにMACを32バイト分コピーする。
                Buffer.BlockCopy(mac, 0, result, IV_SIZE, MAC_SIZE);
                //  MACの後ろに暗号文を残り全部コピーする。
                Buffer.BlockCopy(cipher, 0, result, IV_SIZE + MAC_SIZE, cipher.Length);
                return result;
            }
        }

        /// <summary>
        ///     Encryptで生成したバイト列を、改ざん検証したうえで復号する。
        /// </summary>
        /// <param name="encryptedBytes">[IV][MAC][暗号文]のバイト列。</param>
        /// <returns>復号した平文。</returns>
        /// <exception cref="ArgumentException">バイト列の長さが不正なとき。</exception>
        /// <exception cref="CryptographicException">改ざん・破損でMAC検証に失敗したとき。</exception>
        public static byte[] Decrypt(byte[] encryptedBytes)
        {
            //  最低でもIV+MAC分の長さがなければデータとして壊れている。
            if (encryptedBytes == null || encryptedBytes.Length < IV_SIZE + MAC_SIZE)
            {
                throw new ArgumentException("不正な暗号データです。", nameof(encryptedBytes));
            }

            //  暗号文本体の長さ（全体からIVとMACを引いた残り）。
            int cipherLength = encryptedBytes.Length - IV_SIZE - MAC_SIZE;

            //  [IV][MAC][暗号文]を3つのバイト列に分解する。
            byte[] iv = new byte[IV_SIZE];
            byte[] mac = new byte[MAC_SIZE];
            byte[] cipher = new byte[cipherLength];
            //  先頭16バイトをIVとして取り出す。
            Buffer.BlockCopy(encryptedBytes, 0, iv, 0, IV_SIZE);
            //  続く32バイトをMACとして取り出す。
            Buffer.BlockCopy(encryptedBytes, IV_SIZE, mac, 0, MAC_SIZE);
            //  残りを暗号文として取り出す。
            Buffer.BlockCopy(encryptedBytes, IV_SIZE + MAC_SIZE, cipher, 0, cipherLength);

            //  保存されたMACと、IV+暗号文から再計算したMACを比較する。
            byte[] expectedMac = ComputeMac(iv, cipher);
            //  一致しなければ改ざん・破損とみなして例外を投げる（比較は定数時間で行う）。
            if (!FixedTimeEquals(mac, expectedMac))
            {
                throw new CryptographicException("セーブデータが改ざんまたは破損しています。");
            }

            //  AESの実装を生成する（既定でCBCモード＋PKCS7パディング）。
            using (Aes aes = Aes.Create())
            {
                //  暗号化時と同じ鍵とIVを設定する。
                aes.Key = GetKey();
                aes.IV = iv;

                //  暗号文を復号して平文を返す。
                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                {
                    return decryptor.TransformFinalBlock(cipher, 0, cipherLength);
                }
            }
        }

        /// <summary>AES-CBCのIV(初期化ベクトル)サイズ。ブロック長と同じ16バイト。</summary>
        private const int IV_SIZE = 16;
        /// <summary>HMAC-SHA256の認証タグ(MAC)サイズ。32バイト。</summary>
        private const int MAC_SIZE = 32;
        /// <summary> AES-256の暗号鍵（32バイト）。※暫定のハードコード。 </summary>
        private static readonly byte[] _key =
        {
            0xEE, 0x22, 0xED, 0x3B, 0x06, 0xAE, 0xD8, 0x1A,
            0xD2, 0xDF, 0xF0, 0xC3, 0x7B, 0xC8, 0xBD, 0x65,
            0xEC, 0x9E, 0xFE, 0x89, 0x71, 0xD4, 0xCB, 0xE0,
            0x78, 0x68, 0x42, 0x5C, 0x80, 0xB6, 0x9A, 0x5D,
        };
        /// <summary> HMAC-SHA256の鍵（32バイト）。暗号鍵とは別にする。※暫定のハードコード。 </summary>
        private static readonly byte[] _macKey =
        {
            0xE4, 0xD0, 0xF4, 0xCF, 0x81, 0x28, 0x6C, 0xBE,
            0xA8, 0xD2, 0x5D, 0x02, 0xF0, 0xDF, 0x2E, 0x46,
            0x83, 0xAE, 0x39, 0xAA, 0xF1, 0x77, 0xEE, 0x8F,
            0x44, 0x85, 0x8D, 0xBE, 0x38, 0x26, 0x4C, 0x87,
        };

        /// <summary>
        ///     IVと暗号文に対するHMAC-SHA256(MAC)を計算する。
        /// </summary>
        /// <param name="iv">初期化ベクトル。</param>
        /// <param name="cipher">暗号文。</param>
        /// <returns>32バイトの認証タグ。</returns>
        private static byte[] ComputeMac(byte[] iv, byte[] cipher)
        {
            //  MAC専用の鍵でHMAC-SHA256を生成する。
            using (HMACSHA256 hmac = new HMACSHA256(GetMacKey()))
            {
                //  IVと暗号文を連結したものをHMACの入力にする。
                byte[] data = new byte[iv.Length + cipher.Length];
                //  先頭にIVをコピーする。
                Buffer.BlockCopy(iv, 0, data, 0, iv.Length);
                //  その後ろに暗号文をコピーする。
                Buffer.BlockCopy(cipher, 0, data, iv.Length, cipher.Length);
                //  連結データのHMACを計算して返す。
                return hmac.ComputeHash(data);
            }
        }

        /// <summary>
        ///     2つのバイト列を定数時間で比較する。
        /// </summary>
        /// <param name="a">比較対象1。</param>
        /// <param name="b">比較対象2。</param>
        /// <returns>内容が完全に一致する場合はtrue。</returns>
        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            //  長さが違えば不一致。
            if (a.Length != b.Length) { return false; }

            //  途中でreturnせず全要素をXORで突き合わせ、処理時間を一定に保つ。
            int diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                //  同じバイトなら0、違えば非0ビットが立つ。それをdiffに溜める。
                diff |= a[i] ^ b[i];
            }
            //  最後まで全て一致していればdiffは0のまま。
            return diff == 0;
        }

        /// <summary>
        ///     暗号鍵を取得する。
        /// </summary>
        /// <returns>暗号鍵。</returns>
        private static byte[] GetKey() => _key;

        /// <summary>
        ///     HMACの鍵を取得する。
        /// </summary>
        /// <returns>HMACの鍵。</returns>
        private static byte[] GetMacKey() => _macKey;
    }
}
