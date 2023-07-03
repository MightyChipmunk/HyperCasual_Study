using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge_MapGenerator : MonoBehaviour
{
    [SerializeField] GameObject brick;

    // 벽돌들을 담는 리스트. 리스트를 순회하며 색상을 구분하기 위해 사용한다.
    List<GameObject> bricks = new List<GameObject>();
    // 현재 층에 도착한 플레이어와 AI의 색상을 확인하기 위한 딕셔너리
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

    // 층에 벽돌을 생성하는 함수
    void InitBricks()
    {
        while (zPos >= -2)
        {
            int ran = Random.Range(0, 3);
            GameObject go = Instantiate(brick, transform);
            go.transform.localPosition = new Vector3(xPos, 0.1f, zPos);

            // x,z 위치를 지정해서 생성
            if (xPos < 12)
                xPos += 1;
            else
            {
                xPos = -12;
                zPos -= 1;
            }

            // 랜덤으로 색상을 지정한다.
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

            // name 설정은 디버그를 위해, 그리고 Brick 스크립트의 gen 변수를 지정해준다.
            go.GetComponent<Bridge_Brick>().gen = this;
            go.name = transform.parent.name + "Brick";
            // 그리고 생성한 벽돌들을 List에 추가한다.
            bricks.Add(go);
        }

        // 플레이어가 같은 층에 도착하기 전까지는 비활성화
        for (int i = 0; i < bricks.Count; i++)
        {
            bricks[i].SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Bridge_PlayerController pc;
        Bridge_AIController ai;
        // 만약 플레이어가 층에 도착했다면
        if (other.TryGetComponent<Bridge_PlayerController>(out pc))
        {
            // 플레이어의 색상을 확인하고 딕셔너리의 값을 수정한다.
            if (pc.MyColor == Color.red)
                colors["red"] = true;
            else if (pc.MyColor == Color.green)
                colors["green"] = true;
            else if (pc.MyColor == Color.blue)
                colors["blue"] = true;

            // 색상에 맞는 벽돌들을 활성화시킨다.
            for (int i = 0; i < bricks.Count; i++)
            {
                if (!bricks[i].activeSelf &&
                    bricks[i].GetComponentInChildren<MeshRenderer>().sharedMaterial.color == pc.MyColor)
                    bricks[i].SetActive(true);
            }
        }
        // 만약 AI가 층에 도착했다면
        else if (other.TryGetComponent<Bridge_AIController>(out ai))
        {
            // AI의 색상을 확인하고 딕셔너리의 값을 수정한다.
            if (ai.MyColor == Color.red)
                colors["red"] = true;
            else if (ai.MyColor == Color.green)
                colors["green"] = true;
            else if (ai.MyColor == Color.blue)
                colors["blue"] = true;

            // 색상에 맞는 벽돌들을 활성화시킨다.
            for (int i = 0; i < bricks.Count; i++)
            {
                if (!bricks[i].activeSelf && 
                    bricks[i].GetComponentInChildren<MeshRenderer>().sharedMaterial.color == ai.MyColor)
                    bricks[i].SetActive(true);
            }
        }
    }

    // 벽돌 재생성 시 현재 층에 어떤 색상이 활성화 돼있는지 확인시켜주는 함수
    public bool IsColorExist(int i)
    {
        bool value = false;
        // i의 값은 랜덤한 값을 받아온다.
        // 그 랜덤한 값에 해당하는 색상이 활성화 돼있는지 반환해준다.
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
