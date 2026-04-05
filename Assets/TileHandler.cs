using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileHandler : MonoBehaviour
{
    //refs

    [SerializeField] List<Transform> _neighborChecks = new List<Transform>();
    [SerializeField] SpriteRenderer _tileFill = null;



    //state

    [SerializeField] int _factionIndex = -1;
    public int FactionIndex => _factionIndex;

    [SerializeField] List<TileHandler> _neighborTiles = new List<TileHandler>();
    public List<TileHandler> NeighborTiles => _neighborTiles;


    public void InitializeTile()
    {
        _neighborTiles.Clear();

        foreach (var check in _neighborChecks)
        {
            TileHandler neighbor;
            var hit = Physics2D.OverlapPoint(check.position, 1 << 6);
            if (hit && hit.TryGetComponent<TileHandler>(out neighbor))
            {
                _neighborTiles.Add(neighbor);
            }

        }

    }

    public void AssignFaction(int factionIndex)
    {
        _factionIndex = factionIndex;

        _tileFill.color = FactionController.Instance.GetFactionColor(factionIndex);
    }
}
