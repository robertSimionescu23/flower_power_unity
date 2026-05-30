using UnityEditor;
using UnityEngine;

public class NextRoomDoor : MonoBehaviour
{
    public int leadingRoomNum = 0;
    public GameObject nextRoom;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        nextRoom    = (GameObject) AssetDatabase.LoadAssetAtPath($"Assets/Rooms/Room_{leadingRoomNum}.prefab", typeof(GameObject));
    }

    // Update is called once per frame
    void Update()
    {

    }
}
