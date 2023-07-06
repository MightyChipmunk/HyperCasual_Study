using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge_Ending : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 플레이어가 먼저 엔딩에 도착했다면
        if (other.gameObject.layer == LayerMask.NameToLayer("Player") && !Bridge_GameManager.Instance.IsEnd)
        {
            Bridge_GameManager.Instance.GameEnd(true);
            other.GetComponent<Bridge_PlayerController>().Win(transform);
        }
        // AI가 엔딩에 먼저 도착했다면
        else if (other.gameObject.layer == LayerMask.NameToLayer("AI") && !Bridge_GameManager.Instance.IsEnd)
        {
            Bridge_GameManager.Instance.GameEnd(false);
            other.GetComponent<Bridge_AIController>().Win(transform);
        }
    }
}
