using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RythemManager : MonoBehaviour
{
    [Header("再生する音楽のパラメーター")]
    public float BPM;
    [SerializeField] private float speed;

    [Header("ノーツのオブジェクト")]
    [SerializeField] private Vector2 ScorePosition;
    [SerializeField] private Vector2 NotesPosition;
    [SerializeField] private float endPosition;
    [Space]

    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject ScorePrefab;
    [SerializeField] private GameObject RednotesPrefab;

    [Header("Audioの設定")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip HighHat;
    [SerializeField] private AudioClip BassDrum;

    private float NotesSpawnSpeed;
    private float ScoreSpeed;
    private double Timer;
    void Start()
    {
        NotesSpawnSpeed = 30 / BPM;
        ScoreSpeed = 120 / BPM * speed;

        audioSource = GetComponent<AudioSource>();
    }

    private void ScoreSpawn(float ScoreHight)
    {
        GameObject ScoreClone = Instantiate(ScorePrefab, canvas.transform);
        RectTransform rectTransform = ScoreClone.GetComponent<RectTransform>();

        rectTransform.anchoredPosition = ScorePosition;
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, ScoreHight);

        NotesManager notesManager = ScoreClone.GetComponent<NotesManager>();
        notesManager.GetInfomation(ScorePosition.x, endPosition, ScoreSpeed, ScorePosition.x);
    }

    public void RedNotesSpawn()
    {
        GameObject notesClone = Instantiate(RednotesPrefab, canvas.transform);
        RectTransform rectTransform = notesClone.GetComponent<RectTransform>();

        rectTransform.anchoredPosition = NotesPosition;

        NotesManager notesManager = notesClone.GetComponent<NotesManager>();
        notesManager.GetInfomation(ScorePosition.x, endPosition, ScoreSpeed, NotesPosition.x);

        audioSource.PlayOneShot(HighHat);
    }

    void Update()
    {
        if (Time.time - Timer >= NotesSpawnSpeed)
        {
            if(Timer % (NotesSpawnSpeed * 4) == 0)
            {
                ScoreSpawn(100);
                audioSource.PlayOneShot(BassDrum);
            } 
            else if (Timer % (NotesSpawnSpeed * 2) == 0)
            {
                ScoreSpawn(75);
            }
            else
            {
                ScoreSpawn(50);
            }

            Timer += NotesSpawnSpeed;
        }
    }
}
