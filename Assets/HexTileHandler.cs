using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HexTileHandler : MonoBehaviour
{
    //refs

    [SerializeField] GameObject _landMesh = null;
    [SerializeField] GameObject _waterMesh = null;

    public void InitializeHexTileHandler()
    {
        
        float baseValue = TileController.Instance.GetValueFactorAtPoint(transform.position);
        float noiseValue = TileController.Instance.GetValueFactorAtPoint_Scaled(transform.position, 1f);
        float value = baseValue + noiseValue;
        transform.DOLocalMoveZ(value, 1f);

        if (value < 0.3f)
        {
            SetAsWater();
        }
        else
        {
            SetAsLand();
        }
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
