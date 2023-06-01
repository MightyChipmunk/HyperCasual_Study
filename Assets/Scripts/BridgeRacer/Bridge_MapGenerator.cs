using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge_MapGenerator : MonoBehaviour
{
    [SerializeField] GameObject brick;

    List<GameObject> bricks = new List<GameObject>();
    Dictionary<string, bool> colors = new Dictionary<string, bool>
    {
        {"red", false },
        {"green", false },
        {"blue", false }
    };
    

    float xPos = -12;
    float zPos = 12;

    // Start is called before the first frame update
    void Start()
    {
        InitBricks();
    }

    void InitBricks()
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
                    go.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
                    go.GetComponent<Bridge_Brick>().myColor = Color.red;
                    break;
                case 1:
                    go.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
                    go.GetComponent<Bridge_Brick>().myColor = Color.green;
                    break;
                case 2:
                    go.GetComponentInChildren<MeshRenderer>().material.color = Color.blue;
                    go.GetComponent<Bridge_Brick>().myColor = Color.blue;
                    break;
            }
            go.GetComponent<Bridge_Brick>().gen = this;
            go.name = transform.parent.name + "Brick";
            bricks.Add(go);
        }

        for (int i = 0; i < bricks.Count; i++)
        {
            bricks[i].SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Bridge_PlayerController pc;
        Bridge_AIController ai;
        if (other.TryGetComponent<Bridge_PlayerController>(out pc))
        {
            if (pc.MyColor == Color.red)
                colors["red"] = true;
            else if (pc.MyColor == Color.green)
                colors["green"] = true;
            else if (pc.MyColor == Color.blue)
                colors["blue"] = true;

            for (int i = 0; i < bricks.Count; i++)
            {
                if (!bricks[i].activeSelf &&
                    bricks[i].GetComponentInChildren<MeshRenderer>().sharedMaterial.color == pc.MyColor)
                    bricks[i].SetActive(true);
            }
        }
        else if (other.TryGetComponent<Bridge_AIController>(out ai))
        {
            if (ai.MyColor == Color.red)
                colors["red"] = true;
            else if (ai.MyColor == Color.green)
                colors["green"] = true;
            else if (ai.MyColor == Color.blue)
                colors["blue"] = true;

            for (int i = 0; i < bricks.Count; i++)
            {
                if (!bricks[i].activeSelf && 
                    bricks[i].GetComponentInChildren<MeshRenderer>().sharedMaterial.color == ai.MyColor)
                    bricks[i].SetActive(true);
            }
        }
    }
    public bool IsColorExist(int i)
    {
        bool value = false;
        switch (i)
        {
            case 0:
                value = colors["red"];
                break;
            case 1:
                value = colors["green"];
                break;
            case 2:
                value = colors["blue"];
                break;
        }
        return value;
    }
}
