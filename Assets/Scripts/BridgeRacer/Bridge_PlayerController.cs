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
    // ���� �ڽ��� ���� �˸��� ���� ������Ƽ
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
        // ��ġ �Է��� �ִٸ� dir�� ���� �����Ѵ�.
        if (Input.touchCount > 0)
            dir = SimpleInput.GetAxis("Horizontal") * Vector3.right + SimpleInput.GetAxis("Vertical") * Vector3.forward;
        // ��ġ �Է��� ���ٸ� dir�� 0���� �ʱ�ȭ�Ѵ�.
        else
            dir = Vector3.zero;

        // ���� dir�� 1���� ũ�ٸ� dir�� ũ�⸦ 1�� �ٲ۴�. (dir�� ���⸸ ��Ÿ���� ���⺤�ͱ� ����)
        if (dir.magnitude > 1)
            dir.Normalize();
        // dir�� ũ�Ⱑ 0�� �ƴ϶��(��ġ �Է��� �ִٸ�) �÷��̾ ȸ����Ų��.
        else if (dir.magnitude > 0.2f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), rotSpeed * 2 * Time.deltaTime);
    }

    void Move()
    {
        // ���� �ǰ� ��� ���̶�� �������� ���Ѵ�.
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("GetHit"))
            return;

        // ��ġ �Է��� �ִٸ� State�� Move�� �ٲٰ� �÷��̾ �ٶ󺸰� �ִ� �������� �̵��Ѵ�.
        if (dir.magnitude > 0.2f && Input.touchCount > 0)
        {
            cc.Move(transform.forward * moveSpeed * Time.deltaTime);
            state = State.Move;
        }
        // ��ġ �Է��� ���ٸ� State�� Idle�� �ٲٰ� �̵����� �ʴ´�.
        else
            state = State.Idle;

        // �߷��� ������ ������ �׻� �Ʒ��� �̵��Ѵ�.
        cc.Move(-transform.up * 9.81f * Time.deltaTime);
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
            // ���� �߰�
            Bridge_GameManager.Instance.Score += 100;
            // �浹�� Bridge�� ���� ���� �߰�
            other.transform.parent.GetComponent<Bridge_Bridge>().Count++;
        }

        // �浹�� ��ü�� �ٸ� AI�� ���� �����̶��
        if (other.gameObject.layer == LayerMask.NameToLayer("BrickStack") && other.transform.parent != transform)
        {
            other.GetComponentInParent<Bridge_AIController>().GetHitted();
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
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
        }
    }

    // �ٸ� AI�� �浹���� �� ȣ��Ǵ� �Լ�
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
