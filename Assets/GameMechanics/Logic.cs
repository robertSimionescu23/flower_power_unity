using Unity.Mathematics;
using UnityEngine;

public class Logic : MonoBehaviour
{

    public Vector2 lastCheckpoint = Vector2.zero;
    public GameObject player;
    public GameObject activePlayer = null;

    public CheckPointManagement checkPointMng;
    void Awake()
    {
        RespawnLastCheckPoint();
    }

    [ContextMenu("respawn")]
    public void RespawnLastCheckPoint()
    {
        if(activePlayer!= null)
            Destroy(activePlayer);
        activePlayer = Instantiate(player, lastCheckpoint, Quaternion.identity);
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
}
