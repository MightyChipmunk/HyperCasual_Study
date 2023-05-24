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
    [SerializeField] Transform stairCollider;
    Stack<GameObject> bricks = new Stack<GameObject>();

    GameObject[] nearBricks;

    // Vector Parameters for Move, Rotate
    Vector3 dir;

    // State Parameters for Animation
    enum State
    {
        Idle,
        Move,
        GoToBridge,
        BackToFloor,
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
            GetNearBricks();
        }

        Move();
        Rotate();
        switch (state)
        {
            case State.Idle:
                Move();
                Rotate();
                animator.SetInteger("State", 0);
                break;
            case State.Move:
                Move();
                Rotate();
                animator.SetInteger("State", 1);
                break;
            case State.GoToBridge:
                Move();
                GoToBridge();
                break;
            case State.BackToFloor:
                Move();
                BackToFloor();
                break;
        }
    }

    void GetNearBricks()
    {
        nearBricks = GameObject.FindGameObjectsWithTag("Brick");
        for (int i = 0; i < nearBricks.Length; i++)
        {
            if (Mathf.Abs(nearBricks[i].transform.position.y - transform.position.y) > 5)
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
        
        dir = nearBricks[nearest].transform.position - transform.position;
        dir.Normalize();

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotSpeed);
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

    void GoToBridge()
    {

        if (bricks.Count <= 0)
            state = State.BackToFloor;
    }

    void BackToFloor()
    {
        if (/*touch floor*/true)
            state = State.Move;
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
    }
}
