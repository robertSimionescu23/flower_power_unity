using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class followPlayer : MonoBehaviour
{

    public GameObject player;
    public GameObject room;
    public Bounds levelBounds;
    private Camera cameraObject;
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

    }

    public void calculateCameraBounds()
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

    private Vector3 getCameraPosition(Vector3 currentPos)
    {
        Vector3 newPos = new()
        {
            //Default to center of bounds if camera is larger than room
            x = cameraBounds.extents.x > 0? Mathf.Clamp(currentPos.x, cameraBounds.min.x, cameraBounds.max.x) : cameraBounds.center.x,
            y = cameraBounds.extents.y > 0? Mathf.Clamp(currentPos.y, cameraBounds.min.y, cameraBounds.max.y) : cameraBounds.center.y,
            z = transform.position.z //Z is constant
        };
        return newPos;
    }


    // Update is called once per frame
    void Update()
    {
        transform.position = getCameraPosition(player.transform.position);
    }
}
