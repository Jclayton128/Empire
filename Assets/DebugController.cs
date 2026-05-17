using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugController : MonoBehaviour
{
    void Update()
    {
        SelectPlayerFaction();
        SelectTimeScale();
        if (Input.GetKeyDown(KeyCode.N))
        {
            GameController.Instance.StartNewGame();
        }

    }

    private void SelectTimeScale()
    {
        if (Input.GetKeyDown(KeyCode.Comma))
        {
            GameController.Instance.DecreaseTimeScale();
        }
        if (Input.GetKeyDown(KeyCode.Period))
        {
            GameController.Instance.IncreaseTimeScale();
        }
    }

    private void SelectPlayerFaction()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            FactionController.Instance.SetPlayerFaction(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            FactionController.Instance.SetPlayerFaction(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            FactionController.Instance.SetPlayerFaction(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            FactionController.Instance.SetPlayerFaction(4);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            FactionController.Instance.SetPlayerFaction(5);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            FactionController.Instance.SetPlayerFaction(6);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            FactionController.Instance.SetPlayerFaction(0);
        }
    }
}
