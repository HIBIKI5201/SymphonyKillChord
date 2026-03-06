using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mock.MusicBattle.MusicSync
{
    /// <summary>
    ///     音楽UIの表示を制御するクラス（開発用）。
    /// </summary>
    public class MusicUI : MonoBehaviour
    {
        #region インスペクター表示フィールド
        /// <summary> ノーツの親Transform。 </summary>
        [SerializeField, Tooltip("ノーツの親Transform。")]
        private RectTransform _notesParent;
        /// <summary> ノーツの初期位置。 </summary>
        [SerializeField, Tooltip("ノーツの初期位置。")]
        private RectTransform _originPos;
        /// <summary> ノーツの終点位置。 </summary>
        [SerializeField, Tooltip("ノーツの終点位置。")]
        private RectTransform _noteEndPos;
        /// <summary> ノーツの移動速度。 </summary>
        [SerializeField, Tooltip("ノーツの移動速度。")]
        private float _noteSpeed;
        /// <summary> ノーツのプレファブ。 </summary>
        [SerializeField, Tooltip("ノーツのプレファブ。")]
        private Image _notePrefab;
        #endregion

        #region Publicメソッド
        /// <summary>
        ///     指定された色でノーツを生成し、表示します。
        /// </summary>
        /// <param name="color">ノーツの色。</param>
        public void CreateNote(Color color)
        {
            Image note = Instantiate(_notePrefab);
            note.transform.SetParent(_notesParent, false);
            note.rectTransform.anchoredPosition = _originPos.anchoredPosition;
            note.color = color;

            _notes.Add(note);
        }
        #endregion

        #region プライベートフィールド
        /// <summary> アクティブなノーツのリスト。 </summary>
        private readonly List<Image> _notes = new();
        #endregion

        #region Unityライフサイクルメソッド
        /// <summary>
        ///     フレームごとに呼び出されます。
        ///     ノーツの移動処理を行います。
        /// </summary>
        private void Update()
        {
            MoveNotes();
        }
        #endregion

        #region Privateメソッド
        /// <summary>
        ///     ノーツを移動させ、終点に到達したノーツを削除します。
        /// </summary>
        private void MoveNotes()
        {
            if (_notes.Count < 1) return;

            float deltaSpeed = _noteSpeed * Time.deltaTime;

            for (int i = 0; i < _notes.Count; i++)
            {
                Image note = _notes[i];

                Vector2 forEndVec = _noteEndPos.anchoredPosition - note.rectTransform.anchoredPosition;

                // 終点までの距離がノーツの速度よりも小さい場合。
                if (forEndVec.magnitude < deltaSpeed)
                {
                    if (Mathf.Approximately(forEndVec.magnitude, 0f))
                    {
                        // ノーツが終点に到達したら削除。
                        Destroy(note.gameObject);
                        _notes.RemoveAt(i);
                    }
                    else
                    {
                        // ノーツを終点に移動させる。
                        note.rectTransform.anchoredPosition = _noteEndPos.anchoredPosition;
                    }
                    continue;
                }

                Vector2 dir = forEndVec.normalized;
                note.rectTransform.anchoredPosition += dir * deltaSpeed;
            }
        }
        #endregion
    }
}
