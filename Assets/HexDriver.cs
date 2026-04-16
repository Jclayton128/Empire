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
    [SerializeField] TextMeshProUGUI _hexIndexTMP = null;
    [SerializeField] Image _hexIcon = null;

    public void SetHex(TileHandler tile)
    {
        _hexTypeTMP.text = tile.CurrentTileType.TType.ToString();
        _hexDescTMP.text = tile.CurrentTileType.TypeDescription;

        int dist = TileController.Instance.FindDistanceThroughWater(TileController.Instance.ReferenceTile, tile);
        _hexIndexTMP.text = dist.ToString();



        if (tile.CurrentTileType.TileIcon == null)
        {
            _hexIcon.enabled = false;
        }
        else
        {
            _hexIcon.enabled = true;
            _hexIcon.sprite = tile.CurrentTileType.TileIcon;
        }

    }

    public void ClearHex()
    {
        _hexTypeTMP.text = " ";
        _hexDescTMP.text = " ";
        _hexIndexTMP.text = " ";
        _hexIcon.enabled = false;
    }

}
