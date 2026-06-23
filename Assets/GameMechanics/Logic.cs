using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Logic : MonoBehaviour
{
    //TODO: Implement Door system debug on off
    //TODO: Implement Checkpoint system debug on off
    public Vector2 currCheckpoint = Vector2.zero;
    public GameObject player;
    public GameObject activePlayer = null;
    public GameObject activeRoom   = null;
    public GameObject gameCamera;
    public CheckPointManagement checkPointMng;
    void Start()
    {
        RespawnCurrCheckpoint();
    }

    [ContextMenu("respawn")]
    public void RespawnCurrCheckpoint()
    {
        if(activePlayer!= null)
            Destroy(activePlayer);
        activePlayer = Instantiate(player, currCheckpoint, Quaternion.identity);
        UpdateCameraFocus();
    }


    public void SetCheckPoint(Vector2 newCheckPoint)
    {
        currCheckpoint = newCheckPoint;
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
        cameraScript.CalculateCameraBounds();
    }

    public NextRoomDoor getRightDestinationDoor(NextRoomDoor sourceDoor)
    {
        foreach(NextRoomDoor destDoor in activeRoom.GetComponent<RoomScript>().adjacentRoomDoors)
        {
            //You enter through source door. It points to the now active room
            //If a door in the active room matches the one you came from, return it
            if(destDoor.hallwayID == sourceDoor.hallwayID)
            {
                return destDoor;
            }
        }
        Debug.Log("There was no matching room");
        return null;
    }


    public void ChangeRoom(GameObject nextRoom, NextRoomDoor sourceDoor)
    {
        Destroy(activeRoom);
        activeRoom = Instantiate(nextRoom, Vector3.zero, Quaternion.identity);
        NextRoomDoor destDoor = getRightDestinationDoor(sourceDoor);
        if(destDoor == null)
            Debug.LogError("Destination door not found");
        else
        {
            currCheckpoint = destDoor.checkPoint.transform.position;
            RespawnCurrCheckpoint();
        }
    }

    //  [ContextMenu("Change to room 2")]
    // public void debugChangeToRoom2()
    // {
    //     GameObject room2 = (GameObject) AssetDatabase.LoadAssetAtPath("Assets/Rooms/Room_2.prefab", typeof(GameObject));
    //     ChangeRoom(room2);
    // }

    //  [ContextMenu("Change to room 1")]
    //   public void debugChangeToRoom1()
    // {
    //     GameObject room2 = (GameObject) AssetDatabase.LoadAssetAtPath("Assets/Rooms/Room_1.prefab", typeof(GameObject));
    //     ChangeRoom(room2);
    // }

}
