using System;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.Localization
{
    /// <summary>
    ///     エディタの起動時に1回、訳語の正本をローカライズのテーブルへ自動で取り込みます。
    ///     APIキーと正本フォルダのIDが設定済みで、「起動時に自動で取り込む」がオンのときだけ動きます。
    /// </summary>
    [InitializeOnLoad]
    public static class LocalizationSheetAutoSync
    {
        /// <summary>
        ///     スクリプトの読み込み後、エディタが落ち着いてから取り込みを始めます。
        /// </summary>
        static LocalizationSheetAutoSync()
        {
            // バッチモード（CI・ビルド）では通信しない。再コンパイルのたびに走らないよう、起動中に1回だけにする。
            if (Application.isBatchMode || SessionState.GetBool(SESSION_STATE_KEY, false))
            {
                return;
            }

            SessionState.SetBool(SESSION_STATE_KEY, true);
            EditorApplication.delayCall += HandleDelayCall;
        }

        private const string SESSION_STATE_KEY = "KillChord.LocalizationSheetAutoSync.HasRun";

        /// <summary>
        ///     起動直後の処理が終わったところで、設定を確かめて取り込みます。
        /// </summary>
        private static async void HandleDelayCall()
        {
            try
            {
                if (!LocalizationSheetSync.IsConfigured
                    || !EditorPrefs.GetBool(LocalizationSheetSync.AUTO_SYNC_PREFS_KEY, true)
                    || EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    return;
                }

                (bool isSucceeded, string message) = await LocalizationSheetSync.SyncAsync(null);
                if (!isSucceeded)
                {
                    Debug.LogWarning($"[{nameof(LocalizationSheetAutoSync)}] 訳語の自動取り込みに失敗しました。{message}");
                }
            }
            catch (Exception exception) when (!(exception is OperationCanceledException))
            {
                Debug.LogException(exception);
            }
        }
    }
}
