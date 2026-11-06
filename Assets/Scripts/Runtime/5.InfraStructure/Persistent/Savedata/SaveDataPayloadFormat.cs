using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.Persistent.Savedata
{
    /// <summary>
    ///     セーブデータの本体JSONに形式のバージョン番号を付けて包み、読み込み時に外して最新の形式へ移す。
    ///     バージョン番号の無いデータは、番号を付ける前の形式（バージョン0）として扱う。
    /// </summary>
    public static class SaveDataPayloadFormat
    {
        /// <summary> 今のアプリが書き込む形式のバージョン番号。 </summary>
        public const int CURRENT_VERSION = 1;

        /// <summary>
        ///     本体JSONを今のバージョン番号とともに包む。
        /// </summary>
        /// <param name="json"> セーブデータ本体のJSON。 </param>
        /// <returns> 暗号化して保存する文字列。 </returns>
        public static string Wrap(string json)
        {
            return JsonUtility.ToJson(new Payload(CURRENT_VERSION, json ?? string.Empty));
        }

        /// <summary>
        ///     保存されていた文字列から、バージョン番号と本体JSONを取り出す。
        /// </summary>
        /// <param name="text"> 復号した文字列。 </param>
        /// <param name="version"> 形式のバージョン番号。包まれていない場合は0。 </param>
        /// <param name="json"> 本体JSON。 </param>
        /// <returns> JSONとして読めた場合はtrue。 </returns>
        public static bool TryUnwrap(string text, out int version, out string json)
        {
            version = 0;
            json = null;
            if (string.IsNullOrWhiteSpace(text)) { return false; }

            Payload payload;
            try
            {
                payload = JsonUtility.FromJson<Payload>(text);
            }
            catch (ArgumentException)
            {
                // JSONとして壊れている。
                return false;
            }

            if (payload == null) { return false; }

            // 包む前の形式は、文字列全体が本体JSONになっている。
            if (payload.Version <= 0 || payload.Data == null)
            {
                json = text;
                return true;
            }

            version = payload.Version;
            json = payload.Data;
            return true;
        }

        /// <summary>
        ///     古いバージョンの本体JSONを、今のバージョンの形式へ順番に移す。
        /// </summary>
        /// <param name="version"> 読み込んだデータのバージョン番号。 </param>
        /// <param name="json"> 読み込んだ本体JSON。 </param>
        /// <returns> 今のバージョンの形式の本体JSON。 </returns>
        public static string MigrateToCurrent(int version, string json)
        {
            if (version > CURRENT_VERSION)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(version), version, "今のアプリより新しい形式のデータは移せません。");
            }

            // バージョン0から1へは、本体JSONの形を変えずに番号だけを付ける。
            // 形式を変えるときは、ここに1段ずつ移す処理を足し、CURRENT_VERSIONを上げる。
            return json;
        }

        /// <summary>
        ///     保存する文字列の形。JsonUtilityで読み書きするため、フィールド名がそのままJSONのキーになる。
        /// </summary>
        [Serializable]
        private sealed class Payload
        {
            /// <summary>
            ///     JsonUtilityが読み込むときに使う。
            /// </summary>
            public Payload()
            {
            }

            /// <summary>
            ///     保存する値を持たせて作る。
            /// </summary>
            public Payload(int version, string data)
            {
                _version = version;
                _data = data;
            }

            /// <summary> 形式のバージョン番号。 </summary>
            public int Version => _version;

            /// <summary> セーブデータ本体のJSON。 </summary>
            public string Data => _data;

            [SerializeField, Tooltip("形式のバージョン番号。")]
            private int _version;

            [SerializeField, Tooltip("セーブデータ本体のJSON。")]
            private string _data;
        }
    }
}
