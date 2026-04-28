using Unity.Mathematics;
using UnityEngine;

public class Logic : MonoBehaviour
{

    public Vector2 lastCheckpoint = Vector3.zero;
    public GameObject player;
    public Object activePlayer = null;

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
}
