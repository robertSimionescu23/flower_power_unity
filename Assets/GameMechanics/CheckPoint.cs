using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    public Logic logic;

    void Awake()
    {
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<Logic>();
    }

}
