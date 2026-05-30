using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomScript : MonoBehaviour
{
    public int roomNumber = 0;
    public List<NextRoomDoor> adjacentRoomDoors = new List<NextRoomDoor>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        adjacentRoomDoors = new List<NextRoomDoor>(transform.GetComponentsInChildren<NextRoomDoor>());
    }

    // Update is called once per frame
    void Update()
    {

    }
}
