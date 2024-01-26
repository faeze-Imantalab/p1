using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    private Loading loading;
    void Start()
    {
        loading=Loading.instance;

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            loading.Set(true);
        }
        else
        {
            loading.Set(false);
        }
   

        
    }

    public override void OnConnected()
    {
        base.OnConnected();
        
    }

    public override void OnConnectedToMaster()
    {
        loading.Set(false);
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        SceneManager.LoadScene("Lobby");
    }
}
