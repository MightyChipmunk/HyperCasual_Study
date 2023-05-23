using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge_Brick : MonoBehaviour
{
    [SerializeField] GameObject brick;
    Color myColor;

    private void Start()
    {
        myColor = brick.GetComponent<MeshRenderer>().sharedMaterial.color;
    }

    public void Respawn()
    {
        StartCoroutine("RespawnCo");
    }

    IEnumerator RespawnCo()
    {
        yield return new WaitForSeconds(5);

        GameObject newBrick = Instantiate(brick, transform);
        newBrick.transform.localPosition = Vector3.zero;
        newBrick.transform.localRotation = Quaternion.identity;
        newBrick.transform.localScale = Vector3.zero;
        iTween.ScaleTo(newBrick, iTween.Hash("x", brick.transform.localScale.x, "y", brick.transform.localScale.y,
            "z", brick.transform.localScale.z, "time", 0.3f, "easetype", iTween.EaseType.easeOutQuint));
        newBrick.GetComponent<MeshRenderer>().material.color = myColor;
    }
}
