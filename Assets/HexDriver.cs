using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HexDriver : MonoBehaviour
{
    //refs

    [SerializeField] TextMeshProUGUI _hexTypeTMP = null;
    [SerializeField] TextMeshProUGUI _hexDescTMP = null;
    [SerializeField] Image _hexIcon = null;

    public void SetHex(string hexType, string hexDesc, Sprite hexIcon)
    {
        _hexDescTMP.text = hexDesc;
        _hexTypeTMP.text = hexType; 
        _hexIcon.sprite = hexIcon;
    }
}
