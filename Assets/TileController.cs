using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour
{
    public static TileController Instance { get; private set; }


    //settings
    [SerializeField] Transform _hexMap = null;
    [SerializeField] TileHandler _tilePrefab = null;
    [SerializeField] int _gridWidth = 50;
    [SerializeField] int _gridHeight = 35;

    // Radius of the hexagon (center to vertex)
    [SerializeField] float _radius = 0.5f;


    [Header("Perlin")]
    [SerializeField] float _perlinZoom = 0.5f;

    //state
    System.Random _rnd;

    [SerializeField] float _xOffset;
    [SerializeField] float _yOffset;
    List<TileHandler> _tilesRaw = new List<TileHandler>();
    List<List<TileHandler>> _factionTiles = new List<List<TileHandler>>();
    List<List<TileHandler>> _growingFactions = new List<List<TileHandler>>();



    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        CreateTerrain();
    }

    private void CreateTerrain()
    {
        _rnd = new System.Random();
        _xOffset = (float)_rnd.NextDouble() * 100f;
        _yOffset = (float)_rnd.NextDouble() * -234f;

        _tilesRaw.Clear();
        _factionTiles.Clear();
        _growingFactions.Clear();

        LayTiles();
        InitializeTiles();

        SeedRandomFactions(15);

        SpreadRegions();
    }



    public void LayTiles()
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
                var tile = Instantiate(_tilePrefab, position, Quaternion.identity, _hexMap);
                _tilesRaw.Add(tile);
                tile.name = $"Tile_{_tilesRaw.Count}";

            }
        }
    }
    
    private void InitializeTiles()
    {
        foreach (var tile in _tilesRaw)
        {
            tile.InitializeTile();
        }
    }

    public void SeedRandomFactions(int factionsToSeed)
    {
        List<TileHandler> tilesUnfactioned = new List<TileHandler>(_tilesRaw);

        for (int i = 0; i < factionsToSeed; i++)
        {
            int rand = UnityEngine.Random.Range(0, tilesUnfactioned.Count);
            TileHandler newCapitol = tilesUnfactioned[rand];

            tilesUnfactioned.Remove(newCapitol);
            List<TileHandler> aFactionsTiles = new List<TileHandler>();
            aFactionsTiles.Add(newCapitol);
            _growingFactions.Add(aFactionsTiles);
            newCapitol.AssignFaction(i);
        }
    }

    private void SpreadRegions()
    {
        for (int currentFaction = 0; currentFaction < _growingFactions.Count; currentFaction++)
        {
            TileHandler tileToFaction = null;

            List<TileHandler> tilesInThisFaction = _growingFactions[currentFaction];

            TileHandler bestTileToGrowFrom = null;
            float factionValueToBeat = -9f;
            foreach (var bestTileCandidate in tilesInThisFaction)
            {
                bool hasUnfactionedNeighbors = false;
                foreach (var neighbor in bestTileCandidate.NeighborTiles)
                {
                    if (neighbor.FactionIndex == -1)
                    {
                        hasUnfactionedNeighbors = true;
                        break;
                    }
                }

                if (hasUnfactionedNeighbors && bestTileCandidate.Value > factionValueToBeat)
                {
                    bestTileToGrowFrom = bestTileCandidate;
                }
            }

            if (bestTileToGrowFrom == null)
            {
                _growingFactions.RemoveAt(currentFaction);
                //Debug.Log($"Faction {currentFaction} has no tiles to grow from. Factions still growing: " + _growingFactions.Count);
            }
            else
            {
                float unfactionedValueToBeat = -11f;
                foreach (var tileToTest in bestTileToGrowFrom.NeighborTiles)
                {
                    if (tileToTest.FactionIndex != -1)
                    {
                        continue;
                    }

                    if (tileToTest.GetNeighborlyScore(bestTileToGrowFrom.FactionIndex) > unfactionedValueToBeat)
                    {
                        unfactionedValueToBeat = tileToTest.GetNeighborlyScore(bestTileToGrowFrom.FactionIndex);
                        tileToFaction = tileToTest;
                    }
                }

                if (tileToFaction != null)
                {
                    tileToFaction.AssignFaction(bestTileToGrowFrom.FactionIndex);
                    tilesInThisFaction.Add(tileToFaction);
                }
                else
                {
                    _growingFactions.RemoveAt(currentFaction);
                    //Debug.Log($"Faction {currentFaction} has no tiles to grow into. Factions still growing: " + _growingFactions.Count);
                }
            }

            
        }

        if (_growingFactions.Count > 0)
        {
            Invoke(nameof(SpreadRegions), 0.25f);
        }
        else
        {
            Debug.Log("Region spreading complete");
        }
    }

    public float GetValueFactorAtPoint(Vector3 point)
    {
        return Mathf.PerlinNoise((point.x + _xOffset) * _perlinZoom, (point.y + _yOffset) * _perlinZoom);
    }
}
