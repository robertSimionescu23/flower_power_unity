using System;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class followPlayer : MonoBehaviour
{

    public GameObject  player;
    public GameObject  room;
    public Bounds      levelBounds;
    public float       cameraSpeed = 1.5f;
    public float       cameraOffsetX = 4f;
    public float       cameraOffsetY = 2f;
    public float       currOffset   = 0f;
    private Camera     cameraObject;
    [SerializeField]private Vector2 cameraSize;
    [SerializeField]private Bounds cameraBounds;
    public Tilemap groundTilemap;

    public bool isMoving = false;
    public Vector3 targetPos;
    public float distanceX;
    public float distanceY;
    public Vector3 playerPos;

    //TODO: Implement system to keep camera centered on mean of position ion last x milseconds

    public float medianTimer = 0.3f;
    public float currMedianTimer;
    public Vector3 medianPosition;
    public int positionsUsed;
    private Vector3 tempMedian;

    void Awake()
    {
        cameraObject = gameObject.GetComponent<Camera>();
        cameraSize = new()
        {
            x = cameraObject.orthographicSize * cameraObject.aspect,
            y = cameraObject.orthographicSize
        };
        cameraOffsetX = cameraSize.x;

        currMedianTimer = medianTimer;
        medianPosition  = playerPos;
        positionsUsed   = 0;
    }

    public void CalculateCameraBounds()
    {
        //Start camera at 0
        cameraObject.transform.position = new Vector3(0, 0, cameraObject.transform.position.z);
        float newXMin = levelBounds.min.x + cameraSize.x;
        float newYMin = levelBounds.min.y + cameraSize.y;
        float newXMax = levelBounds.max.x - cameraSize.x;
        float newYMax = levelBounds.max.y - cameraSize.y;

        Vector2 cameraMinBound = new (newXMin, newYMin);
        Vector2 cameraMaxBound = new (newXMax, newYMax);

        cameraBounds = new Bounds();
        cameraBounds.SetMinMax(cameraMinBound, cameraMaxBound);
    }

    public void SetMedianPosition()
    {
        if(currMedianTimer > 0){
            currMedianTimer -= Time.deltaTime;

            tempMedian.x += playerPos.x;
            tempMedian.y += playerPos.y;
            tempMedian.z  = playerPos.z;

            positionsUsed += 1;
        }
        else
        {
            medianPosition.x = (tempMedian.x / positionsUsed) + currOffset;
            medianPosition.y = tempMedian.y / positionsUsed;
            medianPosition.z = tempMedian.z;

            //Resets
            currMedianTimer = medianTimer;
            tempMedian      = Vector3.zero;
            positionsUsed   = 0;
        }

    }

    private Vector3 GetCameraPosition(Vector3 currentPos)
    {
        bool isWallsliding = player.GetComponent<PlayerCharacter>().isWallSliding;
        bool isWallJumping = player.GetComponent<PlayerCharacter>().isWallJumping;

        Vector3 startPos = currentPos;
        if(isMoving){
            // else
            //     currentPos.x = startPos.x;

            // if(player.GetComponent<Rigidbody2D>().linearVelocityY > 1f)
            //     currentPos.y += cameraOffsetY;
            // else if(player.GetComponent<Rigidbody2D>().linearVelocityY < 1f && isWallsliding && !isWallsliding)
            //     currentPos.y -= cameraOffsetY;
            // else if(player.GetComponent<Rigidbody2D>().linearVelocityY == 0)
            //     currentPos.y = player.transform.position.y;

            // if(isWallJumping || isWallsliding)
            //     currentPos.y += cameraOffsetY;
        }

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

    public void SetIsMoving(Vector3 currentPos)
    {
        //TODO: Make values 2f etc variables for ease of config
        if (isMoving)
        {
            if(
                Math.Abs(targetPos.x - currentPos.x) < 0.1f &&
                Math.Abs(targetPos.y - currentPos.y) < 0.1f
            ){
                isMoving = false;
            }
        }
        else
        {
            if(
                Math.Abs(targetPos.x - currentPos.x) > 2.5f ||
                Math.Abs(targetPos.y - currentPos.y) > 1f
            ){
                isMoving = true;
                if(player.GetComponent<Rigidbody2D>().linearVelocityX > 0)
                    currentPos.x += cameraOffsetX;
                else if(player.GetComponent<Rigidbody2D>().linearVelocityX < 0)
                    currentPos.x -= cameraOffsetX;
            }
        }
    }

    //TODO: Implement a system that only allows camera movement if the distance between last movement and current position is bigger tha a set value


    // Update is called once per frame
    void LateUpdate()
    {
        playerPos = player.transform.position;

        SetMedianPosition();
        if(Math.Abs(player.GetComponent<Rigidbody2D>().linearVelocityY) > 2)
            targetPos = GetCameraPosition(new Vector3(medianPosition.x, playerPos.y, playerPos.z));
        else
            targetPos = GetCameraPosition(medianPosition);

        SetIsMoving(targetPos);

        distanceX = Math.Abs(targetPos.x - transform.position.x);
        distanceY = Math.Abs(targetPos.y - transform.position.y);
        if(isMoving)
            transform.position = Vector3.Lerp(transform.position,
                                              targetPos,
                                              cameraSpeed * Time.deltaTime);
    }
}
