using System;
using System.IO;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace KillChord.Runtime.InfraStructure.OutGame.Screen
{
    /// <summary>
    ///     StreamingAssets 内のテキストファイルを読み込むクラス。
    /// </summary>
    internal static class StreamingAssetsTextReader
    {
        /// <summary>
        ///     StreamingAssets 内のテキストファイルの全文を読み込みます。
        /// </summary>
        /// <param name="directoryName"> StreamingAssets 直下のフォルダ名です。 </param>
        /// <param name="fileName"> ファイル名です。 </param>
        /// <param name="ct"> キャンセルトークンです。 </param>
        /// <returns> ファイルの全文です。 </returns>
        /// <exception cref="IOException"> ファイルを読み込めなかった場合に発生します。 </exception>
        public static async ValueTask<string> ReadAllTextAsync(string directoryName, string fileName, CancellationToken ct)
        {
            string root = UnityEngine.Application.streamingAssetsPath;

            // Android では StreamingAssets が APK 内にあり、File API では読めないため UnityWebRequest で読む。
            if (!root.Contains(URL_SCHEME_SEPARATOR, StringComparison.Ordinal))
            {
                string path = Path.Combine(root, directoryName, fileName);
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException($"ファイルが見つかりません。path={path}", path);
                }

                return await File.ReadAllTextAsync(path, Encoding.UTF8, ct);
            }

            string url = $"{root.TrimEnd('/')}/{directoryName}/{fileName}";
            using UnityWebRequest request = UnityWebRequest.Get(url);
            using CancellationTokenRegistration registration = ct.Register(s => ((UnityWebRequest)s).Abort(), request);
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                ct.ThrowIfCancellationRequested();
                await UniTask.Yield();
            }

            ct.ThrowIfCancellationRequested();
            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new IOException($"ファイルを読み込めませんでした。url={url}, error={request.error}");
            }

            return request.downloadHandler != null ? request.downloadHandler.text : string.Empty;
        }

        /// <summary> StreamingAssets のパスが URL 形式かどうかを判別する区切り文字です。 </summary>
        private const string URL_SCHEME_SEPARATOR = "://";
    }
}
