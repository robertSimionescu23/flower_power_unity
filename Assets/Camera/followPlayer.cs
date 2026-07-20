using System;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class followPlayer : MonoBehaviour
{
    //Needed References to gameObject properties
    private Camera                  cameraObject;
    [SerializeField]private Vector2 cameraSize;
    [SerializeField]private Bounds  cameraBounds;

    //External References used
    public Tilemap      groundTilemap;
    public GameObject   player;
    public Vector3      playerPos;
    public GameObject   room;
    public Bounds       levelBounds;

    //Camera Speed
    public float       cameraSpeed;
    public float       defaultCameraSpeed = 1.5f;

    //Dead Zone parameters
    public bool    isMoving  = false;
    public Vector3 targetPos;
    public float   distanceX;
    public float   distanceY;

    void Awake()
    {
        cameraObject = gameObject.GetComponent<Camera>();
        cameraSize = new()
        {
            x = cameraObject.orthographicSize * cameraObject.aspect,
            y = cameraObject.orthographicSize
        };
        cameraSpeed   = defaultCameraSpeed;
    }

    public void CalculateCameraBounds()
    {
        //Start camera at 0
        cameraObject.transform.position = new Vector3(0, 0, cameraObject.transform.position.z);

        //Minimums of camera position based on bounds and camera size
        //As in, place camera left corner within level
        float newXMin = levelBounds.min.x + cameraSize.x;
        float newYMin = levelBounds.min.y + cameraSize.y;
        float newXMax = levelBounds.max.x - cameraSize.x;
        float newYMax = levelBounds.max.y - cameraSize.y;

        Vector2 cameraMinBound = new (newXMin, newYMin);
        Vector2 cameraMaxBound = new (newXMax, newYMax);

        cameraBounds = new Bounds();
        cameraBounds.SetMinMax(cameraMinBound, cameraMaxBound);
    }

    //Make sure camera does not show parts that are not in the level, outer bounds
    private Vector3 GetCameraPosition(Vector3 currentPos)
    {
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


    // Dead zone enforcement
    public void SetIsMoving(float distanceX, float distanceY)
    {
        if (isMoving)
        {
            if(
                distanceX < 0.2f &&
                distanceY < 0.2f
            ){
                isMoving = false;
            }
        }
        else
        {
            if(
                distanceX > 4f ||
                distanceY > 2f
            ){
                Debug.Log("IsMoving");
                isMoving = true;
            }
        }
    }

    //If the player is moving too fast for the camera, update it's speed
    void UpdateCameraSpeedToPlayerSpeed()
    {
        float playerSpeed = Math.Abs(player.GetComponent<Rigidbody2D>().linearVelocityY);
        if(playerSpeed > 8f)
        {
            if(cameraSpeed < playerSpeed)
                cameraSpeed += defaultCameraSpeed * 10 * Time.deltaTime; //TODO: 10 is arbitrary and can be changed for a variable if needed
            else
                cameraSpeed = playerSpeed;
        }
        else
        {
            cameraSpeed = defaultCameraSpeed;
        }
    }
    void LateUpdate()
    {
        playerPos = player.transform.position;
        targetPos = GetCameraPosition(playerPos);

        //Distances from player to camera
        distanceX = Math.Abs(targetPos.x - transform.position.x);
        distanceY = Math.Abs(targetPos.y - transform.position.y);

        SetIsMoving(distanceX, distanceY);

        if(isMoving)
            UpdateCameraSpeedToPlayerSpeed();
            transform.position = Vector3.Lerp(transform.position,
                                              targetPos,
                                              cameraSpeed * Time.deltaTime);
    }
}
