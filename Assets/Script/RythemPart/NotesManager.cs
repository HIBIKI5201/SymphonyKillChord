using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotesManager : MonoBehaviour
{
    private float endPosition;
    private float movePosition;
    private float spawnPosition;

    private RectTransform rectTransform;
    private float Timer;
    private float speed;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void GetInfomation(float ScorePosition, float EndPosition, float Speed, float SpawnPosition)
    {
        endPosition = EndPosition;
        movePosition = ScorePosition - endPosition;

        spawnPosition = SpawnPosition - endPosition;

        speed = Speed;
        Timer = spawnPosition / movePosition * Speed;
    }

    void FixedUpdate()
    {
        Timer -= Time.fixedDeltaTime;
        rectTransform.anchoredPosition = new Vector2(endPosition + (movePosition * (Timer / speed)), rectTransform.anchoredPosition.y);
        
        if (rectTransform.anchoredPosition.x <= -1000)
        {
            Destroy(gameObject);
        }
    }
}
