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
    // ���� �ڽ��� ���� �˸��� ���� ������Ƽ
    public Color MyColor
    {
        get { return myColor; }
    }
    [SerializeField] GameObject Stair;
    // ã�ư� �ٸ�
    Transform bridge;
    Stack<GameObject> bricks = new Stack<GameObject>();

    // ���� ����� ������ �Ǻ��ϱ� ���� �迭
    GameObject[] nearBricks;

    // Vector Parameters for Move, Rotate
    Vector3 dir;

    // State Parameters
    enum State
    {
        // ���� ����
        Idle,
        // ������ ã�ƴٴ�
        // OnTriggerEnter���� ���� ��ȯ��
        Move,
        // �ٸ��� ����
        // OnTriggerEnter���� ���� ��ȯ��
        GoToBridge,
        // �ٸ��� ����
        // OnControllerColliderHit���� ���� ��ȯ��
        Climb,
        // �ٸ����� ������
        // OnTriggerEnter���� ���� ��ȯ��
        BackToFloor,
    }
    State state;

    // ���� ����� �������� �Ÿ�
    float dist = 10000;
    // ���� ����� ������ �ε���
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
        // ���� ���� Idle
        state = State.Idle;

        yield return new WaitForSeconds(3);
        state = State.Move;
    }

    void Update()
    {
        if (Bridge_GameManager.Instance.IsEnd) return;

        // ���� nearBricks �迭�� ���� ���ٸ� nearBricks �迭�� �ʱ�ȭ�Ѵ�.
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

    // ���� ���� �������� �������� �Լ�
    void GetNearBricks()
    {
        // ���� ���� ����� �������� �Ÿ��� �ʱ�ȭ�Ѵ�.
        dist = 10000;
        nearBricks = null;
        nearBricks = GameObject.FindGameObjectsWithTag("Brick");
        // ���� ���� ������ �ƴ϶�� null ������ ����
        // ���� ���� ������ �ִ� �����̶� null ������ ����
        for (int i = 0; i < nearBricks.Length; i++)
        {
            if (Mathf.Abs(nearBricks[i].transform.position.y - transform.position.y) > 5 ||
                nearBricks[i].transform.parent == brickStack)
            {
                nearBricks[i] = null;
            }
        }
    }

    // ���� ����� ������ ã�� �� ������ ���� ȸ���ϴ� �Լ�
    // Move State���� ȣ��ȴ�.
    void Rotate()
    {
        // nearBricks �迭�� ���� ���ٸ� return
        if (nearBricks == null || nearBricks.Length == 0) return;

        // ���� ���� ����� ������ ã�� ���� �����̰� ������ 10�� �Ʒ��� ������ �ִٸ�
        if (dist >= 10000 && bricks.Count < 10)
        {
            // ���� ����� ������ ã�´�.
            for (int i = 0; i < nearBricks.Length; i++)
            {
                // nearBricks[i]�� ���� null�� �ƴϰ�(���� ���̸鼭 ������������ �ʴٸ�)
                // ���� ���� �ڽ��� ����� ���ų� ȸ���̰�
                // ���� ã�� ���� ����� �������� �����ٸ�
                if (nearBricks[i] != null 
                    && (nearBricks[i].GetComponent<MeshRenderer>().sharedMaterial.color == myColor
                    || nearBricks[i].GetComponent<MeshRenderer>().sharedMaterial.color == Color.gray)
                    && Vector3.Distance(nearBricks[i].transform.position, transform.position) < dist)
                {
                    // ���� ����� �������� �Ÿ��� �����ϰ�
                    dist = Vector3.Distance(nearBricks[i].transform.position, transform.position);
                    // ���� ����� ������ �ε����� �����Ѵ�.
                    nearest = i;
                }
            }
            // dist = ���� ����� �������� �Ÿ�
            // nearest = ���� ����� ������ �ε���
        }
        
        // ���� ���� ����� ������ �ε����� ���ߴٸ�
        if (nearBricks[nearest])
            // dir�� ���� ����� ������ ���ϴ� ���⺤��
            dir = nearBricks[nearest].transform.position - transform.position;
        dir.y = 0;
        dir.Normalize();

        //���� dir�� ���� ȸ���Ѵ�.
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotSpeed);
    }

    // �ܼ��� �ٶ󺸴� �������� �̵��ϸ� �߷��� ������ ������ �Ʒ��� �̵��ϴ� �Լ�
    // ��� State���� ȣ��ȴ�.
    void Move()
    {
        // ���� �ǰ� ��� ���̶�� �������� ���Ѵ�.
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("GetHit"))
            return;
        cc.Move(transform.forward * moveSpeed * Time.deltaTime);
        cc.Move(-transform.up * 9.81f * Time.deltaTime);
    }

    // ���� ����� �ٸ��� ã�� �� �ٸ��� �̵��ϴ� �Լ�
    // GoToBridge State ���� ȣ���
    void GoToBridge()
    {
        // bridge ������ ���� ����� �ٸ��� Transform
        bridge = FindBridge();
        // bridge�� null�� �ƴ϶��
        if (bridge)
            // dir�� ���� ����� �ٸ��� �ٶ󺸴� ���⺤��
            dir = bridge.position - transform.position;
        dir.y = 0;
        dir.Normalize();

        //���� dir�� ���� ȸ���Ѵ�.
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotSpeed);
    }

    Transform FindBridge()
    {
        // Bridge �±׸� ���� ���ӿ�����Ʈ�� ��� �����´�.
        GameObject[] bridges = GameObject.FindGameObjectsWithTag("Bridge");
        Transform target = null; // ��ȯ�� Transform ����
        float dist = 10000; // ���� ����� �ٸ����� �Ÿ�

        for (int i = 0; i < bridges.Length; i++)
        {
            // ���� bridges[i]�� ���� ���� ���� ���� ����� �ٸ����� �Ÿ����� ������
            // ���� ���� �ٸ����
            if ((transform.position.y - bridges[i].transform.position.y <= 1f) && 
                Vector3.Distance(transform.position, bridges[i].transform.position) <= dist)
            {
                // target ������ bridges[i]�� ������ ����
                // dist�� �� ����
                target = bridges[i].transform;
                dist = Vector3.Distance(transform.position, bridges[i].transform.position);
            }
        }

        // ���� ����� �ٸ��� Transform ��ȯ
        return target;
    }

    // �ٸ��� ������ �Լ�
    // Climb State���� ȣ��ȴ�.
    void Climb()
    {
        // ������ ���� �չ����� ���Ѵ�. (�ٸ��� ���ϴ� ����)
        dir = Vector3.forward;
        dir.Normalize();

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotSpeed * 4);

        // ������ ������ 0���� �Ǹ� ���ư���.
        if (bricks.Count <= 0)
            state = State.BackToFloor;
    }

    // �ٸ��� �������� �Լ�
    // BackToFloor State���� ȣ��ȴ�.
    void BackToFloor()
    {
        // ���� �޹����� ���Ѵ�.
        dir = -Vector3.forward;
        dir.Normalize();

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotSpeed * 4);
    }


    public void Win(Transform endFloor)
    {
        // �ִϸ��̼� ���
        animator.SetTrigger("End");
        // ������� ��ġ�ϸ鼭
        transform.position = endFloor.transform.position;
        // ī�޶� �Ĵٺ���.
        transform.rotation = Quaternion.LookRotation(-Vector3.forward);
    }

    private void OnTriggerEnter(Collider other)
    {
        // �浹�� ��ü�� �����̰� �ڽ��� ���� ���ٸ�
        if (other.gameObject.layer == LayerMask.NameToLayer("Brick") &&
            other.transform.parent.GetComponent<Bridge_Brick>().myColor == myColor)
        {
            other.transform.parent.GetComponent<Bridge_Brick>().Respawn();

            // ������ ��ġ�� BrickSlot���� �ű��.
            other.transform.parent = brickStack;
            other.transform.localEulerAngles = Vector3.zero;
            // BrickSlot�� Brick ������ŭ y Position�� ���Ѵ�.
            iTween.MoveTo(other.gameObject, iTween.Hash("x", 0, "y", 0.125f * brickStack.childCount, "z", 0, "islocal", true, "time", 0.2f, "easetype", iTween.EaseType.easeOutQuint));
            // ������ TrailRenderer�� Collider�� ����.
            other.GetComponent<TrailRenderer>().enabled = false;
            other.GetComponent<BoxCollider>().enabled = false;
            // ���ÿ� ������ Push�Ѵ�.
            bricks.Push(other.gameObject);

            // nearBricks�� ��ȸ�Ѵ�.
            for (int i = 0; i < nearBricks.Length; i++)
            {
                // ���� �浹�� ������ nearBricks���� ã�´�.
                if (nearBricks[i] == other.gameObject)
                {
                    // ���� �浹�� ������ nearBricks���� ����(������ �����״�)
                    nearBricks[i] = null;
                    // �ٽ� ���� ����� ������ ã�� ���� dist�� �ʱ�ȭ
                    dist = 10000;
                }
            }

            // ���� ������ 10�� �̻� �����ٸ�
            if (bricks.Count >= 10)
                // GoToBridge�� ���� ��ȯ
                state = State.GoToBridge;
        }
        // �浹�� ��ü�� �ٸ��̰� ���� ������ �Ѱ� �̻� ������ �ִٸ�
        else if (other.gameObject.layer == LayerMask.NameToLayer("Bridge") && bricks.Count > 0)
        {
            // ������ �ϳ� ������.
            Destroy(bricks.Pop());
            // ���� ������ ������� ����� �ٸ��� �����Ѵ�.
            GameObject newStair = Instantiate(Stair, other.transform.parent);
            newStair.transform.localEulerAngles = Vector3.zero;
            newStair.transform.localPosition = other.transform.localPosition;
            newStair.GetComponent<MeshRenderer>().material.color = myColor;
            other.transform.localPosition += new Vector3(0, 0.5f, 0.8f);
            // �浹�� Bridge�� ���� ���� �߰�
            other.transform.parent.GetComponent<Bridge_Bridge>().Count++;
        }
        
        // Idle�� �ƴ� �� �ٴڿ� ��Ҵٸ� Move�� ���� ��ȯ
        if (other.gameObject.layer == LayerMask.NameToLayer("Floor") && state != State.Idle)
        {
            state = State.Move;
            GetNearBricks();
        }

        // �浹�� ��ü�� �ٸ� AI Ȥ�� �÷��̾��� ���� �����̶��
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
        // �ٸ��� ���� �� Climb�� State ��ȯ
        // �ٸ��� �� �� �ѹ� ����Ǵ°� �ƴ�, �ٸ��� ��� ���� �� �׻� ����Ǿ� �Ѵ�. (�ѹ��� ����Ǹ� �ٸ��� ������ �߰��� GoToBridge�� ���°� �ٲ�)
        // �׻� ����Ǿ�� �ϱ� ������ Enter �Լ� ��� �Ұ�, ��� �ִ� ����� Trigger�� �ƴϱ� ������ OnTriggerStay�� �Ұ�
        if (state == State.GoToBridge && hit.gameObject.layer == LayerMask.NameToLayer("Bridge") && bricks.Count > 0)
            state = State.Climb;

        // ���� �浹�� ��ü�� ȸ�� �����̶�� (ȸ�� ������ Trigger�� �����ִ�.)
        if (hit.gameObject.layer == LayerMask.NameToLayer("Brick") && hit.gameObject.GetComponent<MeshRenderer>().sharedMaterial.color == Color.gray)
        {
            // ������ ��ġ�� BrickSlot���� �ű��.
            hit.transform.parent = brickStack;
            hit.transform.localEulerAngles = Vector3.zero;
            // ������ TrailRenderer�� Collider�� ����.
            hit.gameObject.GetComponent<TrailRenderer>().enabled = false;
            hit.gameObject.GetComponent<BoxCollider>().enabled = false;
            hit.gameObject.GetComponent<Rigidbody>().useGravity = false;
            hit.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            // BrickSlot�� Brick ������ŭ y Position�� ���Ѵ�.
            iTween.MoveTo(hit.gameObject, iTween.Hash("x", 0, "y", 0.125f * brickStack.childCount, "z", 0, "islocal", true, "time", 0.2f, "easetype", iTween.EaseType.easeOutQuint));
            // ������ ���� �ڽ��� ������ �ٲ۴�.
            hit.gameObject.GetComponent<MeshRenderer>().material.color = myColor;
            // ���ÿ� ������ Push�Ѵ�.
            bricks.Push(hit.gameObject);

            // nearBricks�� ��ȸ�Ѵ�.
            for (int i = 0; i < nearBricks.Length; i++)
            {
                // ���� �浹�� ������ nearBricks���� ã�´�.
                if (nearBricks[i] == hit.gameObject)
                {
                    // ���� �浹�� ������ nearBricks���� ����(������ �����״�)
                    nearBricks[i] = null;
                    // �ٽ� ���� ����� ������ ã�� ���� dist�� �ʱ�ȭ
                    dist = 10000;
                }
            }

            // ���� ������ 10�� �̻� �����ٸ�
            if (bricks.Count >= 10)
                // GoToBridge�� ���� ��ȯ
                state = State.GoToBridge;
        }
    }

    // �ٸ� �÷��̾� Ȥ�� AI�� �浹���� �� ȣ��Ǵ� �Լ�
    public void GetHitted()
    {
        // �ǰ� �ִϸ��̼� ���
        animator.SetTrigger("Hitted");
        // ���ÿ� �ִ� ��� ������ ������.
        while (bricks.Count > 0)
        {
            GameObject popedBrick = bricks.Pop();
            popedBrick.transform.parent = GameObject.Find("Map").transform;
            // ���� ������ ���� ȸ������ �ٲٰ�, �ڿ������� �������� �ϱ� ���� �߷��� �����Ű�� Trigger ������ ����.
            StartCoroutine(SetBrickColor(popedBrick));
            popedBrick.GetComponent<BoxCollider>().enabled = true;
            popedBrick.GetComponent<BoxCollider>().isTrigger = false;
            popedBrick.GetComponent<Rigidbody>().useGravity = true;
        }
    }

    // �浹 ��� ������ �Ծ����� ��Ȳ�� ���� ���� 0.5�� �ڿ� ���� ȸ������ �ǰ�
    IEnumerator SetBrickColor(GameObject popedBrick)
    {
        popedBrick.GetComponent<MeshRenderer>().material.color = Color.white;
        yield return new WaitForSeconds(0.5f);
        popedBrick.GetComponent<MeshRenderer>().material.color = Color.gray;
    }
}
