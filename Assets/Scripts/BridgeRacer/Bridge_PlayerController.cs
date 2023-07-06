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
    Transform brickStack;
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
        brickStack = transform.Find("BrickStack");

        dir = Vector3.forward;

        state = State.Idle;
    }

    void Update()
    {
        if (Bridge_GameManager.Instance.IsEnd) return;

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
        // 만약 피격 모션 중이라면 움직이지 못한다.
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("GetHit"))
            return;

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

    public void Win(Transform endFloor)
    {
        // 애니메이션 재생
        animator.SetTrigger("End");
        // 포디엄에 위치하면서
        transform.position = endFloor.transform.position;
        // 카메라를 쳐다본다.
        transform.rotation = Quaternion.LookRotation(-Vector3.forward);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 충돌한 물체가 벽돌이고 자신의 색과 같다면
        if (other.gameObject.layer == LayerMask.NameToLayer("Brick") &&
            other.transform.parent.GetComponent<Bridge_Brick>().myColor == myColor)
        {
            other.transform.parent.GetComponent<Bridge_Brick>().Respawn();

            // 벽돌의 위치를 BrickSlot으로 옮긴다.
            other.transform.parent = brickStack;
            other.transform.localEulerAngles = Vector3.zero;
            // BrickSlot의 Brick 개수만큼 y Position을 더한다.
            iTween.MoveTo(other.gameObject, iTween.Hash("x", 0, "y", 0.125f * brickStack.childCount, "z", 0, "islocal", true, "time", 0.2f, "easetype", iTween.EaseType.easeOutQuint));
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

        // 충돌한 물체가 다른 AI의 벽돌 스택이라면
        if (other.gameObject.layer == LayerMask.NameToLayer("BrickStack") && other.transform.parent != transform)
        {
            other.GetComponentInParent<Bridge_AIController>().GetHitted();
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // 만약 충돌한 물체가 회색 벽돌이라면 (회색 벽돌은 Trigger가 꺼져있다.)
        if (hit.gameObject.layer == LayerMask.NameToLayer("Brick") && hit.gameObject.GetComponent<MeshRenderer>().sharedMaterial.color == Color.gray)
        {
            // 벽돌의 위치를 BrickSlot으로 옮긴다.
            hit.transform.parent = brickStack;
            hit.transform.localEulerAngles = Vector3.zero;
            // 벽돌의 TrailRenderer와 Collider를 끈다.
            hit.gameObject.GetComponent<TrailRenderer>().enabled = false;
            hit.gameObject.GetComponent<BoxCollider>().enabled = false;
            hit.gameObject.GetComponent<Rigidbody>().useGravity = false;
            hit.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            // BrickSlot의 Brick 개수만큼 y Position을 더한다.
            iTween.MoveTo(hit.gameObject, iTween.Hash("x", 0, "y", 0.125f * brickStack.childCount, "z", 0, "islocal", true, "time", 0.2f, "easetype", iTween.EaseType.easeOutQuint));
            // 벽돌의 색을 자신의 색으로 바꾼다.
            hit.gameObject.GetComponent<MeshRenderer>().material.color = myColor;
            // 스택에 벽돌을 Push한다.
            bricks.Push(hit.gameObject);
        }
    }

    // 다른 AI와 충돌했을 때 호출되는 함수
    public void GetHitted()
    {
        // 피격 애니메이션 재생
        animator.SetTrigger("Hitted");
        // 스택에 있는 모든 벽돌을 꺼낸다.
        while (bricks.Count > 0)
        {
            GameObject popedBrick = bricks.Pop();
            popedBrick.transform.parent = GameObject.Find("Map").transform;
            // 꺼낸 벽돌의 색을 회색으로 바꾸고, 자연스럽게 떨어지게 하기 위해 중력을 적용시키고 Trigger 설정을 끈다.
            StartCoroutine(SetBrickColor(popedBrick));
            popedBrick.GetComponent<BoxCollider>().enabled = true;
            popedBrick.GetComponent<BoxCollider>().isTrigger = false;
            popedBrick.GetComponent<Rigidbody>().useGravity = true;
        }
    }

    // 충돌 즉시 벽돌이 먹어지는 상황을 막기 위해 0.5초 뒤에 색이 회색으로 되게
    IEnumerator SetBrickColor(GameObject popedBrick)
    {
        popedBrick.GetComponent<MeshRenderer>().material.color = Color.white;
        yield return new WaitForSeconds(0.5f);
        popedBrick.GetComponent<MeshRenderer>().material.color = Color.gray;
    }
}
