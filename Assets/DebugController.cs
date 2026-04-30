using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugController : MonoBehaviour
{
    void Update()
    {
        SelectCurrentTool();
        SelectPlayerFaction();

        if (Input.GetKeyDown(KeyCode.N))
        {
            GameController.Instance.StartNewGame();
        }

    }

    private void SelectCurrentTool()
    {
        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    ActionController.Instance.SelectTool(ActionController.ActionTypes.Attack);
        //}
        //if (Input.GetKeyDown(KeyCode.D))
        //{
        //    ActionController.Instance.SelectTool(ActionController.ActionTypes.Defend);
        //}
        //if (Input.GetKeyDown(KeyCode.S))
        //{
        //    ActionController.Instance.SelectTool(ActionController.ActionTypes.Research);
        //}

        //if (Input.mouseScrollDelta.y > Mathf.Epsilon)
        //{
        //    ActionController.Instance.IncrementToolSelection();
        //}
        //else if (Input.mouseScrollDelta.y < -Mathf.Epsilon)
        //{
        //    ActionController.Instance.DecrementToolSelection();
        //}
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
