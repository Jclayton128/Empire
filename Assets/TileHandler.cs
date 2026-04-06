using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileHandler : MonoBehaviour
{
    //refs

    [SerializeField] List<Transform> _neighborChecks = new List<Transform>();
    [SerializeField] SpriteRenderer _tileFill = null;
    [SerializeField] Collider2D _coll = null;



    //state

    [SerializeField] int _factionIndex = -1;
    public int FactionIndex => _factionIndex;

    [SerializeField] List<TileHandler> _neighborTiles = new List<TileHandler>();
    public List<TileHandler> NeighborTiles => _neighborTiles;
    public float Value;



    public void InitializeTile()
    {
        Value = TileController.Instance.GetValueFactorAtPoint(transform.position);


        if (Value < 0.30f)
        {
            _tileFill.color = Color.clear;
            _coll.enabled = false;
        }


    }

    public void CheckForNeighbors()
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

        _neighborTiles = Shuffle(_neighborTiles);
    }

    public List<TileHandler>  Shuffle(List<TileHandler> list)
    {
        System.Random rnd = new System.Random();
        List<TileHandler> output = new List<TileHandler>(list);
        int n = output.Count;
        while (n > 1)
        {
            n--;
            int k = rnd.Next(n + 1);
            TileHandler value = output[k];
            output[k] = output[n];
            output[n] = value;
        }

        return output;
    }


    public float GetNeighborlyScore(int askingFaction)
    {

        //if (_factionIndex == -1)
        //{
        //    return noiseValue;
        //}

        float enemyNeighbors = 0;
        float neutralNeighbors = 0;
        float friendlyNeighbors = 0;

        foreach (var neighbor in _neighborTiles)
        {
            if (neighbor.FactionIndex == askingFaction)
            {
                friendlyNeighbors += 1f;
            }
            else if (neighbor.FactionIndex == -1)
            {
                neutralNeighbors += 1f;
            }
            else
            {
                enemyNeighbors += 1f;
            }
        }

        //return +friendlyNeighbors + neutralNeighbors - enemyNeighbors + Value;
        return friendlyNeighbors;
    }

    public void AssignFaction(int factionIndex)
    {
        _factionIndex = factionIndex;

        _tileFill.color = FactionController.Instance.GetFactionColor(factionIndex);
    }

  
}
