using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Logic : MonoBehaviour
{

    public Vector2 lastCheckpoint = Vector2.zero;
    public GameObject player;
    public GameObject activePlayer = null;
    public GameObject activeRoom   = null;
    public GameObject gameCamera;

    public CheckPointManagement checkPointMng;
    void Start()
    {
        RespawnLastCheckPoint();
    }

    [ContextMenu("respawn")]
    public void RespawnLastCheckPoint()
    {
        if(activePlayer!= null)
            Destroy(activePlayer);
        activePlayer = Instantiate(player, lastCheckpoint, Quaternion.identity);
        UpdateCameraFocus();
    }


    public void SetCheckPoint(Vector2 newCheckPoint)
    {
        lastCheckpoint = newCheckPoint;
    }

    [ContextMenu("Debug on")]
    public void TurnOnDebugMode()
    {
        checkPointMng.TurnOnDebugMode();
    }
    [ContextMenu("Debug off")]
    public void TurnOffDebugMode()
    {
       checkPointMng.TurnOffDebugMode();
    }
    [ContextMenu("Update Camera Focus")]
    public void UpdateCameraFocus()
    {
        followPlayer cameraScript = gameCamera.GetComponent<followPlayer>();
        GameObject groundObject = GameObject.FindGameObjectWithTag("Ground");
        Tilemap tileMap = groundObject.GetComponent<Tilemap>();
        tileMap.CompressBounds();
        Bounds levelBounds = tileMap.localBounds;

        levelBounds.SetMinMax(
            tileMap.transform.TransformPoint(tileMap.localBounds.min),
            tileMap.transform.TransformPoint(tileMap.localBounds.max)
        );

        cameraScript.player       = activePlayer;
        cameraScript.room         = activeRoom;
        cameraScript.levelBounds  = levelBounds;
        cameraScript.calculateCameraBounds();
    }


    public void ChangeRoom(GameObject nextRoom)
    {
        GameObject lastRoom = activeRoom;
        int lastRoomNum = lastRoom.GetComponent<RoomScript>().roomNumber;
        Destroy(activeRoom);
        print("Last room was " + lastRoomNum);
        activeRoom = Instantiate(nextRoom, Vector3.zero, Quaternion.identity);
        //Find the door the player entered through
        NextRoomDoor entranceDoor = activeRoom.GetComponent<RoomScript>().adjacentRoomDoors.Find(door => door.leadingRoomNum == lastRoomNum);
        //Create checkpoints next to each door and have a way to designate them as default checkpoints for that door
        // lastCheckpoint = entranceDoor.transform.position;
        RespawnLastCheckPoint();
    }

     [ContextMenu("Change to room 2")]
    public void debugChangeToRoom2()
    {
        GameObject room2 = (GameObject) AssetDatabase.LoadAssetAtPath("Assets/Rooms/Room_2.prefab", typeof(GameObject));
        ChangeRoom(room2);
    }

     [ContextMenu("Change to room 1")]
      public void debugChangeToRoom1()
    {
        GameObject room2 = (GameObject) AssetDatabase.LoadAssetAtPath("Assets/Rooms/Room_1.prefab", typeof(GameObject));
        ChangeRoom(room2);
    }

}
