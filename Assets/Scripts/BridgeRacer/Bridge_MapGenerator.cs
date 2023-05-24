using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge_MapGenerator : MonoBehaviour
{
    [SerializeField] GameObject brick;

    float xPos = -12;
    float zPos = 12;

    // Start is called before the first frame update
    void OnEnable()
    {
        while (zPos >= -2)
        {
            int ran = Random.Range(0, 3);
            GameObject go = Instantiate(brick, transform);
            go.transform.localPosition = new Vector3(xPos, 0.1f, zPos);

            if (xPos < 12)
                xPos += 1;
            else
            {
                xPos = -12;
                zPos -= 1;
            }

            switch (ran)
            {
                case 0:
                    go.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
                    go.GetComponent<Bridge_Brick>().myColor = Color.green;
                    break;
                case 1:
                    go.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
                    go.GetComponent<Bridge_Brick>().myColor = Color.red;
                    break;
                case 2:
                    go.GetComponentInChildren<MeshRenderer>().material.color = Color.blue;
                    go.GetComponent<Bridge_Brick>().myColor = Color.blue;
                    break;
            }
        }
    }
}
