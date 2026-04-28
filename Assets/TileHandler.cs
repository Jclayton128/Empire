using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TileHandler : MonoBehaviour
{

    //refs

    [SerializeField] List<Transform> _neighborChecks = new List<Transform>();
    [SerializeField] SpriteRenderer _fullFill = null;
    [SerializeField] SpriteRenderer _outerRing = null;
    [SerializeField] SpriteRenderer _innerRing = null;


    [SerializeField] Collider2D _coll = null;
    [SerializeField] List<SpriteRenderer> _borders = null;

    //settings

    [SerializeField] Sprite _fortifyIcon = null;
    [SerializeField] Sprite _mountainIcon = null;
    [SerializeField] Sprite _resourcedIcon = null;
    [SerializeField] List<TileType> _tileMenu = null;

    [Header("Defend Action Parameters")]
    [SerializeField] int _defendBonus_Self = 3;
    [SerializeField] int _defendBonus_Adjacent = 1;

    [Header("Production Parameters")]

    //state

    [SerializeField] int _factionIndex = -1;
    public int FactionIndex => _factionIndex;

    [SerializeField] List<TileHandler> _orderedNeighborTiles = new List<TileHandler>();
    [SerializeField] List<TileHandler> _shuffledNeighborTiles = new List<TileHandler>();
    [SerializeField] List<HexTileHandler> _capturedSubtiles = new List<HexTileHandler>();
    public List<TileHandler> OrderedNeighborTiles => _orderedNeighborTiles;
    public List<TileHandler> ShuffledNeighborTiles => _shuffledNeighborTiles;
    public float ArbitraryNoiseValue;
    int _defendBonus = 1;
    public int DefendBonus => _defendBonus;
    int _attackBonus = 1;
    public int AttackBonus => _attackBonus;

    int _productionBonus = 0;
    public int ProductionBonus => _productionBonus;

    [SerializeField] TileType _currentTileType;
    public TileType CurrentTileType => _currentTileType;

    public TileHandler PreviousTile = null;
    public int Index;

    public void InitializeTile()
    {
        PreviousTile = null;
        Index = -1;
        ArbitraryNoiseValue = TileController.Instance.GetValueFactorAtPoint(transform.position);

        if (ArbitraryNoiseValue < 0.30f)
        {
            SetTileType(TileType.TileTypes.Water);

            //_coll.enabled = false;
        }

        //else if (ArbitraryNoiseValue > 0.7f)
        //{
        //    SetTileType(TileType.TileTypes.Mountain);
        //}

        else
        {
            SetTileType(TileType.TileTypes.Plain);
        }
    }

    public void CheckForNeighbors()
    {
        _orderedNeighborTiles.Clear();

        foreach (var check in _neighborChecks)
        {
            TileHandler neighbor;
            var hit = Physics2D.OverlapPoint(check.position, 1 << 6);
            if (hit && hit.TryGetComponent<TileHandler>(out neighbor))
            {
                _orderedNeighborTiles.Add(neighbor);
            }
            else
            {
                _orderedNeighborTiles.Add(null);
            }

        }

        _shuffledNeighborTiles = TileController.Shuffle(_orderedNeighborTiles);
    }

    public void CaptureSubtiles()
    {
        _capturedSubtiles.Clear();
        ContactFilter2D cf2d = new ContactFilter2D();
        cf2d.useLayerMask = true;
        cf2d.layerMask = 1 << 7;
        List<Collider2D> hits = new List<Collider2D>();
        var hitCount = Physics2D.OverlapCollider(_coll, cf2d, hits);
        if (hitCount > 0)
        {
            HexTileHandler th;
            foreach (var hit in hits)
            {
                th = hit.GetComponent<HexTileHandler>();
                th.InitializeHexTileHandler(CurrentTileType.TType);
                _capturedSubtiles.Add(th);
                
            }
        }
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

        foreach (var neighbor in _orderedNeighborTiles)
        {
            if (neighbor == null) continue;

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

    public void AssignFactionToTile(int factionIndex)
    {
        if (CurrentTileType.TType == TileType.TileTypes.Water)
        {
            Debug.Log("Cannot assign faction to water tiles");
            _factionIndex = -2;
            _fullFill.color = FactionController.Instance.GetFactionFillColor(factionIndex);
            return;
        }

        _factionIndex = factionIndex;
        _fullFill.color = FactionController.Instance.GetFactionFillColor(factionIndex);

    }

    public void HighlightBorders()
    {
        for (int i = 0; i < _neighborChecks.Count; i++)
        {
            if (_orderedNeighborTiles[i] == null)
            {
                _borders[i].color = Color.white;
            }
            else if (_orderedNeighborTiles[i].FactionIndex == _factionIndex)
            {
                _borders[i].color = Color.clear;
            }
            else if (_orderedNeighborTiles[i].FactionIndex != _factionIndex)
            {
                _borders[i].color = Color.white;
            }
        }

    }

    public void DehighlightBorders()
    {
        for (int i = 0; i < _neighborChecks.Count; i++)
        {
            if (_orderedNeighborTiles[i] == null)
            {
                _borders[i].color = FactionController.Instance.GetFactionBorderColor(_factionIndex);
            }
            else if (_orderedNeighborTiles[i].FactionIndex == _factionIndex)
            {
                _borders[i].color = Color.clear;
            }
            else if (_orderedNeighborTiles[i].FactionIndex != _factionIndex)
            {
                _borders[i].color = FactionController.Instance.GetFactionBorderColor(_factionIndex);
            }
        }
    }

    private void OnMouseEnter()
    {
        TileController.Instance.HandleMouseOverTile(this);
        _innerRing.color = Color.yellow;

    }

    private void OnMouseExit()
    {
        TileController.Instance.HandleMouseExitTile();
        _innerRing.color = Color.clear;
    }

    public bool ResourceTile()
    {
        SetTileType(TileType.TileTypes.Resourced);
        _productionBonus = 1;
        return true;
    }

    public bool AttemptDefendTile()
    {

        if (_currentTileType.TType == TileType.TileTypes.Water)
        {
            //cannot attack water
            return false;
        }


        if (_currentTileType.TType != TileType.TileTypes.Plain)
        {
            Debug.Log("Can only defend Plain tiles");
            return false;
        }
        else
        {
            //SetTileType(TileType.TileTypes.Fortified);

            //mega-Boost this tile's defense
            ModifyDefendBonus(_defendBonus_Self);
            
            //mini-boost neighboring friendly tile's defenses
            foreach (var tile in _orderedNeighborTiles)
            {
                if (tile == null || tile.FactionIndex != FactionIndex)
                {
                    continue;
                }
                else
                {
                    tile.ModifyDefendBonus(_defendBonus_Adjacent);
                }
            }

            return true;
        }
    }

    public void UndefendTile()
    {
        ModifyDefendBonus(-_defendBonus_Self);

        //mini-boost neighboring friendly tile's defenses
        foreach (var tile in _orderedNeighborTiles)
        {
            if (tile == null || tile.FactionIndex != FactionIndex)
            {
                continue;
            }
            else
            {
                tile.ModifyDefendBonus(-_defendBonus_Adjacent);
            }
        }
    }

    public void ModifyDefendBonus(int amountToAdd)
    {
        _defendBonus += amountToAdd;
        //if (_defendBonus > 0)
        //{
        //    _innerFill.sprite = _fortifyIcon;
        //}
        //else
        //{
        //    _innerFill.sprite = null;
        //}
    }

    public void ModifyAttackBonus(int amountToAdd)
    {
        _attackBonus += amountToAdd;
        if (_attackBonus > 0)
        {
            //_innerFill.sprite = _defendIcon;
        }
        else
        {
            //_innerFill.sprite = null;
        }
    }

    public void SetTileType(TileType.TileTypes tileType)
    {
        foreach (var t in _tileMenu)
        {
            if (t.TType == tileType)
            {
                _currentTileType = t;
                continue;
            }
        }

        if (tileType == TileType.TileTypes.Water)
        {
            //_fullFill.sprite = null;
            _factionIndex = -2;
            _fullFill.color = FactionController.Instance.GetFactionFillColor(_factionIndex);
        }

        //_innerFill.color = Color.black;
        //_innerFill.sprite = _currentTileType.TileIcon;
    }
}
