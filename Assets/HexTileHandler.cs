using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HexTileHandler : MonoBehaviour
{
    //refs

    [SerializeField] GameObject _landMesh = null;
    [SerializeField] GameObject _waterMesh = null;



    public void InitializeHexTileHandler(TileType.TileTypes tileType)
    {
        float baseValue = 0;
        float value = 0;

        if (tileType == TileType.TileTypes.Water)
        {
            value = baseValue;
            SetAsWater();
        }
        else
        {
            baseValue = 0 - (TileController.Instance.GetValueFactorAtPoint(transform.position) * 1f);
            float noiseValue = TileController.Instance.GetValueFactorAtPoint_Scaled(transform.position, 1f);
            value = baseValue + (noiseValue * .2f);
            SetAsLand();
        }

        transform.DOLocalMoveZ(value, 1f);
    }

    public void SetAsWater()
    {
        _landMesh.SetActive(false);
        _waterMesh.SetActive(true);
    }

    public void SetAsLand()
    {
        _landMesh.SetActive(true);
        _waterMesh.SetActive(false);
    }
}
