using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Bridge_GameManager : MonoBehaviour
{
    public static Bridge_GameManager Instance;

    [SerializeField] TMP_Text highScore;
    [SerializeField] TMP_Text highTime;
    [SerializeField] TMP_Text currentScore;
    [SerializeField] TMP_Text currentTime;

    int score = 0;
    float time = 0;

    bool isEnd = false;
    public bool IsEnd
    {
        get { return isEnd; }
    }

    public int Score
    {
        get { return score; }
        set 
        { 
            score = value;
            currentScore.text = "Score: " + score.ToString();
        }
    }

    Dictionary<string, object> data = new Dictionary<string, object>();

    // Start is called before the first frame update
    IEnumerator Start()
    {
        if (Instance == null)
            Instance = this;

        FireBaseManager.GetData("RytjcXeghx0H4WdsLJL9", (data) =>
        {
            this.data = data;
        });

        while (data.Count < 1)
        {
            yield return null;
        }

        SetHighScore();
    }

    void SetHighScore()
    {
        highScore.text = "HighScore: " + data["score"].ToString();
        highTime.text = "HighTime: " + data["time"].ToString().Substring(0, 5);
    }

    private void Update()
    {
        if (isEnd) return;

        time += Time.deltaTime;
        currentTime.text = time.ToString().Length < 5 ? "Time: " + time.ToString() : "Time: " + time.ToString().Substring(0, 5);
    }

    public void GameEnd(bool playerWin)
    {
        if (playerWin)
        {
            if (score <= Int32.Parse(data["score"].ToString()))
                return;

            Dictionary<string, object> newData = new Dictionary<string, object>()
            {
                {"score", score },
                {"time", time }
            };

            FireBaseManager.SaveData(newData);
        }

        isEnd = true;
    }
}
