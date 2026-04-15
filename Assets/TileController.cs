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
    [SerializeField] HexDriver _hexDriver = null;

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
    public TileHandler ReferenceTile;
    private void Awake()
    {
        Instance = this;
    }

    #region Worldbuilding
    public void CreateNewWorld()
    {
        ClearWorld();
        CreateWorld();
    }

    private void ClearWorld()
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

    private void CreateWorld()
    {
        _rnd = new System.Random();
        _xOffset = (float)_rnd.NextDouble() * 100f;
        _yOffset = (float)_rnd.NextDouble() * -234f;


        LayTiles();
        NeighborizeTiles();

        CheckForInaccessability();
        //SprinkleRandomResourceTiles

        SeedRandomFactions(FactionController.Instance.FactionCount);

        SpreadRegions();
    }

    private void CheckForInaccessability()
    {
        bool t = true;
        foreach(var startingTile in _tilesRaw)
        {
            foreach (var endingTile in _tilesRaw)
            {
                //Debug.Log($"{startingTile} to {endingTile}");
                int dist = FindDistanceToSpecialTile(startingTile, endingTile);

                if (dist < 0)
                {
                    t = false;
                    break;
                }

            }

            if (t == false)
            {
                break;
            }

        }

        if (t == false)
        {
            Debug.Log("Island detected");
        }
        else
        {
            Debug.Log("No Islands detected");
        }
    }

    private void Delay_RegionFinalization()
    {
        //InjectBalanceResourcedTiles();
    }

    private void InjectBalanceResourcedTiles()
    {
        int highestTerritory = 0;
        foreach (var faction in _factionTiles)
        {
            if (faction.Count > highestTerritory)
            {
                highestTerritory = faction.Count;
            }
        }

        float runningProduction;
        float productionDelta;
        int resourcedTilesToBestow;
        foreach (var faction in _factionTiles)
        {

            runningProduction = 0;
            productionDelta = 0;
            resourcedTilesToBestow = 0;

            runningProduction = faction.Count * FactionController.Instance.ProductionPerHex;
            productionDelta = (highestTerritory - faction.Count) * FactionController.Instance.ProductionPerHex;
            resourcedTilesToBestow = 1 + Mathf.RoundToInt(productionDelta);

            if (resourcedTilesToBestow <= 0)
            {
                continue;
            }

  

            List<TileHandler> tiles = new List<TileHandler>(faction);
    
            foreach (var tile in faction)
            {
                if (tile.CurrentTileType.TType != TileType.TileTypes.Plain)
                {
                    tiles.Remove(tile);
                }
            }

            if (resourcedTilesToBestow > tiles.Count)
            {
                resourcedTilesToBestow = tiles.Count;
            }

            tiles = Shuffle(tiles);

            for (int i = 0; i < resourcedTilesToBestow; ++i)
            {
                tiles[i].ResourceTile();
            }
            
        }
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

                tile.InitializeTile();
                _tilesRaw.Add(tile);
                tile.Index = _tilesRaw.Count;
                tile.name = $"Tile_{_tilesRaw.Count}";

            }
        }
    }


    private void NeighborizeTiles()
    {
        foreach (var tile in _tilesRaw)
        {
            tile.CheckForNeighbors();
        }
    }

    private void SeedRandomFactions(int factionsToSeed)
    {
        List<TileHandler> tilesUnfactioned = new List<TileHandler>(_tilesRaw);

        foreach (var tile in _tilesRaw)
        {
            if (tile.CurrentTileType.TType == TileType.TileTypes.Water)
            {
                tilesUnfactioned.Remove(tile);
            }
        }

        for (int i = 0; i < factionsToSeed; i++)
        {
            int rand = UnityEngine.Random.Range(0, tilesUnfactioned.Count);
            TileHandler newCapitol = tilesUnfactioned[rand];

            tilesUnfactioned.Remove(newCapitol);
            List<TileHandler> aFactionsTiles = new List<TileHandler>();
            _factionTiles.Add(aFactionsTiles);

            _factionTiles[i].Add(newCapitol);
            _isFactionGrowing.Add(true);

            newCapitol.AssignFactionToTile(i);
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
            if (_factionTiles[currentFaction].Count >= FactionController.Instance.FactionStartingSize)
            {
                _isFactionGrowing[currentFaction] = false;
            }

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
            Delay_RegionFinalization();
            HighlightFaction(-9);
        }
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

    #endregion

    #region Helpers
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
    public static List<TileHandler> Shuffle(List<TileHandler> list)
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

    public float GetValueFactorAtPoint(Vector3 point)
    {
        return Mathf.PerlinNoise((point.x + _xOffset) * _perlinZoom, (point.y + _yOffset) * _perlinZoom);
    }

    public int GetFactionTerritory(int factionIndex)
    {
        return _factionTiles[factionIndex].Count;
    }

    public float GetFactionProduction(int factionIndex)
    {
        float production = 0;

        foreach (var tile in _factionTiles[factionIndex])
        {
            production += FactionController.Instance.ProductionPerHex;
            production += tile.ResourceBonus;
        }

        return production;
    }

    #endregion

    #region Pathfinding
    public int FindDistanceToSpecialTile(TileHandler startingTile, TileHandler specialTile)
    {
        if (startingTile == null || specialTile == null ||
            startingTile.CurrentTileType.TType == TileType.TileTypes.Water ||
            specialTile.CurrentTileType.TType == TileType.TileTypes.Water)
        {
            return 0;
        }
            
        if (startingTile == specialTile )
        {
            return 0;
        }

        foreach (var tile in _tilesRaw)
        {
            tile.PreviousTile = null;
        }

        Stack<TileHandler> currentCheckPath = new Stack<TileHandler>();

        List<TileHandler> tilesChecked = new List<TileHandler>();
        Queue<TileHandler> tilesToCheck = new Queue<TileHandler>();

        tilesToCheck.Enqueue(startingTile);

        TileHandler tileBeingChecked = null;

        while (tilesToCheck.Count > 0)
        {

            tileBeingChecked = tilesToCheck.Dequeue();

            if (tileBeingChecked == specialTile)
            {
                break;
            }

            tilesChecked.Add(tileBeingChecked);

            //if (isBlockedByAgents && tileBeingChecked.Occupant != null && tileBeingChecked.Occupant.IsAgent)
            if (false == true)
            {
                //Debug.Log($"blocked at {tileBeingChecked.TileIndex}");

            }
            else
            {
                foreach (var tile in tileBeingChecked.OrderedNeighborTiles)
                {
                    if (tile == null || tilesChecked.Contains(tile) || tilesToCheck.Contains(tile))
                    {
                        continue;
                    }
                    else
                    {
                        tilesToCheck.Enqueue(tile);

                        if (tile.PreviousTile == null)
                        {
                            tile.PreviousTile = tileBeingChecked;
                        }

                    }

                }
            }


            //Debug.Log($"checked {tilesChecked.Count} tiles. {tilesToCheck.Count} in queue.");


            if (tilesChecked.Count > 500)
            {
                //Debug.Log("Break at 500!");
                break;
            }
        }

        currentCheckPath.Push(tileBeingChecked);
        TileHandler reverseWalker = null;

        int breaker = 40;
        while (reverseWalker != startingTile)
        {
            reverseWalker = currentCheckPath.Peek().PreviousTile;
            currentCheckPath.Push(reverseWalker);

            breaker--;
            if (breaker == 0)
            {
                //Debug.Log("path breaker");
                break;
            }
        }

        List<TileHandler> pathAsList = new List<TileHandler>(currentCheckPath);

        if (pathAsList.Contains(specialTile))
        {
            return pathAsList.Count - 1;
        }
        else
        {
            return -(pathAsList.Count - 1);
        }


    }
    #endregion

    #region Flow

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleLMBClick();
        }

        if (Input.GetMouseButtonDown(1))
        {
            HandleRMBClick();
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

    private void HandleRMBClick()
    {
        if (_tileUnderCursor)
        {
            ReferenceTile = _tileUnderCursor;
            Debug.Log("new reference: " + _tileUnderCursor.name);
        }
    }


    public void HandleMouseOverTile(TileHandler tuc)
    {
        if (tuc == null) return;

        _tileUnderCursor = tuc;

        _hexDriver.SetHex(_tileUnderCursor);

        HighlightFaction(_tileUnderCursor.FactionIndex);
        FactionController.Instance.DisplayFaction(_tileUnderCursor.FactionIndex);

        TileUnderCursorChanged?.Invoke();
    }

    public void HandleMouseExitTile()
    {
        _tileUnderCursor = null;

        _hexDriver.ClearHex();

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
        if (oldFaction >= 0 )
        {
            _factionTiles[oldFaction].Remove(tile);
        }

        if (newFaction >= 0 )
        {
            _factionTiles[newFaction].Add(tile);
        }

    }

    #endregion
}
