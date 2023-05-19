using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TallMan_PlayerController : MonoBehaviour
{
    // Player Parameters
    float moveSpeed;
    float rotSpeed;
    float jumpSpeed;
    float jumpHeight;
    float sprintSpeed;

    float headSize = 100;
    public float HeadSize
    {
        get { return headSize; }
        set 
        {
            headSize = value; 
            headSize = Mathf.Clamp(headSize, 100, 1000);
            head.localScale = Vector3.one * (headSize / 100f);
            cc.radius = (headSize / 100f) * 0.12f;
            cc.center = new Vector3(0, (headSize / 100f - 1) * cc.radius + 0.48f, -0.05f);
            cc.height = (headSize / 100f - 1) * cc.radius * 2 + 0.96f;
        }
    }

    CharacterController cc;
    Animator animator;
    [SerializeField] Transform head;

    // Vector Parameters for Move, Rotate
    Vector3 dir;
    Vector2 horizontal;

    // State Parameters for Animation
    enum State
    {
        Idle,
        Move,
        Jump,
        Sprint,
    }
    State state;

    void Start()
    {
        var entity = D_Params.GetEntity("TallManParams");
        moveSpeed = entity.f_MoveSpeed;
        rotSpeed = entity.f_RotateSpeed;
        jumpSpeed = entity.f_JumpSpeed;
        jumpHeight = entity.f_JumpHeight;
        sprintSpeed = entity.f_SprintSpeed;

        cc = GetComponent<CharacterController>();    
        animator = GetComponent<Animator>();

        dir = Vector3.forward;
        horizontal = Vector2.zero;

        state = State.Idle;
    }

    void Update()
    {
        switch (state)
        {
            case State.Idle:
                Move();
                animator.SetInteger("State", 0);
                break;
            case State.Move:
                Move();
                Rotate();
                animator.SetInteger("State", 1);
                break;
            case State.Jump:
                animator.SetInteger("State", 2);
                break;
            case State.Sprint:
                Sprint();
                animator.SetInteger("State", -1);
                break;
        }
    }

    void Rotate()
    {
        if (Input.touchCount > 0)
        {
            horizontal += Input.GetTouch(0).deltaPosition / 500 * rotSpeed;
            horizontal.y = 0;

            if (horizontal.magnitude > 10)
            {
                horizontal = horizontal.normalized * 10;
            }

            dir = (Vector3)horizontal + Vector3.forward;
            dir.Normalize();
        }
        else if (Input.touchCount <= 0)
        {
            dir = Vector3.forward;
            horizontal = Vector2.zero;
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), rotSpeed * 2 * Time.deltaTime);
    }

    void Move()
    {
        if (Input.touchCount > 0)
        {
            cc.Move(dir * moveSpeed * Time.deltaTime);
            state = State.Move;
        }
        else if (Input.touchCount <= 0)
            state = State.Idle;

        cc.Move(-transform.up * 0.1f * Time.deltaTime);
    }

    IEnumerator Jump(Transform target)
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
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), rotSpeed * 3 * Time.deltaTime);
        }

        state = State.Idle;
        cc.enabled = true;
    }

    void Sprint()
    {
        cc.Move((Vector3.forward * sprintSpeed - transform.up * 0.1f) * Time.deltaTime);
    }

    Vector3 Bezier2D(Vector3 p1, Vector3 p2, Vector3 p3, float ratio)
    {
        Vector3 p01 = Vector3.Lerp(p1, p2, ratio);
        Vector3 p02 = Vector3.Lerp(p2, p3, ratio);
        Vector3 p03 = Vector3.Lerp(p01, p02, ratio);

        return p03;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("JumpPad"))
        {
            StartCoroutine(Jump(other.transform.GetChild(0)));
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Flag"))
        {
            string text = other.GetComponentInChildren<TMP_Text>().text;
            if (text.Substring(0, 1) == "+")
                HeadSize += Int32.Parse(text.Substring(1));
            else if (text.Substring(0, 1) == "-")
                HeadSize -= Int32.Parse(text.Substring(1));
            else if (text.Substring(0, 1) == "%")
                HeadSize /= float.Parse(text.Substring(1));
            else if (text.Substring(0, 1) == "X")
                HeadSize *= float.Parse(text.Substring(1));
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            HeadSize -= 15;
            Destroy(other.gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Sprint"))
        {
            state = State.Sprint;
            animator.SetTrigger("Sprint");
        }
    }
}
