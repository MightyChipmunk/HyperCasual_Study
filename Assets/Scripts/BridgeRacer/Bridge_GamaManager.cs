using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge_GamaManager : MonoBehaviour
{
    public static Bridge_GamaManager Instance;

    [SerializeField] Transform map;
    [SerializeField] GameObject floorFac;
    [SerializeField] GameObject firstFloor;

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
            if (Score % 1000 == 0)
            {
                floorCnt++;
                GameObject newFloor = Instantiate(floorFac, map);
                newFloor.transform.localPosition = new Vector3(0, 0, 5) + floorPos * floorCnt;

                if (firstFloor)
                {
                    firstFloor = newFloor;
                    firstFloor.transform.Find("Bridge").Find("StairCollider").gameObject.SetActive(false);
                }
            }
        }
    }
    
    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Update()
    {
        time += Time.deltaTime;
    }

    private void OnApplicationQuit()
    {
        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            {"score", score },
            {"time", time }
        };
        FireBaseManager.SaveData(data);
    }
}
