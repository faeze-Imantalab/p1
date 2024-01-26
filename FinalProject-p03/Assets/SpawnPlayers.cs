using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class SpawnPlayers : MonoBehaviour
{
    public GameObject playerPrefab;

    public float minX, maxX, minY, maxY,minZ, maxZ;
    
    // Start is called before the first frame update
    void Start()
    {
        Vector3 randomPosition = new Vector3(Random.Range(minX , maxX) , Random.Range(minY , maxY) , Random.Range(minZ , maxZ));
        PhotonNetwork.Instantiate(playerPrefab.name, randomPosition, Quaternion.identity);
        
    }// اونی که هاست میشه طرق مقابل رو نمی بینه !
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
