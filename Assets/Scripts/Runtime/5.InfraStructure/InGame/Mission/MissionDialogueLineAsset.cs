using KillChord.Runtime.Domain.InGame.Mission;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     Ingame会話1件分の定義。
    /// </summary>
    [Serializable]
    public sealed class MissionDialogueLineAsset
    {
        /// <summary>
        ///     会話情報データへ変換する。
        /// </summary>
        public MissionDialogueLine Create()
        {
            return new(_textEntryKey, _text, _portrait, _voiceCueName, _silentDuration);
        }

        [SerializeField, Tooltip("会話テキストのTutorialSubtitlesテーブルキー。空文字列の場合はフォールバックを表示します。")]
        private string _textEntryKey = string.Empty;
        [SerializeField, TextArea(2, 5), Tooltip("翻訳未登録時に表示する会話テキスト。")]
        private string _text = "";
        [SerializeField, Tooltip("顔画像。未設定可")]
        private Sprite _portrait;
        [SerializeField, Tooltip("会話ボイスのCue名。空文字列可")]
        private string _voiceCueName = string.Empty;
        [SerializeField, Min(0.1f), Tooltip("ボイス無しの場合の表示秒数")]
        private float _silentDuration = 3f;
    }
}
