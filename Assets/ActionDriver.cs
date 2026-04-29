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
    [SerializeField] TextMeshProUGUI _actionPopulationCostTMP = null;


    public void SetName(string name)
    {
        _actionNameTMP.text = name;
    }

    public void SetCost(int resourceCost, int populationCost)
    {
        _actionResourceCostTMP.text = resourceCost.ToString();
        _actionPopulationCostTMP.text = populationCost.ToString();
    }
}
