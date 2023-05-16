using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Player Parameters
    [SerializeField] float speed;

    CharacterController cc;
    Animator animator;
    [SerializeField] GameObject head;

    Vector3 dir;
    Vector2 horizontal;

    void Start()
    {
        cc = GetComponent<CharacterController>();    
        animator = GetComponent<Animator>();

        dir = Vector3.forward;
        horizontal = Vector2.zero;
    }

    void Update()
    {
        Move();   
    }

    void Move()
    {
        if (Input.touchCount > 0)
        {
            cc.Move(dir.normalized * speed * Time.deltaTime);

            horizontal += Input.GetTouch(0).deltaPosition;
            horizontal.y = 0;
            Debug.Log("Asdf");
            if (horizontal.magnitude > 1)
            {
                horizontal = horizontal.normalized;
            }

            dir = (Vector3)horizontal + Vector3.forward;
        }
    }
}
