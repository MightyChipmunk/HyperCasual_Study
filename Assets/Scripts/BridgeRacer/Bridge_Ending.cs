using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge_Ending : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // �÷��̾ ���� ������ �����ߴٸ�
        if (other.gameObject.layer == LayerMask.NameToLayer("Player") && !Bridge_GameManager.Instance.IsEnd)
        {
            Bridge_GameManager.Instance.GameEnd(true);
            other.GetComponent<Bridge_PlayerController>().Win(transform);
        }
        // AI�� ������ ���� �����ߴٸ�
        else if (other.gameObject.layer == LayerMask.NameToLayer("AI") && !Bridge_GameManager.Instance.IsEnd)
        {
            Bridge_GameManager.Instance.GameEnd(false);
            other.GetComponent<Bridge_AIController>().Win(transform);
        }
    }
}
