using UnityEngine;

public class followPlayer : MonoBehaviour
{
    public float followSpeed = 5f;
    public float yOffset     = 1f;
    public Transform target;



    // Update is called once per frame
    void Update()
    {
        Vector3 newPos = new Vector3(target.position.x, target.position.y + yOffset, -10f);
        transform.position = Vector3.Slerp(transform.position, newPos, followSpeed * Time.deltaTime);
    }
}
