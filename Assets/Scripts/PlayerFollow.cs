using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFollow : MonoBehaviour
{
    GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Bridge_GameManager.Instance.IsEnd)
        {
            transform.position = new Vector3(0, 19.0599995f, 82.8199997f);
        }
        else
            transform.position = player.transform.position;
    }
}
