using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class AttackCol : MonoBehaviour
{
    [SerializeField] private List<GameObject> _damageObjs;

    private void Awake()
    {
        _damageObjs = new List<GameObject>();
    }

    private void OnEnable()
    {
        _damageObjs.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && transform.parent.gameObject != other.gameObject && !_damageObjs.Contains(other.gameObject))
        {
            PhotonView otherView = other.GetComponent<PhotonView>();
            if (otherView.IsMine)
            {
                Debug.Log(other.name + "공격성공");
                otherView.RPC("Damaged", RpcTarget.All);
                _damageObjs.Add(other.gameObject);
            }
        }
    }
}
