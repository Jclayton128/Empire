using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActionDriver : MonoBehaviour
{
    //ref

    [SerializeField] TextMeshProUGUI _actionNameTMP = null;
    [SerializeField] TextMeshProUGUI _actionResourceCostTMP = null;


    public void SetName(string name)
    {
        _actionNameTMP.text = name;
    }

    public void SetCost(int resourceCost)
    {
        _actionResourceCostTMP.text = resourceCost.ToString();
    }
}
