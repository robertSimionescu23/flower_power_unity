using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class followPlayer : MonoBehaviour
{

    public GameObject player;
    public GameObject room;
    public Bounds     levelBounds;
    public float       cameraSpeed = 3f;
    public float       cameraOffset = 5f;
    public float       currOffset   = 0f;
    private Camera     cameraObject;
    [SerializeField]private Vector2 cameraSize;
    [SerializeField]private Bounds cameraBounds;

    void Awake()
    {
        cameraObject = gameObject.GetComponent<Camera>();
        cameraSize = new()
        {
            x = cameraObject.orthographicSize * cameraObject.aspect,
            y = cameraObject.orthographicSize
        };
        cameraOffset = cameraSize.x;
    }

    public void CalculateCameraBounds()
    {
        float newXMin = levelBounds.min.x + cameraSize.x;
        float newYMin = levelBounds.min.y + cameraSize.y;
        float newXMax = levelBounds.max.x - cameraSize.x;
        float newYMax = levelBounds.max.y - cameraSize.y;

        Vector2 cameraMinBound = new (newXMin, newYMin);
        Vector2 cameraMaxBound = new (newXMax, newYMax);

        cameraBounds = new Bounds();
        cameraBounds.SetMinMax(cameraMinBound, cameraMaxBound);
    }

    private Vector3 GetCameraPosition(Vector3 currentPos)
    {
        if(player.GetComponent<Rigidbody2D>().linearVelocityX > 0)
            currentPos.x += cameraOffset;
        if(player.GetComponent<Rigidbody2D>().linearVelocityX < 0)
            currentPos.x -= cameraOffset;

        Vector3 newPos = new()
        {
            //Default to center of bounds if camera is larger than room
            x = cameraBounds.extents.x > 0?
                Mathf.Clamp(currentPos.x, cameraBounds.min.x, cameraBounds.max.x)
                : cameraBounds.center.x,

            y = cameraBounds.extents.y > 0?
                Mathf.Clamp(currentPos.y, cameraBounds.min.y, cameraBounds.max.y)
                : cameraBounds.center.y,

            z = transform.position.z //Z is constant
        };


        return newPos;
    }


    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position,
                                         GetCameraPosition(player.transform.position),
                                         cameraSpeed * Time.deltaTime);
    }
}
