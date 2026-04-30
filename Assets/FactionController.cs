using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FactionController : MonoBehaviour
{
    public static FactionController Instance { get; private set; }

    //refs
    [SerializeField] FactionDriver _factionDriver = null;
    [SerializeField] EmpireDriver _playerEmpireDriver = null;


    //settings

    [SerializeField] int _factionCount = 4;
    [SerializeField] int _factionStartingSize = 4;
    public int FactionCount => _factionCount;
    public int FactionStartingSize => _factionStartingSize;

    [SerializeField] Color _oceanColor = Color.blue;
    [SerializeField] List<Color> _factionColors = new List<Color>();

    [SerializeField] int _factionStartingResources = 10;

    public int ActiveFaction => _playerFaction; //This will eventually be updated to reflect whatever's faction has the turn priority.

    //state

    [SerializeField] int _playerFaction = 0;
    public int PlayerFaction => _playerFaction;

    List<int> _resources;
    List<int> _unusedPopulation;

    private void Awake()
    {
         Instance = this;
    }

    public void SetupFactions()
    {
        _resources = new List<int>();
        _unusedPopulation = new List<int>();

        for (int i = 0; i < _factionCount; i++)
        {
            _resources.Add(_factionStartingResources);
            _unusedPopulation.Add(_factionStartingSize);
        }
        _playerEmpireDriver.ShowProduction(_resources[_playerFaction]);
        _playerEmpireDriver.ShowPopulation(_unusedPopulation[_playerFaction]);
    }


    public Color GetFactionBorderColor(int factionIndex)
    {
        if (factionIndex >= 0 && factionIndex < _factionColors.Count)
        {
            return _factionColors[factionIndex];
        }
        else
        {
            //Debug.LogWarning("this faction doesn't exist");

            //neutral or rebel factions don't have borders
            return Color.clear;
        }
    }

    public Color GetFactionFillColor(int factionIndex)
    {
        if (factionIndex >= 0 && factionIndex < _factionColors.Count)
        {
            Color col = _factionColors[factionIndex];
            col.r *= 0.5f;
            col.g *= 0.5f;
            col.b *= 0.5f;
            return col;
        }
        else if (factionIndex == -1)
        {

            //neutral or rebel faction fill
            return Color.gray;
        }
        else if (factionIndex == -2)
        {
            return _oceanColor;
        }
        else
        {
            Debug.LogWarning("this faction doesn't exist");
            return Color.white;
        }
    }

    public void SetPlayerFaction(int playerFaction)
    {
        _playerFaction = playerFaction;
        _playerEmpireDriver.SetPlayerEmpireName($"Player: {_playerFaction}");
        _playerEmpireDriver.ShowProduction(_resources[_playerFaction]);
    }

    public void DisplayFaction(int faction)
    {
        if (faction >= 0 && faction < _factionCount)
        {
            int territory = TileController.Instance.GetFactionTerritory(faction);
            float production = TileController.Instance.GetFactionProduction(faction);
            _factionDriver.SetFaction(faction, territory, production, 0);
        }
        else
        {
            _factionDriver.ClearFaction();
        }

    }


    #region Production

    public bool CheckIfAffordable(int resourceCost,int faction)
    {
        if (_resources[faction] >= resourceCost)
        {
            return true;
        }
        else return false;
    }

    public void AdjustResources(int amountToAdd, int faction)
    {
        _resources[faction] += amountToAdd;

        if (faction == _playerFaction)
        {
            _playerEmpireDriver.ShowProduction(_resources[faction]);
        }
    }

    public void GatherProductionAtEndOfTurn()
    {
        int resources = 0;
        for (int faction = 0; faction < _factionCount; faction++)
        {
            List<TileHandler> tiles = TileController.Instance.GetFactionTileList(faction);

            foreach (var tile in tiles)
            {
                resources += tile.ResourceBonus;
            }

            AdjustResources(resources, faction);
        }

    }

    #endregion

}
