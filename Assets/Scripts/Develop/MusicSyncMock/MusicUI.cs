using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicUI : MonoBehaviour
{
    [SerializeField]
    private RectTransform _notesParent;
    [SerializeField]
    private RectTransform _originPos;
    [SerializeField]
    private RectTransform _noteEndPos;
    [SerializeField]
    private float _noteSpeed;

    [SerializeField]
    private Image _notePrefab;

    private List<Image> _notes = new();

    public void CreateNote(Color color)
    {
        Image note = Instantiate(_notePrefab);
        note.transform.SetParent(_notesParent, false);
        note.rectTransform.anchoredPosition = _originPos.anchoredPosition;
        note.color = color;

        _notes.Add(note);
    }

    private void Update()
    {
        MoveNotes();
    }

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
}
