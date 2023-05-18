using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] GameObject Flag;
    [SerializeField] GameObject Obstacle;
    [SerializeField] GameObject JumpPad;

    List<D_Stage1> stageObjs = new List<D_Stage1>();

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < D_Stage1.CountEntities; i++)
        {
            stageObjs.Add(D_Stage1.GetEntity(i));
        }

        GameObject jumpStart = new GameObject();
        int startIdx = 0;

        for (int i = 0; i < stageObjs.Count; i++)
        {
            GameObject go = new GameObject();
            switch (stageObjs[i].f_name)
            {
                case "flag":
                    go = Instantiate(Flag, gameObject.transform);
                    go.GetComponentInChildren<TMP_Text>().text = stageObjs[i].f_Param;
                    break;
                case "obstacle":
                    go = Instantiate(Obstacle, gameObject.transform);
                    break;
                case "jump_pad_start":
                    go = Instantiate(JumpPad, gameObject.transform);
                    jumpStart = go;
                    startIdx = i;
                    break;
                case "jump_pad_end":
                    go = null;
                    jumpStart.transform.GetChild(0).localPosition += 
                        Vector3.forward * (i - startIdx) * 5 / jumpStart.transform.localScale.x;
                    break;
            }

            if (go)
                go.transform.localPosition += Vector3.forward * (i + 1) * 5;
        }
    }
}
