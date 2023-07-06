using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge_AIController : MonoBehaviour
{
    // AI Parameters
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
    // 찾아갈 다리
    Transform bridge;
    Stack<GameObject> bricks = new Stack<GameObject>();

    // 가장 가까운 벽돌을 판별하기 위한 배열
    GameObject[] nearBricks;

    // Vector Parameters for Move, Rotate
    Vector3 dir;

    // State Parameters
    enum State
    {
        // 시작 상태
        Idle,
        // 벽돌을 찾아다님
        // OnTriggerEnter에서 상태 변환됨
        Move,
        // 다리를 향함
        // OnTriggerEnter에서 상태 변환됨
        GoToBridge,
        // 다리를 오름
        // OnControllerColliderHit에서 상태 변환됨
        Climb,
        // 다리에서 내려옴
        // OnTriggerEnter에서 상태 변환됨
        BackToFloor,
    }
    State state;

    // 가장 가까운 벽돌과의 거리
    float dist = 10000;
    // 가장 가까운 벽돌의 인덱스
    int nearest = 0;

    IEnumerator Start()
    {
        var entity = D_Params.GetEntity("BridgeParams");
        moveSpeed = entity.f_MoveSpeed;
        rotSpeed = entity.f_RotateSpeed;

        cc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        brickStack = transform.Find("BrickStack");

        dir = Vector3.forward;
        // 시작 상태 Idle
        state = State.Idle;

        yield return new WaitForSeconds(3);
        state = State.Move;
    }

    void Update()
    {
        if (Bridge_GameManager.Instance.IsEnd) return;

        // 만약 nearBricks 배열에 값이 없다면 nearBricks 배열을 초기화한다.
        if (nearBricks == null || nearBricks.Length == 0)
        {
            GetNearBricks();
        }

        switch (state)
        {
            case State.Idle:
                animator.SetInteger("State", 0);
                break;
            case State.Move:
                Rotate();
                Move();
                animator.SetInteger("State", 1);
                break;
            case State.GoToBridge:
                GoToBridge();
                Move();
                animator.SetInteger("State", 1);
                break;
            case State.Climb:
                Climb();
                Move();
                animator.SetInteger("State", 1);
                break;
            case State.BackToFloor:
                BackToFloor();
                Move();
                animator.SetInteger("State", 1);
                break;
        }
    }

    // 같은 층의 벽돌들을 가져오는 함수
    void GetNearBricks()
    {
        // 먼저 가장 가까운 벽돌과의 거리를 초기화한다.
        dist = 10000;
        nearBricks = null;
        nearBricks = GameObject.FindGameObjectsWithTag("Brick");
        // 같은 층의 벽돌이 아니라면 null 값으로 변경
        // 현재 내가 가지고 있는 벽돌이라도 null 값으로 변경
        for (int i = 0; i < nearBricks.Length; i++)
        {
            if (Mathf.Abs(nearBricks[i].transform.position.y - transform.position.y) > 5 ||
                nearBricks[i].transform.parent == brickStack)
            {
                nearBricks[i] = null;
            }
        }
    }

    // 가장 가까운 벽돌을 찾고 그 벽돌을 향해 회전하는 함수
    // Move State에서 호출된다.
    void Rotate()
    {
        // nearBricks 배열의 값이 없다면 return
        if (nearBricks == null || nearBricks.Length == 0) return;

        // 만약 가장 가까운 벽돌을 찾지 않은 상태이고 벽돌을 10개 아래로 가지고 있다면
        if (dist >= 10000 && bricks.Count < 10)
        {
            // 가장 가까운 벽돌을 찾는다.
            for (int i = 0; i < nearBricks.Length; i++)
            {
                // nearBricks[i]의 값이 null이 아니고(같은 층이면서 리스폰중이지 않다면)
                // 색상 또한 자신의 색상과 같거나 회색이고
                // 현재 찾은 가장 가까운 벽돌보다 가깝다면
                if (nearBricks[i] != null 
                    && (nearBricks[i].GetComponent<MeshRenderer>().sharedMaterial.color == myColor
                    || nearBricks[i].GetComponent<MeshRenderer>().sharedMaterial.color == Color.gray)
                    && Vector3.Distance(nearBricks[i].transform.position, transform.position) < dist)
                {
                    // 가장 가까운 벽돌과의 거리를 수정하고
                    dist = Vector3.Distance(nearBricks[i].transform.position, transform.position);
                    // 가장 가까운 벽돌의 인덱스를 수정한다.
                    nearest = i;
                }
            }
            // dist = 가장 가까운 벽돌과의 거리
            // nearest = 가장 가까운 벽돌의 인덱스
        }
        
        // 만약 가장 가까운 벽돌의 인덱스를 구했다면
        if (nearBricks[nearest])
            // dir은 가장 가까운 벽돌을 향하는 방향벡터
            dir = nearBricks[nearest].transform.position - transform.position;
        dir.y = 0;
        dir.Normalize();

        //구한 dir을 향해 회전한다.
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotSpeed);
    }

    // 단순히 바라보는 방향으로 이동하며 중력의 영향을 받으며 아래로 이동하는 함수
    // 모든 State에서 호출된다.
    void Move()
    {
        // 만약 피격 모션 중이라면 움직이지 못한다.
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("GetHit"))
            return;
        cc.Move(transform.forward * moveSpeed * Time.deltaTime);
        cc.Move(-transform.up * 9.81f * Time.deltaTime);
    }

    // 가장 가까운 다리를 찾아 그 다리로 이동하는 함수
    // GoToBridge State 에서 호출됨
    void GoToBridge()
    {
        // bridge 변수는 가장 가까운 다리의 Transform
        bridge = FindBridge();
        // bridge가 null이 아니라면
        if (bridge)
            // dir은 가장 가까운 다리를 바라보는 방향벡터
            dir = bridge.position - transform.position;
        dir.y = 0;
        dir.Normalize();

        //구한 dir을 향해 회전한다.
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotSpeed);
    }

    Transform FindBridge()
    {
        // Bridge 태그를 갖는 게임오브젝트를 모두 가져온다.
        GameObject[] bridges = GameObject.FindGameObjectsWithTag("Bridge");
        Transform target = null; // 반환할 Transform 변수
        float dist = 10000; // 가장 가까운 다리와의 거리

        for (int i = 0; i < bridges.Length; i++)
        {
            // 만약 bridges[i]의 값이 현재 구한 가장 가까운 다리와의 거리보다 가깝고
            // 같은 층의 다리라면
            if ((transform.position.y - bridges[i].transform.position.y <= 1f) && 
                Vector3.Distance(transform.position, bridges[i].transform.position) <= dist)
            {
                // target 변수를 bridges[i]의 값으로 대입
                // dist의 값 수정
                target = bridges[i].transform;
                dist = Vector3.Distance(transform.position, bridges[i].transform.position);
            }
        }

        // 가장 가까운 다리의 Transform 반환
        return target;
    }

    // 다리를 오르는 함수
    // Climb State에서 호출된다.
    void Climb()
    {
        // 무조건 맵의 앞방향을 향한다. (다리가 향하는 방향)
        dir = Vector3.forward;
        dir.Normalize();

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotSpeed * 4);

        // 벽돌의 개수가 0개가 되면 돌아간다.
        if (bricks.Count <= 0)
            state = State.BackToFloor;
    }

    // 다리를 내려가는 함수
    // BackToFloor State에서 호출된다.
    void BackToFloor()
    {
        // 맵의 뒷방향을 향한다.
        dir = -Vector3.forward;
        dir.Normalize();

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotSpeed * 4);
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

            // nearBricks를 순회한다.
            for (int i = 0; i < nearBricks.Length; i++)
            {
                // 지금 충돌한 벽돌을 nearBricks에서 찾는다.
                if (nearBricks[i] == other.gameObject)
                {
                    // 지금 충돌한 벽돌을 nearBricks에서 제거(리스폰 중일테니)
                    nearBricks[i] = null;
                    // 다시 가장 가까운 벽돌을 찾기 위해 dist값 초기화
                    dist = 10000;
                }
            }

            // 만약 벽돌을 10개 이상 가졌다면
            if (bricks.Count >= 10)
                // GoToBridge로 상태 변환
                state = State.GoToBridge;
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
            // 충돌한 Bridge의 벽돌 개수 추가
            other.transform.parent.GetComponent<Bridge_Bridge>().Count++;
        }
        
        // Idle이 아닐 때 바닥에 닿았다면 Move로 상태 변환
        if (other.gameObject.layer == LayerMask.NameToLayer("Floor") && state != State.Idle)
        {
            state = State.Move;
            GetNearBricks();
        }

        // 충돌한 물체가 다른 AI 혹은 플레이어의 벽돌 스택이라면
        if (other.gameObject.layer == LayerMask.NameToLayer("BrickStack") && other.transform.parent != transform)
        {
            if (other.transform.parent.gameObject.layer == LayerMask.NameToLayer("AI"))
                other.GetComponentInParent<Bridge_AIController>().GetHitted();
            else
                other.GetComponentInParent<Bridge_PlayerController>().GetHitted();
        }
    }

    
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // 다리에 있을 때 Climb로 State 변환
        // 다리에 들어갈 때 한번 실행되는게 아닌, 다리를 밟고 있을 때 항상 실행되야 한다. (한번만 실행되면 다리를 오르는 중간에 GoToBridge로 상태가 바뀜)
        // 항상 실행되어야 하기 때문에 Enter 함수 사용 불가, 밟고 있는 계단은 Trigger가 아니기 때문에 OnTriggerStay도 불가
        if (state == State.GoToBridge && hit.gameObject.layer == LayerMask.NameToLayer("Bridge") && bricks.Count > 0)
            state = State.Climb;

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

            // nearBricks를 순회한다.
            for (int i = 0; i < nearBricks.Length; i++)
            {
                // 지금 충돌한 벽돌을 nearBricks에서 찾는다.
                if (nearBricks[i] == hit.gameObject)
                {
                    // 지금 충돌한 벽돌을 nearBricks에서 제거(리스폰 중일테니)
                    nearBricks[i] = null;
                    // 다시 가장 가까운 벽돌을 찾기 위해 dist값 초기화
                    dist = 10000;
                }
            }

            // 만약 벽돌을 10개 이상 가졌다면
            if (bricks.Count >= 10)
                // GoToBridge로 상태 변환
                state = State.GoToBridge;
        }
    }

    // 다른 플레이어 혹은 AI와 충돌했을 때 호출되는 함수
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
