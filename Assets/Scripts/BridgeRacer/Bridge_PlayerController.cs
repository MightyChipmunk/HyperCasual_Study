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
    // 층에 자신의 색을 알리기 위한 프로퍼티
    public Color MyColor
    {
        get { return myColor; }
    }
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
        // 터치 입력이 있다면 dir에 값을 대입한다.
        if (Input.touchCount > 0)
            dir = SimpleInput.GetAxis("Horizontal") * Vector3.right + SimpleInput.GetAxis("Vertical") * Vector3.forward;
        // 터치 입력이 없다면 dir을 0으로 초기화한다.
        else
            dir = Vector3.zero;

        // 만약 dir이 1보다 크다면 dir의 크기를 1로 바꾼다. (dir은 방향만 나타내는 방향벡터기 때문)
        if (dir.magnitude > 1)
            dir.Normalize();
        // dir의 크기가 0이 아니라면(터치 입력이 있다면) 플레이어를 회전시킨다.
        else if (dir.magnitude > 0.2f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), rotSpeed * 2 * Time.deltaTime);
    }

    void Move()
    {
        // 터치 입력이 있다면 State를 Move로 바꾸고 플레이어가 바라보고 있는 방향으로 이동한다.
        if (dir.magnitude > 0.2f && Input.touchCount > 0)
        {
            cc.Move(transform.forward * moveSpeed * Time.deltaTime);
            state = State.Move;
        }
        // 터치 입력이 없다면 State를 Idle로 바꾸고 이동하지 않는다.
        else
            state = State.Idle;

        // 중력의 영향을 받으며 항상 아래로 이동한다.
        cc.Move(-transform.up * 9.81f * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 충돌한 물체가 벽돌이고 자신의 색과 같다면
        if (other.gameObject.layer == LayerMask.NameToLayer("Brick") 
            && other.GetComponentInParent<Bridge_Brick>().myColor == myColor)
        {
            other.transform.parent.GetComponent<Bridge_Brick>().Respawn();

            // 벽돌의 위치를 BrickSlot으로 옮긴다.
            other.transform.parent = brickSlot;
            other.transform.localEulerAngles = Vector3.zero;
            // BrickSlot의 Brick 개수만큼 y Position을 더한다.
            iTween.MoveTo(other.gameObject, iTween.Hash("x", 0, "y", 0.125f * brickSlot.childCount, "z", 0, "islocal", true, "time", 0.2f, "easetype", iTween.EaseType.easeOutQuint));
            // 벽돌의 TrailRenderer와 Collider를 끈다.
            other.GetComponent<TrailRenderer>().enabled = false;
            other.GetComponent<BoxCollider>().enabled = false;
            // 스택에 벽돌을 Push한다.
            bricks.Push(other.gameObject);
        }
        // 충돌한 물체가 다리이고 내가 벽돌을 한개 이상 가지고 있다면
        else if (other.gameObject.layer == LayerMask.NameToLayer("Bridge") && bricks.Count > 0)
        {
            // 벽돌을 하나 꺼낸다.
            Destroy(bricks.Pop());
            // 꺼낸 벽돌을 계단으로 만들어 다리에 생성한다.
            GameObject newStair = Instantiate(Stair, other.transform.parent);
            newStair.transform.localEulerAngles = Vector3.zero;
            newStair.transform.localPosition = other.transform.localPosition;
            newStair.GetComponent<MeshRenderer>().material.color = myColor;
            other.transform.localPosition += new Vector3(0, 0.5f, 0.8f);
            // 점수 추가
            Bridge_GameManager.Instance.Score += 100;
            // 충돌한 Bridge의 벽돌 개수 추가
            other.transform.parent.GetComponent<Bridge_Bridge>().Count++;
        }
    }
}
