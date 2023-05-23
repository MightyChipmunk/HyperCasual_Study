using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Bridge_GamaManager : MonoBehaviour
{
    public static Bridge_GamaManager Instance;

    [SerializeField] Transform map;
    [SerializeField] GameObject floorFac;
    [SerializeField] GameObject currentFloor;

    [SerializeField] TMP_Text highScore;
    [SerializeField] TMP_Text highTime;
    [SerializeField] TMP_Text currentScore;
    [SerializeField] TMP_Text currentTime;

    int score = 0;
    float time = 0;

    int floorCnt = 0;
    Vector3 floorPos = new Vector3(0.0f, 3.3249998092651369f, 25.32000160217285f);
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
        time += Time.deltaTime;
        currentTime.text = time.ToString().Length < 5 ? "Time: " + time.ToString() : "Time: " + time.ToString().Substring(0, 5);
    }

    private void OnApplicationQuit()
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
}
