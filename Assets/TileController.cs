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
    List<TileHandler> _tilesUnfactioned = new List<TileHandler>();
    List<TileHandler> _capitolTiles = new List<TileHandler>();


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
        _tilesUnfactioned.Clear();
        _capitolTiles.Clear();

        LayTiles();
        InitializeTiles();

        SeedRandomCapitols(7);

        SpreadRegionsFromCapitols();
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
                var tile = Instantiate(_tilePrefab, position, Quaternion.identity, transform);
                _tilesRaw.Add(tile);

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

    public void SeedRandomCapitols(int capitolsToSeed)
    {
        _tilesUnfactioned = new List<TileHandler>(_tilesRaw);

        for (int i = 0; i < capitolsToSeed; i++)
        {
            int rand = UnityEngine.Random.Range(0, _tilesUnfactioned.Count);
            TileHandler newCapitol = _tilesUnfactioned[rand];

            _tilesUnfactioned.Remove(newCapitol);
            _capitolTiles.Add(newCapitol);

            newCapitol.AssignFaction(_capitolTiles.Count);
        }

    }

    private void SpreadRegionsFromCapitols()
    {
        List<TileHandler> newCapitols = new List<TileHandler>();
        foreach (var capitol in _capitolTiles)
        {
            List<TileHandler> nt = new List<TileHandler>();
            foreach (var tile in capitol.NeighborTiles)
            {
                if (tile.FactionIndex == -1)
                {
                    nt.Add(tile);
                }
            }

            if (nt.Count > 0)
            {
                int rand = UnityEngine.Random.Range(0, nt.Count);
                var tile = nt[rand];
                tile.AssignFaction(capitol.FactionIndex);
                newCapitols.Add(tile);
            }
        }

        _capitolTiles.Clear();
        _capitolTiles = new List<TileHandler>(newCapitols);

        if (_capitolTiles.Count > 1)
        {
            Invoke(nameof(SpreadRegionsFromCapitols), 0.5f);
        }
        else
        {
            Debug.Log("Region spreading complete");
        }
    }

    public float GetSeabedDepthFactorAtPoint(Vector3 point)
    {
        return Mathf.PerlinNoise((point.x + _xOffset) * _perlinZoom, (point.y + _yOffset) * _perlinZoom);
    }
}
