using System.Collections.Generic;
using UnityEngine;

public class CheckPointManagement : MonoBehaviour
{
    private CheckPoint[] CheckPoints;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        getCheckPoints();
    }

    private void getCheckPoints()
    {
        CheckPoints = GetComponentsInChildren<CheckPoint>();
    }

    private void DeleteCheckPoints()
    {
        foreach (CheckPoint curr in CheckPoints)
        {
            Destroy(curr);
        }
        CheckPoints = null;
    }

    public void TurnOnDebugMode()
    {
         foreach (CheckPoint curr in CheckPoints)
        {
            curr.TurnOnDebugMode();
        }
    }

    public void TurnOffDebugMode()
    {
         foreach (CheckPoint curr in CheckPoints)
        {
            curr.TurnOffDebugMode();
        }
    }
}
