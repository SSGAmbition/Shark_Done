using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;


public class PhotonManager : MonoBehaviourPunCallbacks
{
    private readonly string gameVersion = "v1.0";
    private string userId = "Ojui";

    [SerializeField] private GameObject _playerObj;

    public void Awake()
    {
        //방장이 혼자 씬을 로딩하면, 나머지 사람들은 자동으로 싱크가 됨
        PhotonNetwork.AutomaticallySyncScene = true;

        //게임 버전 지정
        PhotonNetwork.GameVersion = gameVersion;

        //서버 접속
        PhotonNetwork.ConnectUsingSettings();
    }
    void Start()
    {
        Debug.Log("00. 포톤 매니저 시작");
        PhotonNetwork.NickName = userId;
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("01. 포톤 서버에 접속");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("02. 랜덤 룸 접속 실패" + returnCode + " " + message);

        //룸 속성 설정
        RoomOptions ro = new RoomOptions();
        ro.IsOpen = true;
        ro.IsVisible = true;
        ro.MaxPlayers = 6;

        //룸을 생성 > 자동 입장됨
        PhotonNetwork.CreateRoom("room_1", ro);
    }
    // Update is called once per frame

    public override void OnCreatedRoom()
    {

        Debug.Log("03. 방 생성 완료");

    }

    public override void OnJoinedRoom()
    {
        Debug.Log("04. 방 입장 완료");
        GameObject player = PhotonNetwork.Instantiate("PlayerObj_test", Vector3.zero + new Vector3(0, 1, 0), Quaternion.identity);
    }
     public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("방 만들기 실패했습니다. : " + message);
        if(PhotonNetwork.IsMasterClient) 
        {
            PhotonNetwork.LoadLevel("PlayScene");
        
        }
    }

}