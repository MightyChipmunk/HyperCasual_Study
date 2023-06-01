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
    public Color MyColor
    {
        get { return myColor; }
    }
    [SerializeField] GameObject Stair;
    Transform bridge;
    Stack<GameObject> bricks = new Stack<GameObject>();

    GameObject[] nearBricks;

    // Vector Parameters for Move, Rotate
    Vector3 dir;

    // State Parameters for Animation
    enum State
    {
        Move,
        GoToBridge,
        Climb,
        BackToFloor,
    }
    [SerializeField] State state;

    void Start()
    {
        var entity = D_Params.GetEntity("BridgeParams");
        moveSpeed = entity.f_MoveSpeed;
        rotSpeed = entity.f_RotateSpeed;

        cc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        brickSlot = transform.Find("BrickSlot");

        dir = Vector3.forward;
        state = State.Move;
    }

    void Update()
    {
        if (nearBricks == null || nearBricks.Length == 0)
        {
            GetNearBricks();
        }

        switch (state)
        {
            case State.Move:
                Move();
                Rotate();
                animator.SetInteger("State", 1);
                break;
            case State.GoToBridge:
                Move();
                GoToBridge();
                animator.SetInteger("State", 1);
                break;
            case State.Climb:
                Move();
                Climb();
                animator.SetInteger("State", 1);
                break;
            case State.BackToFloor:
                Move();
                BackToFloor();
                animator.SetInteger("State", 1);
                break;
        }
    }

    void GetNearBricks()
    {
        dist = 10000;
        nearBricks = null;
        nearBricks = GameObject.FindGameObjectsWithTag("Brick");
        for (int i = 0; i < nearBricks.Length; i++)
        {
            if (Mathf.Abs(nearBricks[i].transform.position.y - transform.position.y) > 5 ||
                nearBricks[i].transform.parent == brickSlot)
            {
                nearBricks[i] = null;
            }
        }
    }

    float dist = 10000;
    int nearest = 0;
    void Rotate()
    {
        if (nearBricks == null || nearBricks.Length == 0) return;

        if (dist >= 10000 && bricks.Count < 10)
        {
            for (int i = 0; i < nearBricks.Length; i++)
            {
                if (nearBricks[i] != null 
                    && nearBricks[i].GetComponent<MeshRenderer>().sharedMaterial.color == myColor
                    && Vector3.Distance(nearBricks[i].transform.position, transform.position) < dist)
                {
                    dist = Vector3.Distance(nearBricks[i].transform.position, transform.position);
                    nearest = i;
                }
            }
        }
        
        if (nearBricks[nearest])
            dir = nearBricks[nearest].transform.position - transform.position;
        dir.y = 0;
        dir.Normalize();

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotSpeed);
    }

    void Move()
    {
        cc.Move(transform.forward * moveSpeed * Time.deltaTime);
        cc.Move(-transform.up * 9.81f * Time.deltaTime);
    }

    void GoToBridge()
    {
        bridge = FindBridge();
        if (bridge)
            dir = bridge.position - transform.position;
        dir.y = 0;
        dir.Normalize();

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotSpeed);
    }

    Transform FindBridge()
    {
        GameObject[] bridges = GameObject.FindGameObjectsWithTag("Bridge");
        Transform target = null;
        float dist = 10000;

        for (int i = 0; i < bridges.Length; i++)
        {
            if ((transform.position.y - bridges[i].transform.position.y <= 1f) && 
                Vector3.Distance(transform.position, bridges[i].transform.position) <= dist)
            {
                target = bridges[i].transform;
                dist = Vector3.Distance(transform.position, bridges[i].transform.position);
            }
        }

        return target;
    }

    void BackToFloor()
    {
        dir = -Vector3.forward;
        dir.Normalize();

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotSpeed * 4);
    }

    void Climb()
    {
        dir = Vector3.forward;
        dir.Normalize();

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotSpeed * 4);

        if (bricks.Count <= 0)
            state = State.BackToFloor;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Brick") 
            && other.transform.parent.GetComponent<Bridge_Brick>().myColor == myColor)
        {
            other.transform.parent.GetComponent<Bridge_Brick>().Respawn();

            other.transform.parent = brickSlot;
            other.transform.localEulerAngles = Vector3.zero;
            iTween.MoveTo(other.gameObject, iTween.Hash("x", 0, "y", 0.125f * brickSlot.childCount, "z", 0, "islocal", true, "time", 0.2f, "easetype", iTween.EaseType.easeOutQuint));
            other.GetComponent<TrailRenderer>().enabled = false;
            other.GetComponent<BoxCollider>().enabled = false;
            bricks.Push(other.gameObject);

            for (int i = 0; i < nearBricks.Length; i++)
            {
                if (nearBricks[i] == other.gameObject)
                {
                    nearBricks[i] = null;
                    dist = 10000;
                }
            }

            if (bricks.Count >= 10)
                state = State.GoToBridge;
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

        if (other.gameObject.layer == LayerMask.NameToLayer("Floor"))
        {
            state = State.Move;
            GetNearBricks();
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (state == State.GoToBridge && hit.gameObject.layer == LayerMask.NameToLayer("Bridge") && bricks.Count > 0)
            state = State.Climb;

        //if (hit.gameObject.layer == LayerMask.NameToLayer("Floor") && bricks.Count <= 0)
        //{
        //    state = State.Move;
        //    GetNearBricks();
        //}
    }
}
