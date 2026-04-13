using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class HexDriver : MonoBehaviour
{
    //refs

    [SerializeField] TextMeshProUGUI _hexTypeTMP = null;
    [SerializeField] TextMeshProUGUI _hexDescTMP = null;
    [SerializeField] Image _hexIcon = null;

    public void SetHex(TileType tileType)
    {
        _hexTypeTMP.text = tileType.TType.ToString();
        _hexDescTMP.text = tileType.TypeDescription;

        if (tileType.TileIcon == null)
        {
            _hexIcon.enabled = false;
        }
        else
        {
            _hexIcon.enabled = true;
            _hexIcon.sprite = tileType.TileIcon;
        }

    }

    public void ClearHex()
    {
        _hexTypeTMP.text = " ";
        _hexDescTMP.text = " ";
        _hexIcon.enabled = false;
    }

}
