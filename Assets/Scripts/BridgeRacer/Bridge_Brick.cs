using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge_Brick : MonoBehaviour
{
    [SerializeField] GameObject brick;
    public Bridge_MapGenerator gen;
    public Color myColor;

    // 벽돌을 재생성하는 함수
    // PlayerController의 벽돌 충돌 함수에서 호출된다.
    public void Respawn()
    {
        StartCoroutine("RespawnCo");
    }

    // 벽돌 재생성을 위한 코루틴
    IEnumerator RespawnCo()
    {
        // 5초 후에 재생성
        yield return new WaitForSeconds(5);

        GameObject newBrick = Instantiate(brick, transform);
        newBrick.transform.localPosition = Vector3.zero;
        newBrick.transform.localRotation = Quaternion.identity;
        newBrick.transform.localScale = Vector3.zero;
        iTween.ScaleTo(newBrick, iTween.Hash("x", brick.transform.localScale.x, "y", brick.transform.localScale.y,
            "z", brick.transform.localScale.z, "time", 0.3f, "easetype", iTween.EaseType.easeOutQuint));

        int ran;
        while (true)
        {
            ran = Random.Range(0, 3);

            // 만약 랜덤하게 생성된 값에 해당하는 색상이 현재 층에 활성화 돼있다면
            if (gen.IsColorExist(ran))
            {
                // 반복문을 탈출한다.
                break;
            }
        }
        
        // 색상을 랜덤하게 지정한다.
        // 이 때 위의 반복문에 의해 색상은 현재 층에 활성화 돼있는 색상중에 선택된다.
        switch (ran)
        {
            case 0:
                myColor = Color.red;
                break;
            case 1:
                myColor = Color.green;
                break;
            case 2:
                myColor = Color.blue;
                break;
        }

        newBrick.GetComponent<MeshRenderer>().material.color = myColor; 
    }
}
