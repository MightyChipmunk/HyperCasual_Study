using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Player Parameters
    float moveSpeed;
    float rotSpeed;
    float jumpSpeed;
    float jumpHeight;

    CharacterController cc;
    Animator animator;
    [SerializeField] GameObject head;

    // Vector Parameters for Move, Rotate
    Vector3 dir;
    Vector2 horizontal;

    // State Parameters for Animation
    enum State
    {
        Idle,
        Move,
        Jump,
    }
    State state;

    // Target Object for Jump
    Transform target;

    void Start()
    {
        var entity = D_Params.GetEntity("PlayerParams");
        moveSpeed = entity.f_MoveSpeed;
        rotSpeed = entity.f_RotateSpeed;
        jumpSpeed = entity.f_JumpSpeed;
        jumpHeight = entity.f_JumpHeight;

        cc = GetComponent<CharacterController>();    
        animator = GetComponent<Animator>();

        dir = Vector3.forward;
        horizontal = Vector2.zero;

        state = State.Idle;
    }

    void Update()
    {
        Rotate();
        Move();
        Anim();
    }

    void Rotate()
    {
        if (Input.touchCount > 0 && state != State.Jump)
        {
            horizontal += Input.GetTouch(0).deltaPosition / 100;
            horizontal.y = 0;
            Debug.Log(horizontal);
            if (horizontal.magnitude > 1)
            {
                horizontal = horizontal.normalized;
            }

            dir = (Vector3)horizontal + Vector3.forward;
        }
        else if (Input.touchCount <= 0 && state != State.Jump)
        {
            dir = Vector3.forward;
            horizontal = Vector2.zero;
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), rotSpeed * Time.deltaTime);
    }

    void Move()
    {
        if (Input.touchCount > 0 && state != State.Jump)
        {
            cc.Move(dir * moveSpeed * Time.deltaTime);
            state = State.Move;
        }
        else if (Input.touchCount <= 0 && state != State.Jump)
            state = State.Idle;

        if (state != State.Jump)
            cc.Move(-transform.up * 0.1f * Time.deltaTime);
    }

    public void StartJump(Transform target)
    {
        this.target = target;
        if (state != State.Jump)
            StartCoroutine(Jump());
    }

    IEnumerator Jump()
    {
        state = State.Jump;
        cc.enabled = false;

        Vector3 pos;
        Vector3 point1 = transform.position;
        Vector3 point2 = transform.position + (target.position - point1) / 2 + Vector3.up * jumpHeight;
        Vector3 point3 = target.position;

        float currentTime = 0;

        while (Vector3.Distance(transform.position, target.position) > 0.03f)
        {
            currentTime += Time.deltaTime;
            yield return null;

            pos = transform.position;
            pos.y = 0;
            dir = target.position - pos;
            dir.Normalize();

            transform.position = Bezier2D(point1, point2, point3, currentTime * jumpSpeed / 10);
        }

        state = State.Idle;
        cc.enabled = true;
    }

    void Anim()
    {
        switch (state)
        {
            case State.Idle:
                animator.SetInteger("State", 0);
                break;
            case State.Move:
                animator.SetInteger("State", 1);
                break;
            case State.Jump:
                animator.SetInteger("State", 2);
                break;
        }
    }

    Vector3 Bezier2D(Vector3 p1, Vector3 p2, Vector3 p3, float ratio)
    {
        Vector3 p01 = Vector3.Lerp(p1, p2, ratio);
        Vector3 p02 = Vector3.Lerp(p2, p3, ratio);
        Vector3 p03 = Vector3.Lerp(p01, p02, ratio);

        return p03;
    }
}
