using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge_MapGenerator : MonoBehaviour
{
    [SerializeField] GameObject brick;

    float xPos = -4;
    float zPos = 14;

    // Start is called before the first frame update
    void Start()
    {
        while (zPos >= 0)
        {
            int ran = Random.Range(0, 3);
            GameObject go = Instantiate(brick, transform);
            go.transform.localPosition = new Vector3(xPos, 0.1f, zPos);

            if (xPos < 4)
                xPos += 2;
            else
            {
                xPos = -4;
                zPos -= 1;
            }

            switch (ran)
            {
                case 0:
                    go.GetComponent<MeshRenderer>().material.color = Color.green;
                    break;
                case 1:
                    go.GetComponent<MeshRenderer>().material.color = Color.red;
                    break;
                case 2:
                    go.GetComponent<MeshRenderer>().material.color = Color.blue;
                    break;
            }
        }
    }
}