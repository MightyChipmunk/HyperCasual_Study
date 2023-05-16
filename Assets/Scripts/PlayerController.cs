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

    void Start()
    {
        cc = GetComponent<CharacterController>();    
        animator = GetComponent<Animator>();

        dir = Vector3.forward;
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

            //if (Input.GetTouch(0).phase == TouchPhase.Moved)
            //{
                Vector2 horizontal = Input.GetTouch(0).deltaPosition;
                Debug.Log("asdf");
                if (horizontal.magnitude > 1)
                {
                    horizontal = horizontal.normalized;
                }

                dir = (Vector3)horizontal + Vector3.forward;
            //}
        }
    }
}
