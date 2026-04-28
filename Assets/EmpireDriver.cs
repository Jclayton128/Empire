using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EmpireDriver : MonoBehaviour
{
    //ref

    [SerializeField] TextMeshProUGUI _playerEmpireNameTMP = null;
    [SerializeField] TextMeshProUGUI _playerProductionBankedTMP = null;

    public void SetPlayerEmpireName(string playerEmpireName)
    {
        _playerEmpireNameTMP.text = playerEmpireName;
    }

    public void ShowProduction(int amount)
    {
        _playerProductionBankedTMP.text = $"${amount}";    
    }
    
}

