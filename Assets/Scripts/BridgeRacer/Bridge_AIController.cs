using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge_AIController : MonoBehaviour
{
    // Player Parameters
    float moveSpeed;
    float rotSpeed;

    CharacterController cc;
    Animator animator;
    Transform brickSlot;
    [SerializeField] Color myColor;
    [SerializeField] GameObject Stair;
    Stack<GameObject> bricks = new Stack<GameObject>();

    GameObject[] nearBricks;

    // Vector Parameters for Move, Rotate
    Vector3 dir;

    // State Parameters for Animation
    enum State
    {
        Idle,
        Move,
    }
    State state;

    void Start()
    {
        var entity = D_Params.GetEntity("BridgeParams");
        moveSpeed = entity.f_MoveSpeed;
        rotSpeed = entity.f_RotateSpeed;

        cc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        brickSlot = transform.Find("BrickSlot");

        dir = Vector3.zero;
        state = State.Idle;
    }

    void Update()
    {
        if (nearBricks == null || nearBricks.Length == 0)
        {
            nearBricks = GameObject.FindGameObjectsWithTag("Brick");
        }

        Move();
        Rotate();
        switch (state)
        {
            case State.Idle:
                animator.SetInteger("State", 0);
                break;
            case State.Move:
                animator.SetInteger("State", 1);
                break;
        }
    }

    float dist = 1000;
    void Rotate()
    {
        if (nearBricks == null || nearBricks.Length == 0) return;

        int nearest = 0;
        for (int i = 0; i < nearBricks.Length; i++)
        {
            if (BrickDist(nearBricks[i].transform.position) < dist)
            {
                dist = BrickDist(nearBricks[i].transform.position);
                nearest = i;
            }
        }

        Debug.Log(nearest);
        dir = nearBricks[nearest].transform.position - transform.position;
        dir.Normalize();
    }

    float BrickDist(Vector3 brickPos)
    {
        float dist = Mathf.Pow(Mathf.Pow(brickPos.x - transform.position.x, 2) 
            + Mathf.Pow(brickPos.z - transform.position.z, 2)
            + Mathf.Pow((brickPos.y - transform.position.y) * 10, 2), 1f / 3f);
        Debug.Log(dist);
        return dist;
    }

    void Move()
    {
        if (dir.magnitude > 0.2f)
        {
            cc.Move(transform.forward * moveSpeed * Time.deltaTime);
            state = State.Move;
        }
        else
            state = State.Idle;

        cc.Move(-transform.up * 9.81f * Time.deltaTime);
    }

    int stairCnt = 0;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Brick") && other.GetComponent<MeshRenderer>().material.color == myColor)
        {
            other.transform.parent.GetComponent<Bridge_Brick>().Respawn();

            other.transform.parent = brickSlot;
            other.transform.localEulerAngles = Vector3.zero;
            iTween.MoveTo(other.gameObject, iTween.Hash("x", 0, "y", 0.125f * brickSlot.childCount, "z", 0, "islocal", true, "time", 0.2f, "easetype", iTween.EaseType.easeOutQuint));
            other.GetComponent<TrailRenderer>().enabled = false;
            other.GetComponent<BoxCollider>().enabled = false;
            bricks.Push(other.gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Bridge") && bricks.Count > 0)
        {
            Destroy(bricks.Pop());
            GameObject newStair = Instantiate(Stair, other.transform.parent);
            newStair.transform.localEulerAngles = Vector3.zero;
            newStair.transform.localPosition = other.transform.localPosition;
            newStair.GetComponent<MeshRenderer>().material.color = myColor;
            other.transform.localPosition += new Vector3(0, 0.5f, 0.8f);
            other.transform.parent.GetComponent<Bridge_Bridge>().Count++;
        }
    }
}
