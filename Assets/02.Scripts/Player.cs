using System.Collections;
using Photon.Pun;
using UnityEngine;

public class Player : MonoBehaviourPunCallbacks
{
    public PhotonView pv;

    [Header("Stat")] 
    public float MoveSpeed;
    [SerializeField] private GameObject _itemView;
    [SerializeField] private bool isMove;

    public GameObject AttackCol;

    public bool isGettingTrash;

    [field: SerializeField] public int Hp { get; private set; }

    private Animator _animator;

    public void Start()
    {
        _animator = GetComponent<Animator>();
        pv = GetComponent<PhotonView>();

        isMove = true;
    }

    private void Update()
    {
        if (pv.IsMine && isMove)
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            Vector3 moveVec = ((transform.forward * v) + (transform.right * h)) * Time.deltaTime * MoveSpeed;

            transform.position += moveVec;
            _animator.SetBool("Move", moveVec != Vector3.zero);
            if (Input.GetMouseButtonDown(0))
            {
                _animator.SetBool("Attack", true);
            }
            
            Vector3 pos = Camera.main.WorldToViewportPoint(transform.position);
            if(pos.x < 0) pos.x = 0;
            if (pos.x > 1) pos.x = 1;
            if (pos.y < 0) pos.y = 0;
            if (pos.y > 1) pos.y = 1;

            transform.position = Camera.main.ViewportToWorldPoint(pos);
            
            RookMouse(Input.mousePosition);
            
            //pv.RPC("RookMouse", RpcTarget.All, Input.mousePosition);
        }
    }

    public void StartAttack()
    {
        pv.RPC("AttackOn", RpcTarget.All, null);
    }

    [PunRPC]
    public void AttackOn()
    {
        _animator.SetBool("Attack", false);
        AttackCol.gameObject.SetActive(true);
    }

    [PunRPC]
    public void AttackOff()
    {
        AttackCol.gameObject.SetActive(false);
    }

    public void EndAttack()
    {
        pv.RPC("AttackOff", RpcTarget.All, null); 
    }
    
    void RookMouse(Vector3 pos)
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(pos);

        Plane GroupPlane = new Plane(Vector3.up, Vector3.zero);

        float rayLength;

        if(GroupPlane.Raycast(cameraRay, out rayLength))

        {
            Vector3 pointTolook = cameraRay.GetPoint(rayLength);

            transform.LookAt(new Vector3(pointTolook.x, transform.position.y, pointTolook.z));

        }
    }

    [PunRPC]
    void GetTrash()
    {
        isGettingTrash = true;
        pv.RPC("IsGetTrash", RpcTarget.All, isGettingTrash);
        //GameObject trash = PhotonNetwork.Instantiate("TrashObj_player", _itemView.transform.position, Quaternion.identity);
    }
    
    
    [PunRPC]
    void IsGetTrash(bool isGetting)
    {
        _itemView.gameObject.SetActive(isGetting);
    }

    [PunRPC]
    void ThrowTrash()
    {
        _itemView.gameObject.SetActive(false);
        Debug.Log("작동");
        GameObject trashObj = PhotonNetwork.Instantiate("TrashObj", transform.position + new Vector3(1.5f, 0, 0), Quaternion.identity);
    }
    

    [PunRPC]
    void Damaged()
    {
        if (pv.IsMine)
        {
            Debug.Log("공격 받음");
            if (!_animator.GetBool("hit"))
            {
                isMove = false;
                _animator.SetBool("hit", true);
                if (Hp > 0)
                {
                    Hp--;
                
                    StartCoroutine(Coroutine_Stun());
                }
                if (isGettingTrash)
                {
                    isGettingTrash = false;
                
                    pv.RPC("ThrowTrash", RpcTarget.All, null);
                }
            }
        }
    }

    [PunRPC]
    void StopStunning()
    {
        if (pv.IsMine)
        {
            Debug.Log("스턴 멈춤");
            _animator.SetBool("hit", false);
            isMove = true;
        }
    }

    private readonly WaitForSeconds _3secWait = new(3f);
    
    IEnumerator Coroutine_Stun()
    {
        yield return _3secWait;
        pv.RPC("StopStunning", RpcTarget.All, null);
    }

    public void StunStop()
    {
        pv.RPC("StopStunning", RpcTarget.All, null);
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (isGettingTrash)
        {
            pv.RPC("IsGetTrash", newPlayer, isGettingTrash);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Trash"))
        {
            PhotonView otherPV = other.GetComponent<PhotonView>();  
            if (otherPV.IsMine)
            {
                PhotonNetwork.Destroy(other.gameObject);
            }   
            pv.RPC("GetTrash", RpcTarget.All);
        }
    }
}
