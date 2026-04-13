using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class TileController : MonoBehaviour
{
    public static TileController Instance { get; private set; }
    public Action TileUnderCursorChanged;



    //settings
    [SerializeField] Transform _hexMap = null;
    [SerializeField] TileHandler _tilePrefab = null;
    [SerializeField] TextMeshPro _barycenterIndicatorPrefab = null;
    [SerializeField] int _gridWidth = 50;
    [SerializeField] int _gridHeight = 35;

    // Radius of the hexagon (center to vertex)
    [SerializeField] float _radius = 0.5f;
    [SerializeField] float _buildTime = 0.0625f;


    [Header("Perlin")]
    [SerializeField] float _perlinZoom = 0.5f;

    //state
    System.Random _rnd;

    [SerializeField] float _xOffset;
    [SerializeField] float _yOffset;
    List<TileHandler> _tilesRaw = new List<TileHandler>();
    List<List<TileHandler>> _factionTiles = new List<List<TileHandler>>();
    List<bool> _isFactionGrowing = new List<bool>();
    List<TextMeshPro> _barycenterIndicators = new List<TextMeshPro>();

    TileHandler _tileUnderCursor;
    public TileHandler TileUnderCursor => _tileUnderCursor;

    private void Awake()
    {
        Instance = this;
    }

 
    public void CreateNewWorld()
    {
        ClearTerrain();
        CreateTerrain();
    }

    private void ClearTerrain()
    {
        if (_tilesRaw.Count > 0)
        {
            for (int i = _tilesRaw.Count - 1; i >= 0; i--)
            {
                Destroy(_tilesRaw[i].gameObject);
            }
        }

        if (_barycenterIndicators.Count > 0)
        {
            for (int i = _barycenterIndicators.Count - 1; i >= 0; i--)
            {
                Destroy(_barycenterIndicators[i].gameObject);
            }
        }

        _barycenterIndicators.Clear();
        _tilesRaw.Clear();
        _factionTiles.Clear();
        _isFactionGrowing.Clear();
    }

    private void CreateTerrain()
    {
        _rnd = new System.Random();
        _xOffset = (float)_rnd.NextDouble() * 100f;
        _yOffset = (float)_rnd.NextDouble() * -234f;


        LayTiles();
        InitializeTiles();

        SeedRandomFactions(FactionController.Instance.FactionCount);

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

        foreach (var tile in _tilesRaw)
        {
            tile.CheckForNeighbors();
        }
    }

    private void SeedRandomFactions(int factionsToSeed)
    {
        List<TileHandler> tilesUnfactioned = new List<TileHandler>(_tilesRaw);

        for (int i = 0; i < factionsToSeed; i++)
        {
            int rand = UnityEngine.Random.Range(0, tilesUnfactioned.Count);
            TileHandler newCapitol = tilesUnfactioned[rand];

            tilesUnfactioned.Remove(newCapitol);
            List<TileHandler> aFactionsTiles = new List<TileHandler>();
            aFactionsTiles.Add(newCapitol);
            _factionTiles.Add(aFactionsTiles);
            newCapitol.AssignFactionToTile(i);
            _isFactionGrowing.Add(true);

            Vector3 pos = newCapitol.transform.position;
            var barycenter = Instantiate(_barycenterIndicatorPrefab, pos, Quaternion.identity);
            _barycenterIndicators.Add(barycenter);  
            barycenter.text = i.ToString();
        }
    }

    private void SpreadRegions()
    {
        int growingFactions = 0;
        for (int currentFaction = 0; currentFaction < _factionTiles.Count; currentFaction++)
        {
            if (_isFactionGrowing[currentFaction] == false)
            {
                continue;
            }

            Vector3 currentBarycenter = FindMoveBarycenter(currentFaction);

            growingFactions++;

            TileHandler tileToFaction = null;

            List<TileHandler> tilesInThisFaction = _factionTiles[currentFaction];

            TileHandler bestTileToGrowFrom = null;
            float factionValueToBeat = -25f;
            foreach (var bestTileCandidate in tilesInThisFaction)
            {
                bool hasUnfactionedNeighbors = false;
                foreach (var neighbor in bestTileCandidate.ShuffledNeighborTiles)
                {
                    if (neighbor == null) continue;

                    if (neighbor.FactionIndex == -1)
                    {
                        hasUnfactionedNeighbors = true;
                        break;
                    }
                }

                if (hasUnfactionedNeighbors && bestTileCandidate.ArbitraryNoiseValue > factionValueToBeat)
                {
                    bestTileToGrowFrom = bestTileCandidate;
                }
            }

            if (bestTileToGrowFrom == null)
            {
                _isFactionGrowing[currentFaction] = false;
                //_growingFactions.RemoveAt(currentFaction);
                //Debug.Log($"Faction {currentFaction} has no tiles to grow from. Factions still growing: " + _growingFactions.Count);
            }
            else
            {
                float unfactionedValueToBeat = -25f;
                foreach (var tileToTest in bestTileToGrowFrom.ShuffledNeighborTiles)
                {
                    if (tileToTest == null) continue;
                    
                    if (tileToTest.FactionIndex != -1)
                    {
                        continue;
                    }

                    //evaluate if this particular unfactioned tile is a good fit for this faction
                    float neighborlyScore = tileToTest.GetNeighborlyScore(bestTileToGrowFrom.FactionIndex);
                    float distanceToBarycenter = 0;

                    //...by checking whether the prospective barycenter would still be within the faction
                    Vector3 prospectiveBarycenter = FindProspectiveBarycenter(currentFaction, tileToTest.transform.position);
                    TileHandler barycenterTest = GetTileHandlerAtPoint(prospectiveBarycenter);
                    if (barycenterTest)
                    {
                        if (barycenterTest.FactionIndex != currentFaction && barycenterTest != tileToTest)
                        {
                            distanceToBarycenter = (prospectiveBarycenter - tileToTest.transform.position).magnitude;
                        }
                        else
                        {
                            distanceToBarycenter = (prospectiveBarycenter - tileToTest.transform.position).magnitude;
                        }
                    }
                    else
                    {
                        //Debug.Log("No barycenter tile");
                        distanceToBarycenter = (prospectiveBarycenter - tileToTest.transform.position).magnitude;
                        //distanceToBarycenter = (prospectiveBarycenter - tileToTest.transform.position).magnitude;
                    }
                        

                    if ((neighborlyScore - (distanceToBarycenter/1f)) > unfactionedValueToBeat)
                    {
                        unfactionedValueToBeat = tileToTest.GetNeighborlyScore(bestTileToGrowFrom.FactionIndex);
                        tileToFaction = tileToTest;
                    }
                }

                if (tileToFaction != null)
                {
                    tileToFaction.AssignFactionToTile(bestTileToGrowFrom.FactionIndex);
                    tilesInThisFaction.Add(tileToFaction);
                }
                else
                {
                    _isFactionGrowing[currentFaction] = false;

                    //_growingFactions.RemoveAt(currentFaction);
                    //Debug.Log($"Faction {currentFaction} has no tiles to grow into. Factions still growing: " + _growingFactions.Count);
                }
            }

            
        }

        if (growingFactions > 0)
        {
            Invoke(nameof(SpreadRegions), _buildTime);
        }
        else
        {
            Debug.Log("Region spreading complete");
            HighlightFaction(-9);
        }
    }

    public float GetValueFactorAtPoint(Vector3 point)
    {
        return Mathf.PerlinNoise((point.x + _xOffset) * _perlinZoom, (point.y + _yOffset) * _perlinZoom);
    }

    private Vector3 FindMoveBarycenter(int factionIndex)
    {
        Vector3 barycenter = Vector3.zero;

        List<TileHandler> tilesToAverage = new List<TileHandler>(_factionTiles[factionIndex]);

        foreach (var tile in tilesToAverage)
        {
            barycenter += tile.transform.position;
        }

        barycenter /= tilesToAverage.Count;

        _barycenterIndicators[factionIndex].transform.position = barycenter;

        return barycenter;
    }

    private Vector3 FindProspectiveBarycenter(int factionIndex, Vector3 newPoint)
    {
        Vector3 prospectiveBarycenter = Vector3.zero;

        List<TileHandler> tilesToAverage = new List<TileHandler>(_factionTiles[factionIndex]);

        foreach (var tile in tilesToAverage)
        {
            prospectiveBarycenter += tile.transform.position;
        }

        prospectiveBarycenter += newPoint;

        prospectiveBarycenter /= (tilesToAverage.Count + 1);

        return prospectiveBarycenter;
    }

    private TileHandler GetTileHandlerAtPoint(Vector2 point)
    {
        //Debug.Log($"Looking for tile at {point}");
        TileHandler tile;
        var hit = Physics2D.OverlapCircle(point, 0.05f, 1 << 6);
        if (hit && hit.TryGetComponent<TileHandler>(out tile))
        {
            return tile;
        }
        else return null;
    }


    public int GetFactionTerritoryCount(int factionIndex)
    {
        return _factionTiles[factionIndex].Count;
    }

    #region Flow

    private void Update()
    {


        if (Input.GetMouseButtonDown(0))
        {
            HandleLMBClick();
        }
    }

    private void HandleLMBClick()
    {
        if (_tileUnderCursor)
        {
            ToolController.Instance.HandleClickOnTile(_tileUnderCursor);
            FactionController.Instance.DisplayFaction(_tileUnderCursor.FactionIndex);
        }
    }


    public void HandleMouseOverTile(TileHandler tuc)
    {
        if (tuc == null) return;

        _tileUnderCursor = tuc;
        HighlightFaction(_tileUnderCursor.FactionIndex);
        FactionController.Instance.DisplayFaction(_tileUnderCursor.FactionIndex);
        TileUnderCursorChanged?.Invoke();
    }

    public void HandleMouseExitTile()
    {
        _tileUnderCursor = null;
        HighlightFaction(-9);
        FactionController.Instance.DisplayFaction(-9);
        TileUnderCursorChanged?.Invoke();
    }

    public void HighlightFaction(int factionIndexToHighlight)
    {
        for (int factionIndex = 0; factionIndex < _factionTiles.Count; factionIndex++)
        {
            if (factionIndex == factionIndexToHighlight)
            {
                foreach (var tile in _factionTiles[factionIndex])
                {
                    tile.HighlightBorders();
                }
            }
            else
            {
                foreach (var tile in _factionTiles[factionIndex])
                {
                    tile.DehighlightBorders();
                }
            }
        }

    }

    public void ChangeTileFaction(TileHandler tile, int oldFaction, int newFaction)
    {
        _factionTiles[oldFaction].Remove(tile);
        _factionTiles[newFaction].Add(tile);
    }

    #endregion
}
