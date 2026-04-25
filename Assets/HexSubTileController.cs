using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HexSubTileController : MonoBehaviour
{
    public static HexSubTileController Instance { get; private set; }

    //settings
    [SerializeField] Transform _hexMap = null;
    [SerializeField] int _gridWidth = 50;
    [SerializeField] int _gridHeight = 35;
    [SerializeField] HexTileHandler _hexSubTilePrefab = null;
    [SerializeField] float _radius = 0.5f;

    //state
    List<HexTileHandler> _subTiles = new List<HexTileHandler>();

    private void Awake()
    {
        Instance = this;
    }

    [ContextMenu("Lay SubTiles")]
    public void LaySubTiles()
    {
        float hexWidth = Mathf.Sqrt(3) * _radius;
        float hexHeight = 2f * _radius;

        for (int col = 0; col < _gridWidth; col++)
        {
            for (int row = 0; row < _gridHeight; row++)
            {
                // Calculate position
                float xPos = col * hexWidth;
                // Offset odd columns
                float yPos = row * hexHeight;
                if (col % 2 != 0)
                {
                    yPos += hexHeight * 0.5f;
                }

                Vector3 position = new Vector3(xPos, yPos, 0);
                var tile = Instantiate(_hexSubTilePrefab, position, Quaternion.identity, _hexMap);

                //tile.InitializeHexTileHandler();
                _subTiles.Add(tile);
                //tile.Index = _subTiles.Count;
                tile.name = $"SubTile_{_subTiles.Count}";

            }
        }
    }

    public void ClearAllSubTiles()
    {
        if (_subTiles.Count > 0)
        {
            for (int i = _subTiles.Count - 1; i >= 0; i--)
            {
                Destroy(_subTiles[i].gameObject);
            }
        }
    }
}
