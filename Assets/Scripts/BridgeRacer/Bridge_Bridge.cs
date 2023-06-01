using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge_Bridge : MonoBehaviour
{
    [SerializeField] int openCount = 22;

    int cnt = 0;
    public int Count
    {
        get { return cnt; }
        set 
        { 
            cnt = value; 
            if (cnt >= openCount)
            {
                transform.Find("StairCollider").gameObject.SetActive(false);    
            }
        }
    }
}
