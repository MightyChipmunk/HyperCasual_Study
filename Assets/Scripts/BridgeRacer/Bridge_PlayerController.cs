using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge_PlayerController : MonoBehaviour
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

        dir = Vector3.forward;

        state = State.Idle;
    }

    void Update()
    {
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
    void Rotate()
    {
        if (Input.touchCount > 0)
            dir = SimpleInput.GetAxis("Horizontal") * Vector3.right + SimpleInput.GetAxis("Vertical") * Vector3.forward;
        else
            dir = Vector3.zero;

        if (dir.magnitude > 1)
            dir.Normalize();
        else if (dir.magnitude > 0.2f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), rotSpeed * 2 * Time.deltaTime);
    }

    void Move()
    {
        if (dir.magnitude > 0.2f && Input.touchCount > 0)
        {
            cc.Move(transform.forward * moveSpeed * Time.deltaTime);
            state = State.Move;
        }
        else 
            state = State.Idle;

        cc.Move(-transform.up * 9.81f * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Brick") 
            && other.GetComponentInParent<Bridge_Brick>().myColor == myColor)
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
            Bridge_GameManager.Instance.Score += 100;
            other.transform.parent.GetComponent<Bridge_Bridge>().Count++;
        }
    }
}
