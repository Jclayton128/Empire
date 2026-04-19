using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FactionDriver : MonoBehaviour
{
    //ref
    [SerializeField] TextMeshProUGUI _factionNameTMP = null;
    [SerializeField] TextMeshProUGUI _territoryTMP = null;
    [SerializeField] TextMeshProUGUI _productionTMP = null;
    [SerializeField] TextMeshProUGUI _unrestTMP = null;

    [SerializeField] Image _factionHex = null;


    public void SetFaction(int factionIndex, int territoryCount, float productionCount, int unrestCount)
    {
        _factionNameTMP.text = $"Faction {factionIndex}";
        _territoryTMP.text = $"Territory: {territoryCount}";
        _productionTMP.text = "Production: " + string.Format("{0:F1}", productionCount);
        _unrestTMP.text = $"Unrest: {unrestCount}%";
        _factionHex.color = FactionController.Instance.GetFactionBorderColor(factionIndex);
    }

    public void ClearFaction()
    {
        _factionNameTMP.text = " ";
        _territoryTMP.text = " ";
        _productionTMP.text = " ";
        _unrestTMP.text = " ";
    }
}
