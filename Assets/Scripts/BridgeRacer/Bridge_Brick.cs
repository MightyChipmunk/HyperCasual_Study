using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge_Brick : MonoBehaviour
{
    [SerializeField] GameObject brick;
    public Bridge_MapGenerator gen;
    public Color myColor;

    // ������ ������ϴ� �Լ�
    // PlayerController�� ���� �浹 �Լ����� ȣ��ȴ�.
    public void Respawn()
    {
        StartCoroutine("RespawnCo");
    }

    // ���� ������� ���� �ڷ�ƾ
    IEnumerator RespawnCo()
    {
        // 5�� �Ŀ� �����
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

            // ���� �����ϰ� ������ ���� �ش��ϴ� ������ ���� ���� Ȱ��ȭ ���ִٸ�
            if (gen.IsColorExist(ran))
            {
                // �ݺ����� Ż���Ѵ�.
                break;
            }
        }
        
        // ������ �����ϰ� �����Ѵ�.
        // �� �� ���� �ݺ����� ���� ������ ���� ���� Ȱ��ȭ ���ִ� �����߿� ���õȴ�.
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
